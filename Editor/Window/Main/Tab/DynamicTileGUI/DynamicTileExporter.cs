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
        /// <param name="excludeObjects">除外するオブジェクト</param>
        /// <param name="buildFolderPath"></param>
        /// <param name="onError">エラー時のコールバック</param>
        public static void Export(
            ConvertToAssetConfig assetConfig,
            List<PLATEAUCityObjectGroup> excludeObjects,
            string buildFolderPath,
            Action<string> onError = null)
        {
            var cityObjects = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>();
            if (cityObjects == null || cityObjects.Length == 0)
            {
                onError?.Invoke("都市モデルが見つかりません。都市モデルをインポートしてください。");
                return;
            }

            var groupName = AddressableGroupName;
            if (!string.IsNullOrEmpty(buildFolderPath))
            {
                // ビルドフォルダパスを指定する場合はグループを分ける
                var directoryName = Path.GetFileName(
                    buildFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                groupName += "_" + directoryName;
            }

            foreach (var cityObject in cityObjects)
            {
                if (excludeObjects.Contains(cityObject))
                {
                    continue;
                }
                if (cityObject == null || cityObject.gameObject == null)
                {
                    Debug.LogWarning($"GameObjectがnullです。");
                    continue;
                }
    
                assetConfig.SrcGameObj = cityObject.gameObject;

                // 変換実行
                var convertedObject = PrepareAndConvert(assetConfig, onError);
                if (convertedObject == null)
                {
                    Debug.LogWarning($"{cityObject.gameObject.name} の変換に失敗しました。");
                    continue;
                }
                
                string prefabPath = $"{assetConfig.AssetPath}/{convertedObject.name}.prefab";
                var prefabAsset = PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);
                if (prefabAsset == null)
                {
                    Debug.LogWarning($"{convertedObject.name} プレハブの保存に失敗しました。");
                    continue;
                }

                // プレハブをAddressableに登録
                var downSampleLevel = 0; // TODO: ダウンサンプルレベルごとに登録
                var address = convertedObject.name + "_down_" + downSampleLevel; // TODO : タイルごとにAddressを設定する
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefabPath,
                    address,
                    groupName,
                    new List<string> { AddressableLabel });

                ReplaceWithDynamicTile(
                    convertedObject.name,
                    convertedObject.name,
                    downSampleLevel,
                    convertedObject.transform.parent);
            }

            if (!string.IsNullOrEmpty(buildFolderPath))
            {
                if (!Directory.Exists(buildFolderPath))
                {
                    onError?.Invoke("指定されたビルドフォルダが存在しません。");
                    return;
                }

                // ビルドパスを指定
                AddressablesUtility.SetGroupLoadAndBuildPath(groupName, buildFolderPath);
            }

            // DynamicTile管理用GameObjectを生成
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DynamicTileManager");
                manager = managerObj.AddComponent<PLATEAUTileManager>();
            }
            
            Dialogue.Display("動的タイルの保存が完了しました！", "OK");
        }

        private static GameObject PrepareAndConvert(ConvertToAssetConfig config, Action<string> onError)
        {
            var assetPath = config.AssetPath;
            string subFolderName = config.SrcGameObj.name;
            string subFolderFullPath = Path.Combine(assetPath, subFolderName);
            List<GameObject> convertObjects = null;
            
            try
            {
                if (!Directory.Exists(subFolderFullPath))
                {
                    Directory.CreateDirectory(subFolderFullPath);
                }
                
                // AssetDatabase用のパスに変換
                config.SetByFullPath(subFolderFullPath);

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
        
        private static GameObject ReplaceWithDynamicTile(string objectName, string originalAddress, int downSampleLevel, Transform parent)
        {
            GameObject oldObj = GameObject.Find(objectName);
            if (oldObj != null)
            {
                GameObject.DestroyImmediate(oldObj);
            }

            GameObject newObj = new GameObject(objectName);
            
            // PLATEAUDynamicTileコンポーネントを付与し、Addressをセット
            var dynamicTileComp = newObj.AddComponent<PLATEAU.DynamicTile.PLATEAUDynamicTile>();
            dynamicTileComp.OriginalAddress = originalAddress;
            newObj.transform.SetParent(parent, false);
            
            // 画面の表示上、DynamicTileをロードする
            dynamicTileComp.LoadTile();
            
            return newObj;
        }
    }
} 