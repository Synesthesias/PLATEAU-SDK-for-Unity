using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace PLATEAU.Editor.Window.Main.Tab.DynamicTileGUI
{
    /// <summary>
    /// 都市モデルをDynamicTile用にプレハブ化し、一括エクスポート処理を提供する。
    /// </summary>
    public static class DynamicTileExporter
    {
        private const string AddressableGroupName = "PLATEAUCityObjectGroup";
        private const string AddressableLabel = "DynamicTile";
        private const string MetaDataAddressName = "PLATEAUDynamicTileCollection";

        /// <summary>
        /// 都市モデルをDynamicTile用にプレハブ化し、一括エクスポートする。
        /// </summary>
        /// <param name="assetConfig">変換設定</param>
        /// <param name="buildFolderPath"></param>
        /// <param name="onError">エラー時のコールバック</param>
        public static void Export(
            ConvertToAssetConfig assetConfig,
            string buildFolderPath,
            Action<string> onError = null)
        {
            var isExcludeAssetFolder = !string.IsNullOrEmpty(buildFolderPath);
            var cityObjects = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>();
            if (cityObjects == null || cityObjects.Length == 0)
            {
                onError?.Invoke("都市モデルが見つかりません。都市モデルをインポートしてください。");
                return;
            }

            using var progressBar = new ProgressBar();

            var groupName = AddressableGroupName;
            if (isExcludeAssetFolder)
            {
                // ビルドフォルダパスを指定する場合はグループを分ける
                var directoryName = Path.GetFileName(
                    buildFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                groupName += "_" + directoryName;
            }

            // グループを削除
            AddressablesUtility.RemoveNonDefaultGroups(AddressableLabel);

            // DynamicTile管理用GameObjectを生成
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DynamicTileManager");
                manager = managerObj.AddComponent<PLATEAUTileManager>();
            }
            manager.ClearTiles();
            
            // メタデータ生成
            var tileCollection = ScriptableObject.CreateInstance<PLATEAUDynamicTileCollection>();

            for (var i = 0; i < cityObjects.Length; i++)
            {
                var cityObject = cityObjects[i];
                if (cityObject == null || cityObject.gameObject == null)
                {
                    Debug.LogWarning($"GameObjectがnullです。");
                    continue;
                }

                float progress = (float)(i+1) / cityObjects.Length;
                progressBar.Display("動的タイルを生成中..", progress);

                assetConfig.SrcGameObj = cityObject.gameObject;
            
                var baseFolderPath = Path.Combine(assetConfig.AssetPath, cityObject.gameObject.name);
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
                    continue;
                }
                
                // TODO: タイルごとにプレハブを保存する
                string prefabPath = saveFolderPath + ".prefab";
                var prefabAsset = PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);
                if (prefabAsset == null)
                {
                    Debug.LogWarning($"{convertedObject.name} プレハブの保存に失敗しました。");
                    continue;
                }

                progressBar.Display("動的タイルをAddressableに登録中..", progress);
    
                // プレハブをAddressableに登録
                // TODO : タイルごとにAddress名を設定する
                var address = prefabAsset.name;
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefabPath,
                    address,
                    groupName,
                    new List<string> { AddressableLabel });

                var tile = new PLATEAUDynamicTile(address, convertedObject.transform.parent, convertedObject);
                manager.AddTile(tile);
                
                // タイル情報を登録
                tileCollection.AddTile(tile);

                // シーン上のオブジェクトを削除
                GameObject.DestroyImmediate(convertedObject);
            }
            
            // メタデータを保存
            SaveAndRegisterMetaData(tileCollection, assetConfig.AssetPath, groupName);

            progressBar.Display("Addressableのビルドを実行中...", 0.1f);

            if (isExcludeAssetFolder)
            {
                // Remote用のプロファイルを作成
                var profileID = AddressablesUtility.SetOrCreateProfile(groupName);
                if (!string.IsNullOrEmpty(profileID))
                {
                    AddressablesUtility.SetRemoteProfileSettings(buildFolderPath, groupName);
                    AddressablesUtility.SetGroupLoadAndBuildPath(groupName);
                }
            }
            else
            {
                // プロファイルをデフォルトに設定
                AddressablesUtility.SetDefaultProfileSettings();
            }

            // Addressablesのビルドを実行
            AddressablesUtility.BuildAddressables(true);

            // シーンが更新されているので、再度取得
            manager = GameObject.FindObjectOfType<PLATEAUTileManager>(); 
            if (manager == null)
            {
                Debug.LogWarning("PLATEAUTileManagerが見つかりません。");
                return;
            }

            manager.SaveCatalogPath("");
            if (isExcludeAssetFolder)
            {
                // カタログファイルのパスを取得
                var catalogFiles = Directory.GetFiles(buildFolderPath, "catalog_*.json");
                if (catalogFiles.Length == 0)
                {
                    Debug.LogError("カタログファイルが見つかりません");
                    return;
                }
                var catalogPath = catalogFiles[0]; // 最新のカタログファイルを使用
                manager.SaveCatalogPath(catalogPath);
            }
            
            SceneViewCameraTracker.Initialize();

            progressBar.Display("Addressableのビルドを実行中...", 0.99f);
            Dialogue.Display("動的タイルの保存が完了しました！", "OK");
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
        private static void SaveAndRegisterMetaData(PLATEAUDynamicTileCollection tileCollection, string assetPath, string groupName)
        {
            if (tileCollection == null)
            {
                Debug.LogWarning("メタデータがnullです。");
                return;
            }

            // メタデータをアセットとして保存
            string dataPath = Path.Combine(assetPath, MetaDataAddressName + ".asset");
            AssetDatabase.CreateAsset(tileCollection, dataPath);
            AssetDatabase.SaveAssets();

            // メタデータをAddressableに登録
            AddressablesUtility.RegisterAssetAsAddressable(
                dataPath,
                MetaDataAddressName,
                groupName,
                new List<string> { AddressableLabel });
        }
    }
} 