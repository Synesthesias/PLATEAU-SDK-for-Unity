using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Import;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    internal class GenerateOneTile : IOnOneTileImported
    {
        private readonly DynamicTileProcessingContext context;
        private readonly IProgressDisplay progressDisplay;

        public GenerateOneTile(DynamicTileProcessingContext context, IProgressDisplay progressDisplay)
        {
            this.context = context;
            this.progressDisplay = progressDisplay;
        }
        
        public bool OnOneTileImported(TileImportResult importResult)
        {
            if (context == null || !context.IsValid())
            {
                Debug.LogError("DynamicTileProcessingContextが無効です。SetupPreProcessingが呼ばれているか確認してください。");
                return false;
            }

            var placedObject = importResult.RootObject;
            var zoomLevel = importResult.ZoomLevel;
            int totalGmlCount = importResult.TotalGmlCount;
            if (placedObject == null) return false;
            context.GmlCount = totalGmlCount;

            if (zoomLevel == 11)
            {
                var childObjects = placedObject.transform.Cast<Transform>()
                    .Select(t => t.gameObject)
                    .ToArray();
                // 複数GameObject処理
                ProcessGameObjects(
                    childObjects,
                    zoomLevel,
                    context,
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
                    context,
                    errorMessage => Debug.LogError($"DynamicTileExporter ProcessGameObject error: {errorMessage}"));
            }

            return true;
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
                new List<string> { DynamicTileExporter.AddressableLabel });

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
    }
}