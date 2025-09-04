using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Addressables;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 都市モデルをDynamicTile用にプレハブ化し、一括でアセットバンドルとして出力する。
    /// </summary>
    public class DynamicTileExporter : IPostTileImportProcessor
    {

        private const string AddressableLabel = "DynamicTile";
        private const string AddressableAddressBase = "PLATEAUTileMeta";
        
        
        public DynamicTileProcessingContext Context { get; private set; }
        private IProgressDisplay progressDisplay;
        
        public DynamicTileExporter(IProgressDisplay progressDisplay)
        {
            this.progressDisplay = progressDisplay;
        }

        /// <summary>
        /// 動的タイルの事前処理を行います。
        /// 成否を返します。
        /// </summary>
        public bool SetupPreProcessing(CityImportConfig cityConfig)
        {
            if (cityConfig == null)
            {
                Debug.LogError("CityImportConfigがnullです。");
                return false;
            }

            var config = cityConfig.DynamicTileImportConfig;

            PLATEAUEditorEventListener.disableProjectChangeEvent = true; // タイル生成中フラグを設定
            var succeeded = false;
            try
            {
                // DynamicTile管理用Managerを破棄
                var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
                if (manager != null)
                {
                    GameObject.DestroyImmediate(manager.gameObject);
                }

                Context = new DynamicTileProcessingContext(config);

                // プロジェクト内であればアセットバンドルをStreamingAssets以下に出力します。
                // なぜならデフォルトのローカルビルドパスであるLibrary以下は、2回目にプロジェクト外に出力した時にクリアされカタログが読めなくなるためです。
                // プロジェクト外であればユーザー指定のフォルダをそのまま使用します。

                // プロファイルを作成
                var profileID = AddressablesUtility.SetOrCreateProfile();
                if (string.IsNullOrEmpty(profileID))
                {
                    Debug.LogError("プロファイルの作成に失敗しました。");
                    return false;
                }

                // ビルド先パスを決定
                string bundleOutputPath;
                if (Context.IsExcludeAssetFolder)
                {
                    // プロジェクト外: ユーザー指定のフォルダーをそのまま使用
                    bundleOutputPath = Context.BuildFolderPath;
                }
                else
                {
                    // プロジェクト内: StreamingAssets/PLATEAUBundles/{GroupName}
                    bundleOutputPath = Path.Combine(
                        Application.streamingAssetsPath,
                        AddressableLoader.AddressableLocalBuildFolderName,
                        Context.AddressableGroupName);
                    bundleOutputPath = PathUtil.FullPathToAssetsPath(bundleOutputPath);
                    Context.BuildFolderPath = bundleOutputPath;
                }

                // ビルド設定を行います。
                AddressablesUtility.SetRemoteProfileSettings(bundleOutputPath, Context.AddressableGroupName);
                AddressablesUtility.SetGroupLoadAndBuildPath(Context.AddressableGroupName);

                if (!Context.IsValid())
                {
                    Debug.LogError("context is invalid.");
                    return false;
                }
                
                // 同じフォルダに複数回タイル化したとき、前と位置を合わせるため、既存メタのReferencePointを反映します（存在すれば）
                SetReferencePointSameAsOldMetaIfExist(cityConfig);
                
                succeeded = true;
                return true; 
            }
            finally
            {
                // 失敗時は常にフラグを戻す（成功時は CompleteProcessing 側の finally で戻す）
                if (!succeeded)
                {
                    PLATEAUEditorEventListener.disableProjectChangeEvent = false;
                }
            }
        }

        private static GameObject PrepareAndConvert(
            ConvertToAssetConfig config,
            string saveFolderPath,
            Action<string> onError)
        {
            var assetPath = config.AssetPath;
            List<GameObject> convertObjects = null;
            
            try
            {
                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                }

                // 変換処理が要求するフルパスを設定
                config.SetByFullPath(saveFolderPath);

                // 変換
                convertObjects = new ConvertToAsset().ConvertCore(config, new DummyProgressBar());
                
                // アセットパスを戻す
                config.AssetPath = assetPath;
            }
            catch (Exception ex)
            {
                onError?.Invoke($"変換処理中にエラーが発生しました: {ex}");
                config.AssetPath = assetPath; // エラー時もパスを元に戻す
                return null;
            }
            
            if (convertObjects != null && convertObjects.Count > 0)
            {
                return convertObjects[0];
            }
            return null;
        }

        /// <summary>
        /// メタデータを保存し、Addressableとして登録する
        /// </summary>
        /// <returns>登録されたmetaStoreのアドレスを返します</returns>
        private static string SaveAndRegisterMetaData(PLATEAUDynamicTileMetaStore metaStore, string assetPath, string groupName)
        {
            if (metaStore == null)
            {
                Debug.LogWarning("メタデータがnullです。");
                return null;
            }

            // metaStoreの名前をグループ名に基づいて変更
            string shorterGroupName = groupName.Replace(DynamicTileProcessingContext.AddressableGroupBaseName + "_", "");
            string addressName = $"{AddressableAddressBase}_{shorterGroupName}";
            
            // assetPathが既に相対パスであることを確認し、必要に応じて変換
            string normalizedAssetPath = AssetPathUtil.NormalizeAssetPath(assetPath);

            string dataPath = Path.Combine(normalizedAssetPath, addressName + ".asset");
            // Path.Combineは環境によってバックスラッシュを使うため、フォワードスラッシュに統一
            dataPath = dataPath.Replace('\\', '/');

            // 既存アセットとの衝突を回避
            // dataPath = AssetDatabase.GenerateUniqueAssetPath(dataPath);

            // 既に存在する場合は新規作成を行わない（前と同じフォルダに追加で生成するケースが該当）
            var existing = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(metaStore, dataPath);
                AssetDatabase.SaveAssets();
            }

            // メタデータをAddressableに登録
            AddressablesUtility.RegisterAssetAsAddressable(
                dataPath,
                addressName,
                groupName,
                new List<string> { AddressableLabel });
            return addressName;
        }

        /// <summary>
        /// DynamicTileの完了処理を行います（メタストア保存、Addressable処理、マネージャー設定）
        /// 成否を返します。
        /// </summary>
        public bool CompleteProcessing()
        {
            if (Context == null || !Context.IsValid())
            {
                Debug.LogError("DynamicTileProcessingContextが無効です。");
                return false;
            }

            try
            {
                // 保存先にすでにメタデータがあるなら、上書きではなく追加します
                AddTilesToOldMetaIfOldMetaExist();
                
                // メタデータを保存
                var metaAddress = SaveAndRegisterMetaData(Context.MetaStore, Context.AssetConfig.AssetPath, Context.AddressableGroupName);
                

                // アセットバンドルのビルド時に「シーンを保存しますか」とダイアログが出てくるのがうっとうしいので前もって保存して抑制します。
                // 保存については処理前にダイアログでユーザーに了承を得ています。
                EditorSceneManager.SaveOpenScenes();                

                // Addressablesのビルドを実行
                AddressablesUtility.BuildAddressables(false);
                
                AddressablesUtility.BackToDefaultProfile();

                // if (Context.IsExcludeAssetFolder)
                // {
                //     AddressablesUtility.RemoveGroup(Context.AddressableGroupName);
                //     AssetDatabase.SaveAssets();
                // }
                

                // managerを生成
                var managerObj = new GameObject("DynamicTileManager");
                var manager = managerObj.AddComponent<PLATEAUTileManager>();

                // 最新のカタログファイルのパスを取得（Asset相対/フルの両方に対応）
                var catalogSearchDir = Path.IsPathRooted(Context.BuildFolderPath)
                ? Context.BuildFolderPath
                : AssetPathUtil.GetFullPath(Context.BuildFolderPath);
                var catalogFiles = Directory.GetFiles(catalogSearchDir, "catalog_*.json")
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .ToArray();
                if (catalogFiles.Length == 0)
                {
                    Debug.LogError("カタログファイルが見つかりません");
                    return false;
                }

                var catalogPath = catalogFiles[0];
                manager.SaveCatalogPath(catalogPath);

                // 一時フォルダーを削除
                CleanupTempFolder();

                // タイルのある場所にシーンビューカメラをフォーカスします。
                manager.InitializeTiles().Wait();
                FocusSceneViewCameraToTiles(manager);
                
                // 上で自動保存しておてメタアドレスを保存しないのは中途半端なのでここでも保存します。
                EditorSceneManager.SaveOpenScenes();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"動的タイルのエクスポート中にエラーが発生しました: {ex.Message}");
                return false;
            }
            finally
            {
                PLATEAUEditorEventListener.disableProjectChangeEvent = false; // タイル生成中フラグを設定

                var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
                if (manager != null)
                {
                    PLATEAUSceneViewCameraTracker.Initialize();
                    manager.InitializeTiles().Wait(); // タイルの初期化
                }

                // シーンをEdit
                var scene = EditorSceneManager.GetActiveScene();
                if (!scene.isDirty)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }

        /// <summary>
        /// 一時フォルダーを削除します
        /// </summary>
        public static void CleanupTempFolder()
        {
            // FIXME 現状、Assets外のアプリビルド後に動かすためにはここはコメントアウトする必要あり
            
            // var assetPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            // if (AssetDatabase.DeleteAsset(assetPath))
            // {
            //     AssetDatabase.Refresh();
            //     Debug.Log($"一時フォルダーを削除しました: {assetPath}");
            // }
            // else
            // {
            //     Debug.Log($"一時フォルダーなし: {assetPath}"); // Assets内のケース
            // }
        }

        /// <summary>
        /// 進行中のAddressable化をキャンセルします。
        /// </summary>
        public void Cancel()
        {
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
            // Contextの破棄
            if (Context != null)
            {
                // 作成途中のAddressableグループを削除
                if (!string.IsNullOrEmpty(Context.AddressableGroupName))
                {
                    AddressablesUtility.RemoveGroup(Context.AddressableGroupName);
                }

                // 一時フォルダーを削除
                CleanupTempFolder();
            }
        }

        // IPostTileImportProcessor による処理
        #region IPostTileImportProcessor

        /// <summary>
        /// インポートしつつ動的タイルにするモードにおいて、GML1つがインポートされた後に呼ばれます。
        /// 動的タイルにします。
        /// <see cref="IPostTileImportProcessor"/> のコールバック実装です。
        /// </summary>
        public void OnTileImported(TileImportResult importResult)
        {
            if (Context == null || !Context.IsValid())
            {
                Debug.LogError("DynamicTileProcessingContextが無効です。SetupPreProcessingが呼ばれているか確認してください。");
                return;
            }

            var placedObject = importResult.RootObject;
            var zoomLevel = importResult.ZoomLevel;
            int totalGmlCount = importResult.TotalGmlCount;
            if (placedObject == null) return;
            Context.GmlCount = totalGmlCount;

            if (zoomLevel == 11)
            {
                var childObjects = placedObject.transform.Cast<Transform>()
                                           .Select(t => t.gameObject)
                                           .ToArray();
                // 複数GameObject処理
                ProcessGameObjects(
                    childObjects,
                    zoomLevel,
                    Context,
                    errorMessage => Debug.LogError($"DynamicTileExporter ProcessGameObject error: {errorMessage}"));

                // ProcessGameObjectsで、childObjectsはProcessGameObjects内で削除されるがParentが残るため、ここで削除
                GameObject.DestroyImmediate(placedObject);
            }
            else
            {
                // 処理
                ProcessGameObject(
                    placedObject,
                    zoomLevel,
                    Context,
                    errorMessage => Debug.LogError($"DynamicTileExporter ProcessGameObject error: {errorMessage}"));
            }
        }

        /// <summary>
        /// GameObject複数を処理し、プレハブ化・Addressable登録を行う
        /// </summary>
        /// <param name="targets">処理対象のゲームオブジェクト</param>
        /// <param name="zoomLevel">ズームレベル</param>
        /// <param name="context">コンテキスト</param>
        /// <param name="onError">エラー時のコールバック</param>
        /// <returns>処理が成功した場合はtrue</returns>
        private bool ProcessGameObjects(
            IEnumerable<GameObject> targets,
            int zoomLevel,
            DynamicTileProcessingContext context,
            Action<string> onError = null)
        {
            bool allSucceeded = true;
            foreach (var target in targets)
            {
                if (target == null) continue;
                var success = ProcessGameObject(
                    target,
                    zoomLevel,
                    context,
                    onError);
                allSucceeded &= success;
            }
            return allSucceeded;
        }

        /// <summary>
        /// GameObject１つを処理し、プレハブ化・Addressable登録を行う
        /// </summary>
        /// <param name="target">処理対象のゲームオブジェクト</param>
        /// <param name="zoomLevel">ズームレベル</param>
        /// <param name="context">コンテキスト</param>
        /// <param name="onError">エラー時のコールバック</param>
        /// <returns>処理が成功した場合はtrue</returns>
        private bool ProcessGameObject(
            GameObject target,
            int zoomLevel,
            DynamicTileProcessingContext context,
            Action<string> onError = null)
        {
            if (target == null)
            {
                Debug.LogWarning($"GameObjectがnullです。");
                return false;
            }

            string outputPath = context.AssetConfig?.AssetPath ?? "Assets/PLATEAUPrefabs/";
            var outputDirFullPath = Path.IsPathRooted(outputPath)
                ? outputPath
                : AssetPathUtil.GetFullPath(outputPath);
            // ディレクトリの存在確認（フルパスで）
            if (!Directory.Exists(outputDirFullPath))
            {
                Directory.CreateDirectory(outputDirFullPath);
            }

            // 進捗更新を通知
            // 進捗計算: 10%から始まり、GML処理完了ごとに進む（最大80%）
            int loadedGmlCount = context.IncrementAndGetLoadedGmlCount();
            float progress = 10f + ((float)loadedGmlCount / context.GmlCount) * 70f;
            progressDisplay?.SetProgress(ImportToDynamicTile.TileProgressTitle, progress,
                $"動的タイルを生成中... {target.name}");

            context.AssetConfig.SrcGameObj = target;

            var baseFolderAssetPath = Path.Combine(context.AssetConfig.AssetPath, target.name);
            var saveFolderAssetPath = AssetPathUtil.CreateDirectoryWithIncrementalNameIfExist(baseFolderAssetPath);
            var saveFolderFullPath = AssetPathUtil.GetFullPath(saveFolderAssetPath);

            var convertedObject = PrepareAndConvert(context.AssetConfig, saveFolderFullPath, onError);
            if (convertedObject == null)
            {
                Debug.LogWarning($"{target.name} の変換に失敗しました。");
                return false;
            }

            string prefabPath = saveFolderAssetPath + ".prefab";
            var prefabAsset = PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"{convertedObject.name} プレハブの保存に失敗しました。");
                return false;
            }

            var denominator = GetDenominatorFromZoomLevel(zoomLevel);
            if (denominator == 0)
            {
                Debug.LogWarning($"未対応のズームレベルです: {zoomLevel}");
                GameObject.DestroyImmediate(convertedObject);
                return false;
            }

            var prefabData = MultiResolutionPrefabCreator.CreateFromGameObject(convertedObject, context.AssetConfig.AssetPath, denominator, zoomLevel, true);
            if (prefabData == null)
            {
                Debug.LogWarning($"{convertedObject.name} の低解像度プレハブ生成に失敗しました。");
                GameObject.DestroyImmediate(convertedObject);
                return false;
            }
            RegisterAsset(prefabAsset, prefabPath, prefabData, context.AddressableGroupName, context.MetaStore);

            // シーン上のオブジェクトを削除
            GameObject.DestroyImmediate(convertedObject);

            return true;
        }

        /// <summary>
        /// アセット登録
        /// </summary>
        /// <param name="results"></param>
        /// <param name="groupName"></param>
        /// <param name="cityObject"></param>
        /// <param name="metaStore"></param>
        private void RegisterAsset(
            GameObject prefab,
            string savePath,
            MultiResolutionPrefabCreator.Result result,
            string groupName,
            PLATEAUDynamicTileMetaStore metaStore)
        {
            string prefabPath;
            if (string.IsNullOrEmpty(savePath))
            {
                // フォールバック: Prefab から AssetPath を取得
                prefabPath = AssetDatabase.GetAssetPath(prefab);
            }
            else
            {
                prefabPath = Path.IsPathRooted(savePath)
                ? AssetPathUtil.GetAssetPath(savePath)      // フルパス → Assetパス
                : AssetPathUtil.NormalizeAssetPath(savePath); // 既にAssetパス
            }
            var bounds = result.Bounds;
            var zoomLevel = result.ZoomLevel;

            // プレハブをAddressableに登録
            var address = prefab != null ? prefab.name : Path.GetFileNameWithoutExtension(prefabPath);

            AddressablesUtility.RegisterAssetAsAddressable(
                prefabPath,
                address,
                groupName,
                new List<string> { AddressableLabel });

            Debug.Log($"プレハブをAddressableに登録しました: {address} path : {prefabPath}");

            // メタ情報を登録
            metaStore.AddMetaInfo(address, bounds, 0, zoomLevel);

        }

        /// <summary>
        /// ズームレベルから解像度の分母を取得します。
        /// [9: 1/4, 10: 1/2, 11: 1/1]
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static int GetDenominatorFromZoomLevel(int zoomLevel)
        {
            return zoomLevel switch
            {
                9 => 4,
                10 => 2,
                11 => 1,
                _ => 0 // 未対応
            };
        }

        #endregion 

        //　IPostGmlImportProcessorによる処理(レガシー)
        #region IPostGmlImportProcessor

        /// <summary>
        /// インポートしつつ動的タイルにするモードにおいて、GML1つがインポートされた後に呼ばれます。
        /// 動的タイルにします。
        /// <see cref="IPostGmlImportProcessor"/>を実装するものです。
        /// </summary>
        public void OnGmlImported(GmlImportResult importResult)
        {
            var placedObjects = importResult.GeneratedObjects;
            var meshCode = importResult.GridCode;
            int totalGmlCount = importResult.TotalGmlCount;
            if (placedObjects == null || !placedObjects.Any() || Context == null || !Context.IsValid()) return;
            Context.GmlCount = totalGmlCount;

            // 実際の処理委譲
            ProcessCityObjects(
                placedObjects,
                Context,
                meshCode);
        }

        /// <summary>
        /// 新しい都市オブジェクト処理メソッド（コールバック付き）
        /// </summary>
        private void ProcessCityObjects(
            List<GameObject> placedObjects,
            DynamicTileProcessingContext context,
            string meshCode)
        {
            if (placedObjects == null || !placedObjects.Any() || context == null || !context.IsValid()) return;

            // 都市オブジェクトを取得
            var cityObjectGroups = placedObjects
                .Select(obj => obj.GetComponent<PLATEAUCityObjectGroup>())
                .Where(group => group != null)
                .ToList();

            if (cityObjectGroups.Count == 0) return;

            string outputPath = context.AssetConfig?.AssetPath ?? "Assets/PLATEAUPrefabs/";
            var outputDirFullPath = Path.IsPathRooted(outputPath)
                            ? outputPath
                            : AssetPathUtil.GetFullPath(outputPath);

            // ディレクトリの存在確認
            if (!Directory.Exists(outputDirFullPath))
            {
                Directory.CreateDirectory(outputDirFullPath);
            }

            // 進捗表示
            int loadedGmlCount = Context.IncrementAndGetLoadedGmlCount();
            float progress = 10f + ((float)loadedGmlCount / Context.GmlCount) * 70f;
            for (int i = 0; i < cityObjectGroups.Count; i++)
            {
                var cityObject = cityObjectGroups[i];
                if (cityObject == null) continue;

                // オブジェクト名にメッシュコードを追加
                cityObject.name = meshCode + "_" + cityObject.name;

                // プログレスバー更新
                progressDisplay?.SetProgress(ImportToDynamicTile.TileProgressTitle, progress,
                    $"動的タイルを生成中... {cityObject.name}");

                // 個別のオブジェクトを処理
                ProcessCityObject(
                    cityObject,
                    context.AssetConfig,
                    context.AddressableGroupName,
                    context.MetaStore,
                    errorMessage => Debug.LogError($"DynamicTileExporter error: {errorMessage}")
                );
            }
        }

        /// <summary>
        /// 都市オブジェクト１つを処理し、プレハブ化・Addressable登録を行う
        /// </summary>
        /// <param name="cityObject">処理対象の都市オブジェクト</param>
        /// <param name="assetConfig">変換設定</param>
        /// <param name="groupName">Addressableグループ名</param>
        /// <param name="metaStore">メタデータストア</param>
        /// <param name="onError">エラー時のコールバック</param>
        /// <returns>処理が成功した場合はtrue</returns>
        public static bool ProcessCityObject(
            PLATEAUCityObjectGroup cityObject,
            ConvertToAssetConfig assetConfig,
            string groupName,
            PLATEAUDynamicTileMetaStore metaStore,
            Action<string> onError = null)
        {
            if (cityObject == null || cityObject.gameObject == null)
            {
                Debug.LogWarning($"GameObjectがnullです。");
                return false;
            }

            assetConfig.SrcGameObj = cityObject.gameObject;

            var baseFolderAssetPath = Path.Combine(assetConfig.AssetPath, $"{cityObject.gameObject.name}_11");
            var saveFolderAssetPath = AssetPathUtil.CreateDirectoryWithIncrementalNameIfExist(baseFolderAssetPath);

            var saveFolderFullPath = AssetPathUtil.GetFullPath(saveFolderAssetPath);
            var convertedObject = PrepareAndConvert(assetConfig, saveFolderFullPath, onError);
            if (convertedObject == null)
            {
                Debug.LogWarning($"{cityObject.gameObject.name} の変換に失敗しました。");
                return false;
            }

            string prefabPath = saveFolderAssetPath + ".prefab";
            var prefabAsset = PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"{convertedObject.name} プレハブの保存に失敗しました。");
                return false;
            }

            //低解像度のプレハブを生成 (Tile生成処理追加後に仕様が変わるのでとりあえずベタ実装）
            var prefab1Data = new MultiResolutionPrefabCreator.Result { SavePath = prefabPath, Prefab = prefabAsset, Bounds = convertedObject.GetComponentInChildren<Renderer>() == null ? default : convertedObject.GetComponentInChildren<Renderer>().bounds, ZoomLevel = 11 }; // ↑で生成済みなのでResultのみ
            var prefab2Data = MultiResolutionPrefabCreator.CreateFromGameObject(convertedObject, assetConfig.AssetPath, 2, 10);
            var prefab4Data = MultiResolutionPrefabCreator.CreateFromGameObject(convertedObject, assetConfig.AssetPath, 4, 9);

            var prefabs = new List<MultiResolutionPrefabCreator.Result> { prefab1Data, prefab2Data, prefab4Data };
            prefabs = prefabs.Where(p => p != null && p.Prefab != null).ToList();

            RegisterAssets(prefabs, groupName, cityObject, metaStore);

            // シーン上のオブジェクトを削除
            GameObject.DestroyImmediate(convertedObject);

            return true;
        }

        /// <summary>
        /// アセット登録
        /// </summary>
        /// <param name="results"></param>
        /// <param name="groupName"></param>
        /// <param name="cityObject"></param>
        /// <param name="metaStore"></param>
        private static void RegisterAssets(
            IList<MultiResolutionPrefabCreator.Result> results,
            string groupName,
            PLATEAUCityObjectGroup cityObject,
            PLATEAUDynamicTileMetaStore metaStore)
        {
            foreach (var res in results)
            {
                if (res == null || res.Prefab == null)
                {
                    Debug.Log($"Skipped null prefab.");
                    continue;
                }

                var prefabAsset = res.Prefab;
                string prefabPath;
                if (string.IsNullOrEmpty(res.SavePath))
                {
                    prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
                }
                else
                {
                    prefabPath = Path.IsPathRooted(res.SavePath)
                    ? AssetPathUtil.GetAssetPath(res.SavePath)
                    : AssetPathUtil.NormalizeAssetPath(res.SavePath);
                }

                var bounds = res.Bounds;
                var zoomLevel = res.ZoomLevel;
                var lod = cityObject.Lod;

                // プレハブをAddressableに登録
                // TODO : タイルごとにAddress名を設定する
                var address = prefabAsset.name;
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefabPath,
                    address,
                    groupName,
                    new List<string> { AddressableLabel });

                Debug.Log($"プレハブをAddressableに登録しました: {address} path : {prefabPath}");

                // メタ情報を登録
                metaStore.AddMetaInfo(address, bounds, lod, zoomLevel);
            }
        }

        #endregion
        
        /// <summary>
        /// <see cref="PLATEAUTileManager"/>が保持するタイル範囲がSceneViewカメラにぴったり収まるようなカメラ位置を計算して返します。
        /// シーンビューのカメラは真下を向きます。
        /// ただし、離れすぎて見えない場合は見える程度の距離にします。
        /// </summary>
        public static void FocusSceneViewCameraToTiles(PLATEAUTileManager manager) 
        {
            if (manager == null)
            {
                Debug.LogWarning("PLATEAUTileManagerがnullです。");
                return;
            }

            var bounds = manager.GetTileBounds();
            if (bounds.size == Vector3.zero)
            {
                Debug.LogWarning("有効なタイルBoundsが存在しません。");
                return;
            }

            // SceneViewカメラ情報を取得
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                Debug.LogWarning("シーンビューまたはシーンビューカメラが見つからないため、タイルへのカメラのフォーカスを中止します。");
                return;
            }

            Camera cam = sceneView.camera;
            float verticalFov = cam.fieldOfView;
            float aspect = cam.aspect;

            // XZ平面上での半サイズ
            float halfWidth = bounds.size.x * 0.5f;
            float halfDepth = bounds.size.z * 0.5f;

            // バウンディング球半径 (XZ 上で計算)
            float radius = Mathf.Sqrt(halfWidth * halfWidth + halfDepth * halfDepth);

            // 縦 FOV から必要距離を計算
            float distanceVertical = radius / Mathf.Tan(Mathf.Deg2Rad * verticalFov * 0.5f);

            // 横 FOV も考慮 (aspect から計算)
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * verticalFov * 0.5f) * aspect);
            float distanceHorizontal = radius / Mathf.Tan(horizontalFov * 0.5f);

            // 離れすぎてタイルが隠れないように
            float maxDistance = manager.loadDistances.Select(ld => ld.Value.Item2).Max() * 0.4f; // 0.4の根拠は勘

            // 横か縦か大きい方を採用
            float distance = Mathf.Min(distanceVertical, distanceHorizontal);
            distance = Mathf.Min(distance, maxDistance);
            
            // シーンビューの視点をタイルにフォーカス
            var nextPivot = bounds.center;

            // カメラ移動を反映させます。これがないと、手動でシーンを動かすまでタイルが出てきません。
            EditorApplication.delayCall += () =>
            {
                // 1フレーム目
                var sv = SceneView.lastActiveSceneView;
                if (sv != null && sv.camera != null && manager != null)
                {
                    sv.pivot = nextPivot;
                    sv.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                    sv.size = distance;
                    
                    var camPos = sv.camera.transform.position;
                    manager.UpdateCameraPosition(camPos);
                    manager.UpdateAssetsByCameraPosition(camPos).ContinueWithErrorCatch();
                    sv.Repaint();

					// 2フレーム目
					void NudgeOnce()
					{
						var sv2 = SceneView.lastActiveSceneView;
						if (sv2 == null || sv2.camera == null || manager == null)
						{
							EditorApplication.update -= NudgeOnce;
							return;
						}
						var delta = 0.6f;
						sv2.pivot = nextPivot + new Vector3(delta, 0f, 0f);
						sv2.Repaint();
						EditorApplication.QueuePlayerLoopUpdate();

						var camPos2 = sv2.camera.transform.position;
						manager.UpdateCameraPosition(camPos2);
						manager.UpdateAssetsByCameraPosition(camPos2).ContinueWithErrorCatch();

						EditorApplication.update -= NudgeOnce;
					}
					EditorApplication.update += NudgeOnce;
                }
            };
        }
        
        /// <summary>
        /// 保存先にすでにメタデータがあるなら、そこに新規のタイル情報を追加します。
        /// これにより上書きの代わりに新規追加になるようにします。
        /// 加えて、前の処理のタイルをAddressable Groupに追加することで新規追加後に新旧を両方読めるようにします。
        /// </summary>
        private void AddTilesToOldMetaIfOldMetaExist()
        {
            string shorterGroupName = Context.AddressableGroupName.Replace(DynamicTileProcessingContext.AddressableGroupBaseName + "_", "");
            string addressName = $"{AddressableAddressBase}_{shorterGroupName}";
            string normalizedAssetPath = AssetPathUtil.NormalizeAssetPath(Context.AssetConfig.AssetPath);
            string dataPath = Path.Combine(normalizedAssetPath, addressName + ".asset").Replace('\\', '/');

            var existingMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
            if (existingMeta != null && Context.MetaStore != null && Context.MetaStore.TileMetaInfos != null)
            {
                // Assets内のケースで、既存のメタに新規分を追加します。
                foreach (var info in Context.MetaStore.TileMetaInfos)
                {
                    if (info == null) continue;
                    existingMeta.AddMetaInfo(info.AddressName, info.Extent, info.LOD, info.ZoomLevel);
                }
                
                EditorUtility.SetDirty(existingMeta);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(dataPath, ImportAssetOptions.ForceUpdate);
                
                Context.MetaStore = existingMeta;

                // 追加前のアセットバンドルをAddressable Groupに登録
                try
                {
                    var addresses = GetDistinctAddressesFromMeta(existingMeta);
                    EnsureAddressesInGroup(addresses, Context.AddressableGroupName, null);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"既存メタのアドレス取り込みに失敗しました: {ex.Message}");
                }
            }
            else
            {
                // Assets外のケースで、既存のメタに新規分を追加します。
                try
                {
                    if (!string.IsNullOrEmpty(Context.BuildFolderPath) && Directory.Exists(Context.BuildFolderPath))
                    {
                        var catalogFiles = Directory.GetFiles(Context.BuildFolderPath, "catalog_*.json", SearchOption.AllDirectories)
                            .OrderByDescending(File.GetLastWriteTimeUtc)
                            .ToArray();
                        if (catalogFiles.Length > 0)
                        {
                            var latestCatalog = catalogFiles[0];
                            var loader = new AddressableLoader();
                            var oldMeta = loader.InitializeAsync(latestCatalog).GetAwaiter().GetResult();
                            if (oldMeta != null && oldMeta.TileMetaInfos != null && Context.MetaStore != null)
                            {
                                foreach (var info in oldMeta.TileMetaInfos)
                                {
                                    if (info == null) continue;
                                    Context.MetaStore.AddMetaInfo(info.AddressName, info.Extent, info.LOD, info.ZoomLevel);
                                }

                                // 追加前のアセットバンドルをAddressable Groupに登録
                                var addresses = GetDistinctAddressesFromMeta(oldMeta);
                                EnsureAddressesInGroup(addresses, Context.AddressableGroupName, latestCatalog);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"旧メタデータの読み込みに失敗しました: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 同じディレクトリに複数回タイル出力したとき、上書きではなく追加するために旧アセットバンドルを認識させる目的で、
        /// 指定アドレスを現在のグループに含めます。
        /// 1) Assets 内に同名Prefabがあれば再登録
        /// 2) 無ければ、catalogPath が指定されている場合に限り Addressables 経由でロード→一時Prefab化→登録
        /// </summary>
        private static void EnsureAddressesInGroup(IEnumerable<string> addresses, string groupName, string catalogPath)
        {
            if (addresses == null) return;

            AsyncOperationHandle<IResourceLocator>? catalogHandle = null;
            bool catalogLoaded = false;

            var tempRoot = Path.Combine("Assets", AddressableLoader.AddressableLocalBuildFolderName, groupName).Replace('\\','/');
            if (!Directory.Exists(tempRoot)) Directory.CreateDirectory(tempRoot);

            foreach (var address in addresses)
            {
                if (string.IsNullOrEmpty(address)) continue;
                try
                {
                    // 1) Assets 内にあるケース
                    var guids = AssetDatabase.FindAssets($"t:Prefab {address}");
                    bool registered = false;
                    if (guids != null && guids.Length > 0)
                    {
                        foreach (var guid in guids)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            if (string.IsNullOrEmpty(path)) continue;
                            var name = Path.GetFileNameWithoutExtension(path);
                            if (!string.Equals(name, address, StringComparison.Ordinal)) continue;
                            AddressablesUtility.RegisterAssetAsAddressable(path, address, groupName, new List<string> { AddressableLabel });
                            registered = true;
                            break;
                        }
                    }
                    if (registered) continue;

                    // 2) Assets外のケースでは、Addressablleに認識させるために一時的にインポートしてプレハブにします。
                    if (!string.IsNullOrEmpty(catalogPath))
                    {
                        if (!catalogLoaded)
                        {
                            var ch = Addressables.LoadContentCatalogAsync(catalogPath);
                            ch.WaitForCompletion();
                            catalogHandle = ch;
                            catalogLoaded = true;
                        }
                        var handle = Addressables.LoadAssetAsync<GameObject>(address);
                        handle.WaitForCompletion();
                        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                        {
                            var go = handle.Result;
                            var savePath = Path.Combine(tempRoot, address + ".prefab").Replace('\\','/');
                            var saved = PrefabUtility.SaveAsPrefabAsset(go, savePath);
                            if (saved != null)
                            {
                                AddressablesUtility.RegisterAssetAsAddressable(savePath, address, groupName, new List<string> { AddressableLabel });
                            }
                        }
                        Addressables.Release(handle);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"アドレス取り込みに失敗しました: {address} - {ex.Message}");
                }
            }

            if (catalogLoaded && catalogHandle.HasValue)
            {
                Addressables.Release(catalogHandle.Value);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// メタストアから重複のないアドレス一覧を抽出します。
        /// </summary>
        private static List<string> GetDistinctAddressesFromMeta(PLATEAUDynamicTileMetaStore meta)
        {
            if (meta == null || meta.TileMetaInfos == null) return new List<string>();
            return meta.TileMetaInfos
                .Where(i => i != null && !string.IsNullOrEmpty(i.AddressName))
                .Select(i => i.AddressName)
                .Distinct()
                .ToList();
        }
        
        /// <summary>
        /// 保存先にすでにメタデータがあるのなら、ReferencePointを既存のメタデータに合わせます。
        /// なければ範囲選択後のデフォルト値が使われます。
        /// これにより、同じフォルダに複数回タイルを生成した場合でも位置が合うようになります。
        /// </summary>
        private void SetReferencePointSameAsOldMetaIfExist(CityImportConfig cityConfig)
        {
            Vector3 rp = cityConfig.ReferencePoint.ToUnityVector(); // デフォルト値
            try
            {

                string shorterGroupName =
                    Context.AddressableGroupName.Replace(DynamicTileProcessingContext.AddressableGroupBaseName + "_",
                        "");
                string addressName = $"{AddressableAddressBase}_{shorterGroupName}";

                // 1) Assets 内にメタがある場合
                string normalizedAssetPath = AssetPathUtil.NormalizeAssetPath(Context.AssetConfig.AssetPath);
                string dataPath = Path.Combine(normalizedAssetPath, addressName + ".asset").Replace('\\', '/');
                var existingMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
                if (existingMeta != null)
                {
                    rp = existingMeta.ReferencePoint;
                }
                else
                {
                    // 2) 外部出力先のカタログからAddressables経由で読み込み
                    if (!string.IsNullOrEmpty(Context.BuildFolderPath) && Directory.Exists(Context.BuildFolderPath))
                    {
                        var catalogFiles = Directory.GetFiles(Context.BuildFolderPath, "catalog_*.json",
                                SearchOption.AllDirectories)
                            .OrderByDescending(File.GetLastWriteTimeUtc)
                            .ToArray();
                        if (catalogFiles.Length > 0)
                        {
                            var latestCatalog = catalogFiles[0];
                            var loader = new AddressableLoader();
                            var oldMeta = loader.InitializeAsync(latestCatalog).GetAwaiter().GetResult();
                            if (oldMeta != null)
                            {
                                rp = oldMeta.ReferencePoint;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"既存メタからReferencePointの取得に失敗しました: {ex.Message}");
            }
            finally
            {
                Context.MetaStore.ReferencePoint = rp;
                cityConfig.ReferencePoint = rp.ToPlateauVector();
            }
        }

    }
    
    
}