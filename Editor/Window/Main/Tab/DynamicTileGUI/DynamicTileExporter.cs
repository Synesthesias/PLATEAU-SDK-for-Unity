using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityInfo;
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
                assetConfig.SrcGameObj = cityObject.gameObject;

                // 変換実行
                var convertedObject = PrepareAndConvert(assetConfig, onError);
                if (convertedObject == null)
                {
                    onError?.Invoke("変換に失敗しました。");
                    return;
                }
                
                string prefabPath = $"{assetConfig.AssetPath}/{convertedObject.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);

                // プレハブをAddressableに登録
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefabPath,
                    prefabPath,
                    groupName,
                    new List<string> { AddressableLabel });
            }
            
            if (!string.IsNullOrEmpty(buildFolderPath))
            {
                // ビルドパスを指定
                AddressablesUtility.SetGroupLoadAndBuildPath(groupName, buildFolderPath);

                // ビルド対象外にする
                AddressablesUtility.SetGroupIncludeInBuild(groupName, false);
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
    }
} 