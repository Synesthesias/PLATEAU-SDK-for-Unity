using PLATEAU.CityImport.Config;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile.TileModule;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.DynamicTile
{
    /// <summary>
    /// タイルを再生成します。
    /// </summary>
    public class TileRebuilder
    {
        private IOnTileGenerateStart[] onTileGenerateStarts;
        private IBeforeTileAssetBuild[] beforeTileAssetBuilds;
        private IAfterTileAssetBuild[] afterTileAssetBuilds;

        [MenuItem("PLATEAU/Debug/Rebuild Tiles")]
        public static void RebuildInScene()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            new TileRebuilder().Rebuild(manager).ContinueWithErrorCatch();
        }
        
        public async Task Rebuild(PLATEAUTileManager manager)
        {
            // TODO この処理は、タイルがすべて表示される前提で作られています。
            //      非表示のタイルも含めて再生成するよう改修が必要です。
            var prefabDirPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            var importConfig = new DynamicTileImportConfig(ImportType.DynamicTile, prefabDirPath, true);
            var context = new DynamicTileProcessingContext(importConfig);
            
            
            // 各処理を生成します。
            var tileToPrefab = new TileToPrefab(manager, prefabDirPath);
            // var tileManagerGenerator = new TileManagerGenerator(context);
            var tileAddressableConfigMaker = new TileAddressableConfigMaker(context);
            var tileEditorProcedure = new TileGenEditorProcedure();
            var setupBuildPath = new SetUpTileBuildPath(context);
            // var addTilesToOldMetaIfExist = new AddTilesToOldMetaIfExist(context);
            // var setReferencePointToSameIfExist = new SetReferencePointSameIfExist(cityConf, context);
            // var generateOneTile = new GenerateOneTile(context, progressDisplay);
            var saveAndRegisterMetaData = new SaveAndRegisterMetaData(context);
            var cleanUpTempFolder = new TileCleanupTempFolder(context);
            var exportUnityPackage = new ExportUnityPackageOfPrefabs(context);
            
            // フェイズ1: 事前処理
            // 動的タイル出力の事前処理を列挙します。
            onTileGenerateStarts = new IOnTileGenerateStart[]
            {
                tileEditorProcedure, // エディタ上での準備
                tileToPrefab, // タイルをプレハブにします。
                setupBuildPath, // Addressableビルドパスを設定
                // tileManagerGenerator, // 古いTileManagerを消します。
                tileAddressableConfigMaker, // Addressableの設定を行います。
                // setReferencePointToSameIfExist // 既存のタイルがあればそれと同じ基準点を使うようにします。
            };
            
            // フェイズ2: タイル生成
            // 1タイル生成ごとの処理を列挙します。
            // var onOneTileImported = new IOnOneTileImported[]
            // {
                // generateOneTile // 1タイル生成ごとに呼ばれます。
            // };
            
            // フェイズ3: ビルド直前
            // タイルをビルドする直前の処理を列挙します。
            beforeTileAssetBuilds = new IBeforeTileAssetBuild[]
            {
                // addTilesToOldMetaIfExist, // 前と同じフォルダに出力するなら追加します。前のフォルダにあるunity packageのインポートも行います。
                saveAndRegisterMetaData, // メタデータを保存・登録
                tileEditorProcedure // エディタ上での準備。処理順の都合上、配列の最後にしてください。
            };
            
            // フェイズ4: ビルド直後
            // タイルをビルドしたあとの処理を列挙します。
            afterTileAssetBuilds = new IAfterTileAssetBuild[]
            {
                // tileManagerGenerator, // TileManagerを生成します。
                tileAddressableConfigMaker, // Addressableの設定を消去し元に戻します。
                exportUnityPackage, // 生成したプレハブ群をUnityPackageとしてエクスポートします。cleanupTempFolderより前に行ってください。
                cleanUpTempFolder, // 不要なフォルダを消します。
                tileEditorProcedure // エディタ上での後始末。処理の都合上、配列の最後にしてください。
            };


            // 実行
            StartTileGeneration();
            await BeforeTileAssetsBuilds();
            AddressablesUtility.BuildAddressables(context.BuildMode);
            AfterTileAssetsBuilds();
        }

        private bool StartTileGeneration()
        {
            try
            {
                // 与えられた事前処理を実行します。
                foreach (var before in onTileGenerateStarts)
                {
                    var result = before.OnTileGenerateStart();
                    if (!result)
                    {
                        throw new Exception("動的タイルの事前処理に失敗しました。");
                    }
                }

                return true; 
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                // 与えられた事前処理の例外系を実行します。
                foreach (var before in onTileGenerateStarts)
                {
                    before.OnTileGenerateStartFailed();
                }
                return false;
            }
        }

        private async Task<bool> BeforeTileAssetsBuilds()
        {
            foreach (var before in beforeTileAssetBuilds)
            {
                var ok = await before.BeforeTileAssetBuildAsync();
                if (!ok)
                {
                    Debug.LogError("failed to exec beforeTileAssetsBuild.");
                    return false;
                }
            }

            return true;
        }

        private bool AfterTileAssetsBuilds()
        {
            foreach (var after in afterTileAssetBuilds)
            {
                bool ok = after.AfterTileAssetBuild();
                if (!ok)
                {
                    Debug.LogError("failed to exec afterTileAssetsBuild.");
                    return false;
                }
            }

            return true;
        }
    }
}