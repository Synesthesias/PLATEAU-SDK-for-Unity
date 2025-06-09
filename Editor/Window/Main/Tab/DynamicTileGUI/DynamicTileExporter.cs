using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var cityObjectGroups = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>();
            if (cityObjectGroups == null || cityObjectGroups.Length == 0)
            {
                onError?.Invoke("CityObjectGroupが見つかりません。都市モデルにCityObjectGroupが含まれていることを確認してください。");
                return;
            }

            using var progressBar = new ProgressBar();

            var groupName = AddressableGroupName;
            var isExcludeAssetFolder = !string.IsNullOrEmpty(buildFolderPath);
            if (isExcludeAssetFolder)
            {
                // ビルドフォルダパスを指定する場合はグループを分ける
                var directoryName = Path.GetFileName(
                    buildFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                groupName += "_" + directoryName;
            }

            // グループを削除
            AddressablesUtility.RemoveNonDefaultGroups(AddressableLabel);
            
            // マネージャーを取得または生成
            var tileManager = GetOrCreateTileManager();
            tileManager.ClearTiles();

            for (var i = 0; i < cityObjectGroups.Length; i++)
            {
                var cityObject = cityObjectGroups[i];
                if (cityObject == null || cityObject.gameObject == null)
                {
                    Debug.LogWarning($"GameObjectがnullです。");
                    continue;
                }
                
                float progress = (float)(i+1) / cityObjectGroups.Length;
                progressBar.Display("動的タイルを生成中..", progress);

                var parentTransform = cityObject.transform.parent;
                assetConfig.SrcGameObj = cityObject.gameObject;
                
                // アドレス名を取得
                var addressName = GetAddress(cityObject.gameObject);
                if (string.IsNullOrEmpty(addressName))
                {
                    Debug.LogWarning($"{cityObject.gameObject.name} のアドレス名の取得に失敗しました。");
                    continue;
                }
            
                // アドレス名が重複している場合は、_1, _2, ... のように連番を付ける
                int count = 1;
                var baseAddressName = addressName;
                while (tileManager.DynamicTiles.Any(t => t.Address == addressName))
                {
                    // 最後の部分（baseName）に連番を付ける
                    addressName = $"{baseAddressName}_{count}";
                    count++;
                }
    
                var saveFolderPath = Path.Combine(assetConfig.AssetPath, addressName);
                var convertedObject = PrepareAndConvert(assetConfig, saveFolderPath, onError);
                if (convertedObject == null)
                {
                    Debug.LogWarning($"{cityObject.gameObject.name} : {addressName} : {saveFolderPath} の変換に失敗しました。");
                    continue;
                }
                
                string prefabPath = saveFolderPath + ".prefab";
                var prefabAsset = PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);
                if (prefabAsset == null)
                {
                    Debug.LogWarning($"{convertedObject.name} プレハブの保存に失敗しました。");
                    continue;
                }

                progressBar.Display("動的タイルをAddressableに登録中..", progress);
    
                // プレハブをAddressableに登録
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefabPath,
                    addressName,
                    groupName,
                    new List<string> { AddressableLabel });
                
                // DynamicTileManagerにアドレスを追加
                tileManager.AddTile(
                    new PLATEAUDynamicTile(addressName, parentTransform));
    
                // シーン上のオブジェクトを削除
                GameObject.DestroyImmediate(convertedObject);
            }

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
            
            // Addressableのビルド後、シーンを読み直しているので、再度Manager取得
            tileManager = GetOrCreateTileManager();
            tileManager.SaveCatalogPath("");
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
                tileManager.SaveCatalogPath(catalogPath);
            }

            progressBar.Display("Addressableのビルドを実行中...", 0.99f);
            Dialogue.Display("動的タイルの保存が完了しました！", "OK");
        }

        /// <summary>
        /// PLATEAUTileManagerを取得または生成します。
        /// </summary>
        /// <returns>PLATEAUTileManagerのインスタンス。生成に失敗した場合はnull。</returns>
        private static PLATEAUTileManager GetOrCreateTileManager()
        {
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DynamicTileManager");
                manager = managerObj.AddComponent<PLATEAUTileManager>();
            }
            return manager;
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
        /// 指定したGameObjectからAddressables用のアドレス名を取得します。
        /// </summary>
        /// <param name="obj">アドレスを取得したいGameObject</param>
        /// <returns>アドレス名。取得できない場合は空文字列。</returns>
        private static string GetAddress(GameObject obj)
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
                // TODO：Zoom Levelを取得する
                return $"tile_zoom_0_grid_{meshCode}_{baseName}";
            }
            return baseName;
        }
    }
} 