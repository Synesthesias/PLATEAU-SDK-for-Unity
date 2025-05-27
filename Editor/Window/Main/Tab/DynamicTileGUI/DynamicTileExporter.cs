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

            var groupName = AddressableGroupName;
            if (isExcludeAssetFolder)
            {
                // ビルドフォルダパスを指定する場合はグループを分ける
                var directoryName = Path.GetFileName(
                    buildFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                groupName += "_" + directoryName;
            }

            var addresses = new List<string>();
            foreach (var cityObject in cityObjects)
            {
                if (cityObject == null || cityObject.gameObject == null)
                {
                    Debug.LogWarning($"GameObjectがnullです。");
                    continue;
                }
            
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
            
                // プレハブをAddressableに登録
                // TODO : タイルごとにAddress名を設定する
                var address = prefabAsset.name;
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefabPath,
                    address,
                    groupName,
                    new List<string> { AddressableLabel });
                Debug.Log($"プレハブをAddressableに登録: {address}");
                
                addresses.Add(address);
            
                // シーン上のオブジェクトを削除
                GameObject.DestroyImmediate(convertedObject);
            }

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
                AddressablesUtility.SetOrCreateProfile("Default");
            }

            // Addressablesのビルドを実行
            AddressablesUtility.BuildAddressables();
            
            // DynamicTile管理用GameObjectを生成
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DynamicTileManager");
                manager = managerObj.AddComponent<PLATEAUTileManager>();
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
            
            foreach (var address in addresses)
            {
                var tile = new PLATEAUDynamicTile(address);
                manager.AddTile(tile);
            }

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
                convertObjects = new ConvertToAsset().ConvertCore(config);
                
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
    }
} 