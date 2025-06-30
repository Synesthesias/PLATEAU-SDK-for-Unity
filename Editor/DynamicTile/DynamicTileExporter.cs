using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using PLATEAU.Util.Async;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 都市モデルをDynamicTile用にプレハブ化し、一括エクスポート処理を提供する。
    /// </summary>
    public static class DynamicTileExporter
    {
        private const string AddressableGroupName = "PLATEAUCityObjectGroup";
        private const string AddressableLabel = "DynamicTile";

        /// <summary>
        /// Addressableの事前処理を行います。
        /// </summary>
        public static DynamicTileProcessingContext SetupAddressablePreProcessing(DynamicTileImportConfig config)
        {
            try
            {
                var context = new DynamicTileProcessingContext(config);

                // グループを削除
                AddressablesUtility.RemoveNonDefaultGroups(AddressableLabel);
                
                PLATEAUEditorEventListener.IsTileCreationInProgress = true; // タイル生成中フラグを設定
                
                // DynamicTile管理用Managerを破棄
                var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
                if (manager != null)
                {
                    GameObject.DestroyImmediate(manager.gameObject);
                }
                
                return context;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Addressable事前処理でエラーが発生しました: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 都市オブジェクト配列を処理し、プレハブ化・Addressable登録を行う
        /// </summary>
        /// <param name="cityObjects">処理対象の都市オブジェクト配列</param>
        /// <param name="context">DynamicTile処理コンテキスト</param>
        /// <param name="onError">エラー時のコールバック</param>
        public static void ProcessCityObjects(
            List<PLATEAUCityObjectGroup> cityObjects,
            DynamicTileProcessingContext context,
            Action<string> onError = null)
        {
            if (context == null || !context.IsValid())
            {
                Debug.LogError("DynamicTileProcessingContextが無効です。");
                return;
            }

            for (var i = 0; i < cityObjects.Count; i++)
            {
                float progress = (float)(i + 1) / cityObjects.Count;
                context.ProgressBar?.Display("動的タイルを生成中..", progress);

                ProcessCityObject(cityObjects[i], context.AssetConfig, context.AddressableGroupName, context.MetaStore, onError);

                context.ProgressBar?.Display("動的タイルをAddressableに登録中..", progress);
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
                var address = GetAddress(prefabAsset, zoomLevel);
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

        /// <summary>
        /// 指定したGameObjectからAddressables用のアドレス名を取得します。
        /// </summary>
        /// <param name="obj">アドレスを取得したいGameObject</param>
        /// <param name="zoomLevel"></param>
        /// <returns>アドレス名。取得できない場合は空文字列。</returns>
        private static string GetAddress(GameObject obj, int zoomLevel)
        {
            if (obj == null) return string.Empty;

            string baseName = obj.name;
            string meshCode = string.Empty;

            // 親のGMLオブジェクトを探す
            var gmlObjects = obj.GetComponentsInParent<Transform>()
                .Where(t => t.name.EndsWith(".gml"))
                .ToList();

            if (gmlObjects.Count > 0)
            {
                var gmlName = gmlObjects[0].name;

                // パスが含まれているかチェック
                // 13_1/533936_htd_6697_op.gml
                // pref/sumidagaw-shingashigawa-ryuiki/53393680_fld_6697_l2_op.gml
                if (gmlName.Contains("/"))
                {
                    // パスが含まれている場合は最後の部分を取得
                    var parts = gmlName.Split('/');
                    var lastPart = parts[parts.Length - 1];
                    // 最後の部分から最初の_までの部分を取得
                    meshCode = lastPart.Split('_')[0];
                }
                else
                {
                    // パスが含まれていない場合は最初の_までの部分を取得
                    meshCode = gmlName.Split('_')[0];
                }
            }

            var gridCode = GridCode.Create(meshCode);
            if (!gridCode.IsValid)
            {
                Debug.LogError($"不正なメッシュコード形式です: {meshCode}");
                return string.Empty;
            }

            // 2次メッシュ（6桁）
            if (meshCode.Length == 6)
            {
                return $"tile_{meshCode}_{baseName}";
            }
            // 3次メッシュ（8桁）
            else if (meshCode.Length == 8)
            {
                return $"tile_zoom_{zoomLevel}_grid_{meshCode}_{baseName}";
            }
            return baseName;
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
                onError?.Invoke($"変換処理中にエラーが発生しました: {ex.Message}");
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
            string dataPath = Path.Combine(assetPath, addressName + ".asset");
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
        /// DynamicTileの完了処理を行います（メタストア保存、Addressable処理、マネージャー設定）
        /// </summary>
        /// <param name="context">DynamicTile処理コンテキスト</param>
        /// <param name="onError">エラー時のコールバック（省略可）</param>
        public static void CompleteDynamicTileProcessing(
            DynamicTileProcessingContext context = null,
            System.Action<string> onError = null)
        {
            if (context == null || !context.IsValid())
            {
                Debug.LogError("DynamicTileProcessingContextが無効です。");
                return;
            }

            try
            {
                // メタデータを保存
                SaveAndRegisterMetaData(context.MetaStore, context.AssetConfig.AssetPath, context.AddressableGroupName);

                context.ProgressBar?.Display("Addressableのビルドを実行中...", 0.1f);

                if (context.IsExcludeAssetFolder)
                {
                    // Remote用のプロファイルを作成
                    var profileID = AddressablesUtility.SetOrCreateProfile(context.AddressableGroupName);
                    if (!string.IsNullOrEmpty(profileID))
                    {
                        AddressablesUtility.SetRemoteProfileSettings(context.BuildFolderPath, context.AddressableGroupName);
                        AddressablesUtility.SetGroupLoadAndBuildPath(context.AddressableGroupName);
                    }
                }
                else
                {
                    // プロファイルをデフォルトに設定
                    AddressablesUtility.SetDefaultProfileSettings(context.AddressableGroupName);
                }

                // Addressablesのビルドを実行
                AddressablesUtility.BuildAddressables(true);

                // managerを生成
                var managerObj = new GameObject("DynamicTileManager");
                var manager = managerObj.AddComponent<PLATEAUTileManager>();

                if (context.IsExcludeAssetFolder)
                {
                    // カタログファイルのパスを取得
                    var catalogFiles = Directory.GetFiles(context.BuildFolderPath, "catalog_*.json");
                    if (catalogFiles.Length == 0)
                    {
                        Debug.LogError("カタログファイルが見つかりません");
                        return;
                    }
                    var catalogPath = catalogFiles[0]; // 最新のカタログファイルを使用
                    manager.SaveCatalogPath(catalogPath);
                }

                context.ProgressBar?.Display("Addressableのビルドを実行中...", 0.99f);
                Dialogue.Display("動的タイルの保存が完了しました！", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"動的タイルのエクスポート中にエラーが発生しました: {ex.Message}");
                onError?.Invoke($"動的タイルのエクスポート中にエラーが発生しました: {ex.Message}");
            }
            finally
            {
                PLATEAUEditorEventListener.IsTileCreationInProgress = false; // タイル生成中フラグを設定     
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
                
                // リソース解放
                context?.Dispose();
            }
        }
    }
}