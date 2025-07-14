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

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 都市モデルをDynamicTile用にプレハブ化し、一括でアセットバンドルとして出力する。
    /// </summary>
    public class DynamicTileExporter : IPostGmlImportProcessor
    {
        private const string AddressableGroupName = "PLATEAUCityObjectGroup";
        private const string AddressableLabel = "DynamicTile";
        
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
        public bool SetupPreProcessing(DynamicTileImportConfig config)
        {
            if (config == null)
            {
                Debug.LogError("DynamicTileImportConfigがnullです。");
                return false;
            }
            
            PLATEAUEditorEventListener.disableProjectChangeEvent = true; // タイル生成中フラグを設定
            
            // DynamicTile管理用Managerを破棄
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager != null)
            {
                GameObject.DestroyImmediate(manager.gameObject);
            }
            
            Context = new DynamicTileProcessingContext(config);
    
            // グループを削除
            AddressablesUtility.RemoveNonDefaultGroups(AddressableLabel, Context.IsExcludeAssetFolder);

            // グループを設定
            if (Context.IsExcludeAssetFolder)
            {
                // Remote用のプロファイルを作成
                var profileID = AddressablesUtility.SetOrCreateProfile(Context.AddressableGroupName);
                if (!string.IsNullOrEmpty(profileID))
                {
                    AddressablesUtility.SetRemoteProfileSettings(Context.BuildFolderPath, Context.AddressableGroupName);
                    AddressablesUtility.SetGroupLoadAndBuildPath(Context.AddressableGroupName);
                }
            }
            else
            {
                // プロファイルをデフォルトに設定
                AddressablesUtility.SetDefaultProfileSettings(Context.AddressableGroupName);
            }

            if (!Context.IsValid())
            {
                Debug.LogError("context is invalid.");
                return false;
            }
            return true;
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

            var baseFolderPath = Path.Combine(assetConfig.AssetPath, $"{cityObject.gameObject.name}_11"); // 解像度オリジナルなので_11を付ける
            var saveFolderPath = baseFolderPath;
            int count = 1;
            // 同名のディレクトリが存在する場合は、_1, _2, ... のように連番を付けて保存
            while (Directory.Exists(saveFolderPath))
            {
                saveFolderPath = $"{baseFolderPath}_{count}";
                count++;
            }

            var convertedObject = PrepareAndConvert(assetConfig, saveFolderPath, onError);
            if (convertedObject == null)
            {
                Debug.LogWarning($"{cityObject.gameObject.name} の変換に失敗しました。");
                return false;
            }

            string prefabPath = saveFolderPath + ".prefab";
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

            RegisterAssets(new List<MultiResolutionPrefabCreator.Result> { prefab1Data, prefab2Data, prefab4Data }, groupName, cityObject, metaStore);

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
            foreach(var res in results)
            {
                if (res == null || res.Prefab == null)
                {
                    Debug.LogWarning("変換結果がnullまたはプレハブがnullです。");
                    continue;
                }

                var prefabAsset = res.Prefab;
                var prefabPath = AssetPathUtil.GetAssetPath(res.SavePath);
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
                
                // AssetDatabase用のパスに変換
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
        private static void SaveAndRegisterMetaData(PLATEAUDynamicTileMetaStore metaStore, string assetPath, string groupName)
        {
            if (metaStore == null)
            {
                Debug.LogWarning("メタデータがnullです。");
                return;
            }

            // メタデータをアセットとして保存
            string addressName = nameof(PLATEAUDynamicTileMetaStore);

            // metaStoreの名前をグループ名に基づいて変更
            if (groupName.IndexOf('_') >= 0)
            {
                var groupNameSplit = groupName.Split('_');
                addressName += "_" + groupNameSplit[1];
            }

            // assetPathが既に相対パスであることを確認し、必要に応じて変換
            string normalizedAssetPath = assetPath;
            if (Path.IsPathRooted(assetPath))
            {
                // 絶対パスの場合は相対パスに変換
                normalizedAssetPath = AssetPathUtil.GetAssetPath(assetPath);
            }
            else if (!assetPath.StartsWith("Assets"))
            {
                // Assetsで始まらない相対パスの場合は、Assetsからの相対パスに変換
                normalizedAssetPath = AssetPathUtil.GetAssetPathFromRelativePath(assetPath);
            }

            string dataPath = Path.Combine(normalizedAssetPath, addressName + ".asset");
            // Path.Combineは環境によってバックスラッシュを使うため、フォワードスラッシュに統一
            dataPath = dataPath.Replace('\\', '/');
            
            AssetDatabase.CreateAsset(metaStore, dataPath);
            AssetDatabase.SaveAssets();

            // メタデータをAddressableに登録
            AddressablesUtility.RegisterAssetAsAddressable(
                dataPath,
                addressName,
                groupName,
                new List<string> { AddressableLabel });
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

            // ディレクトリの存在確認
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // 都市オブジェクトを個別に処理
            for (int i = 0; i < cityObjectGroups.Count; i++)
            {
                var cityObject = cityObjectGroups[i];
                if (cityObject == null) continue;

                // オブジェクト名にメッシュコードを追加
                cityObject.name = meshCode + "_" + cityObject.name;

                // 進捗更新を通知
                // 進捗計算: 10%から始まり、GML処理完了ごとに進む（最大80%）
                int loadedGmlCount = Context.IncrementAndGetLoadedGmlCount();
                float progress = 10f + ((float)loadedGmlCount / Context.GmlCount) * 70f;
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
                // メタデータを保存
                SaveAndRegisterMetaData(Context.MetaStore, Context.AssetConfig.AssetPath, Context.AddressableGroupName);

                // Addressablesのビルドを実行
                AddressablesUtility.BuildAddressables(true);

                // managerを生成
                var managerObj = new GameObject("DynamicTileManager");
                var manager = managerObj.AddComponent<PLATEAUTileManager>();

                if (Context.IsExcludeAssetFolder)
                {
                    // カタログファイルのパスを取得
                    var catalogFiles = Directory.GetFiles(Context.BuildFolderPath, "catalog_*.json");
                    if (catalogFiles.Length == 0)
                    {
                        Debug.LogError("カタログファイルが見つかりません");
                        return false;
                    }
                    var catalogPath = catalogFiles[0]; // 最新のカタログファイルを使用
                    manager.SaveCatalogPath(catalogPath);
                    
                    // 一時フォルダーを削除
                    CleanupTempFolder();
                }

                Dialogue.Display("動的タイルの保存が完了しました！", "OK");
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
                    manager.InitializeTiles().ContinueWithErrorCatch(); // タイルの初期化
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
            var assetPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                AssetDatabase.Refresh();
                Debug.Log($"一時フォルダーを削除しました: {assetPath}");
            }
            else
            {
                Debug.LogWarning($"一時フォルダーの削除に失敗しました: {assetPath}");
            }
        }

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
        /// 進行中のAddressable化をキャンセルします。
        /// </summary>
        public void Cancel()
        {

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
    }
}