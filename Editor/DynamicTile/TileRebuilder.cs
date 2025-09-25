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
    /// これは次の手順を踏みます。
    /// ・アセットバンドル化のもととなったプレハブをロードします。
    /// ・シーンに配置されたプレハブインスタンスが外部から変更されるものとします。
    /// ・その変更をプレハブに再適用して再度アセットバンドル化します。
    /// </summary>
    public class TileRebuilder
    {
        private IOnTileGenerateStart[] onTileGenerateStarts;
        private IBeforeTileAssetBuild[] beforeTileAssetBuilds;
        private IAfterTileAssetBuild[] afterTileAssetBuilds;
        public const string EditingTilesParentName = "EditingTiles";

        [MenuItem("PLATEAU/Debug/Tile Prefabs To Scene")]
        public static void TilePrefabsToScene()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }
            new TileRebuilder().TilePrefabsToScene(manager).ContinueWithErrorCatch();
        }

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

        public async Task TilePrefabsToScene(PLATEAUTileManager manager)
        {
            var context = CreateContext(manager);
            if (context.IsExcludeAssetFolder)
            {
                // unitypackageを読み込み
                await EditorAsync.ImportPackageAsync(context.UnityPackagePath);
            }
            var prefabDir = context.AssetConfig.AssetPath;
            // 指定フォルダ配下の全プレハブをシーンへ配置
            var rootAssetPath = AssetPathUtil.NormalizeAssetPath(prefabDir);
            if (string.IsNullOrEmpty(rootAssetPath) || !AssetDatabase.IsValidFolder(rootAssetPath))
            {
                Debug.LogWarning($"Prefab directory is invalid: {rootAssetPath}");
                return;
            }

            // 親Transformを用意。古い物があれば削除。
            var prev = manager.transform.Find(EditingTilesParentName);
            if(prev != null) Object.DestroyImmediate(prev.gameObject);
            var parentGo = new GameObject(EditingTilesParentName);
            parentGo.transform.SetParent(manager.transform);
            var parent = parentGo.transform;

            // フォルダ配下の全てのPrefabを検索して配置
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootAssetPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null) continue;
                instance.transform.SetParent(parent, false);
                instance.name = Path.GetFileNameWithoutExtension(path);

                GameObjectUtil.AssureComponent<PLATEAUEditingTile>(instance);
            }

            await Task.CompletedTask;
        }
        
        
        public async Task Rebuild(PLATEAUTileManager manager)
        {
            var context = CreateContext(manager);
            
            var tileAddressableConfigMaker = new TileAddressableConfigMaker(context);
            var tileEditorProcedure = new TileGenEditorProcedure();
            var setupBuildPath = new SetUpTileBuildPath(context);
            var applyEditingTilesToPrefabs = new ApplyEditingTilesToPrefabs(context);
            var registerEditingTilePrefabsForBuild = new RegisterEditingTilePrefabsForBuild(context);
            // var addTilesToOldMetaIfExist = new AddTilesToOldMetaIfExist(context);
            // var setReferencePointToSameIfExist = new SetReferencePointSameIfExist(cityConf, context);
            // var generateOneTile = new GenerateOneTile(context, progressDisplay);
            var saveAndRegisterMetaData = new SaveAndRegisterMetaData(context);
            var initializeTileManager = new InitializeTileManagerAndFocus();
            var cleanUpTempFolder = new TileCleanupTempFolder(context);
            var exportUnityPackage = new ExportUnityPackageOfPrefabs(context);
            
            // フェイズ1: 事前処理
            // 動的タイル出力の事前処理を列挙します。
            onTileGenerateStarts = new IOnTileGenerateStart[]
            {
                tileEditorProcedure, // エディタ上での準備
                setupBuildPath, // Addressableビルドパスを設定
                // tileManagerGenerator, // 古いTileManagerを消します。
                tileAddressableConfigMaker, // Addressableの設定を行います。
                applyEditingTilesToPrefabs, // 編集中タイルをプレハブに適用します。
                registerEditingTilePrefabsForBuild, // 編集中タイルのプレハブをAddressableとメタに登録します。
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
                initializeTileManager,
                tileAddressableConfigMaker, // Addressableの設定を消去し元に戻します。
                exportUnityPackage, // 生成したプレハブ群をUnityPackageとしてエクスポートします。cleanupTempFolderより前に行ってください。
                cleanUpTempFolder, // 不要なフォルダを消します。
                new CleanupEditingTilesInScene(), // シーン上のEditingTilesをクリーンアップ
                tileEditorProcedure // エディタ上での後始末。処理の都合上、配列の最後にしてください。
            };


            // 実行
            StartTileGeneration();
            await BeforeTileAssetsBuilds();
            AddressablesUtility.BuildAddressables(AddressablesUtility.TileBuildMode.New);
            AfterTileAssetsBuilds();
        }

        private DynamicTileProcessingContext CreateContext(PLATEAUTileManager manager)
        {
            var importConf = new DynamicTileImportConfig(ImportType.DynamicTile, manager.OutputPath, true);
            return new DynamicTileProcessingContext(importConf);
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