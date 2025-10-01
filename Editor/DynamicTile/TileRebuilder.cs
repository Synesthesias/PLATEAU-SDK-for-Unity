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
    /// ・<see cref="TilePrefabsToSceneAll"/>を利用し、セットバンドル化のもととなったプレハブをロードします。
    /// ・シーンに配置されたプレハブインスタンスが外部から変更されるものとします。
    /// ・<see cref="Rebuild"/>を利用し、その変更をプレハブに再適用して再度アセットバンドル化します。
    /// </summary>
    public class TileRebuilder
    {
        private IOnTileGenerateStart[] onTileGenerateStarts;
        private IBeforeTileAssetBuild[] beforeTileAssetBuilds;
        private IAfterTileAssetBuild[] afterTileAssetBuilds;
        public const string EditingTilesParentName = "EditingTiles";

        [MenuItem("PLATEAU/Debug/Tile Prefabs To Scene (All)")]
        public static void TilePrefabsToSceneAll()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }
            new TileRebuilder().TilePrefabsToScene(manager).ContinueWithErrorCatch();
        }

        [MenuItem("PLATEAU/Debug/Rebuild Tiles (All)")]
        public static void RebuildInSceneAll()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            new TileRebuilder().Rebuild(manager).ContinueWithErrorCatch();
        }

        [MenuItem("PLATEAU/Debug/Tile Prefabs To Scene (Selected Tiles)")]
        public static void TilePrefabsToSceneSelected()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            var selected = GetSelectedTiles(manager);
            if (selected == null || selected.Count == 0)
            {
                Debug.LogWarning("No selected tiles found in the scene.");
                return;
            }

            new TileRebuilder().TilePrefabsToScene(manager, selected).ContinueWithErrorCatch();
        }

        [MenuItem("PLATEAU/Debug/Rebuild Tiles (Selected Tiles)")]
        public static void RebuildInSceneSelected()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            var selected = GetSelectedTiles(manager);
            if (selected == null || selected.Count == 0)
            {
                Debug.LogWarning("No selected tiles found in the scene.");
                return;
            }

            new TileRebuilder().RebuildByTiles(manager, selected).ContinueWithErrorCatch();
        }

        private static System.Collections.Generic.List<PLATEAUDynamicTile> GetSelectedTiles(PLATEAUTileManager manager)
        {
            var result = new System.Collections.Generic.List<PLATEAUDynamicTile>();
            var selectedGos = Selection.gameObjects;
            if (selectedGos == null || selectedGos.Length == 0) return result;

            foreach (var tile in manager.DynamicTiles)
            {
                if (tile == null) continue;
                var root = tile.LoadedObject;
                if (root == null) continue;
                var rootTf = root.transform;
                foreach (var go in selectedGos)
                {
                    if (go == null) continue;
                    var tf = go.transform;
                    if (tf == rootTf || tf.IsChildOf(rootTf))
                    {
                        result.Add(tile);
                        break;
                    }
                }
            }
            return result;
        }

        public async Task TilePrefabsToScene(PLATEAUTileManager manager)
        {
            var context = CreateContext(manager);
            if (context.IsExcludeAssetFolder)
            {
                // unitypackageを読み込み
                var ok = await EditorAsync.ImportPackageAsync(context.UnityPackagePath);
                if (!ok)
                {
                    Debug.LogError("failed to import unity package.");
                    return;
                }
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
        
        /// <summary>
        /// 指定されたタイルのみプレハブをシーンへ配置します。
        /// </summary>
        public async Task TilePrefabsToScene(PLATEAUTileManager manager, System.Collections.Generic.IEnumerable<PLATEAUDynamicTile> tiles)
        {
            var context = CreateContext(manager);
            if (context.IsExcludeAssetFolder)
            {
                var ok = await EditorAsync.ImportPackageAsync(context.UnityPackagePath);
                if (!ok)
                {
                    Debug.LogError("failed to import unity package.");
                    return;
                }
            }

            var prefabDir = context.AssetConfig.AssetPath;
            var rootAssetPath = AssetPathUtil.NormalizeAssetPath(prefabDir);
            if (string.IsNullOrEmpty(rootAssetPath) || !AssetDatabase.IsValidFolder(rootAssetPath))
            {
                Debug.LogWarning($"Prefab directory is invalid: {rootAssetPath}");
                return;
            }

            var prev = manager.transform.Find(EditingTilesParentName);
            if(prev != null) Object.DestroyImmediate(prev.gameObject);
            var parentGo = new GameObject(EditingTilesParentName);
            parentGo.transform.SetParent(manager.transform);
            var parent = parentGo.transform;

            var targetSet = new System.Collections.Generic.HashSet<string>();
            if (tiles != null)
            {
                foreach (var t in tiles) if (t != null && !string.IsNullOrEmpty(t.Address)) targetSet.Add(t.Address);
            }

            // 指定が無ければ全件従来通り
            if (targetSet.Count == 0)
            {
                await TilePrefabsToScene(manager);
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootAssetPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                var address = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(address) || !targetSet.Contains(address)) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null) continue;
                instance.transform.SetParent(parent, false);
                instance.name = address;

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
            var saveAndRegisterMetaData = new SaveAndRegisterMetaData(context);
            var initializeTileManager = new InitializeTileManagerAndFocus();
            var cleanUpTempFolder = new TileCleanupTempFolder(context);
            var exportUnityPackage = new ExportUnityPackageOfPrefabs(context);
            
            // 事前処理
            // 動的タイル出力の事前処理を列挙します。
            onTileGenerateStarts = new IOnTileGenerateStart[]
            {
                tileEditorProcedure, // エディタ上での準備
                setupBuildPath, // Addressableビルドパスを設定
                tileAddressableConfigMaker, // Addressableの設定を行います。
                applyEditingTilesToPrefabs, // 編集中タイルをプレハブに適用します。
                registerEditingTilePrefabsForBuild, // 編集中タイルのプレハブをAddressableとメタに登録します。
            };
            
            // ビルド直前
            // タイルをビルドする直前の処理を列挙します。
            beforeTileAssetBuilds = new IBeforeTileAssetBuild[]
            {
                new DeleteOldBundlesBeforeBuild(context), // 旧bundleを削除
                saveAndRegisterMetaData, // メタデータを保存・登録
                new RemoveEditingTileComponentBeforeBuild(), // ビルド直前に PLATEAUEditingTile を除去
                tileEditorProcedure // エディタ上での準備。処理順の都合上、配列の最後にしてください。
            };
            
            // ビルド直後
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
            if (!StartTileGeneration())
            {
                Debug.LogError("failed on startTileGeneration.");
                return;
            }

            if (!await BeforeTileAssetsBuilds())
            {
                Debug.LogError("failed on BeforeTileAssetsBuilds.");
                return;
            }
            AddressablesUtility.BuildAddressables(context.BuildMode);
            if (!AfterTileAssetsBuilds())
            {
                Debug.LogError("failed on AfterTileAssetsBuilds.");
                return;
            }
        }

        /// <summary>
        /// 指定されたタイルのみを対象に差分リビルドします。
        /// </summary>
        public async Task RebuildByTiles(PLATEAUTileManager manager, System.Collections.Generic.IEnumerable<PLATEAUDynamicTile> tiles)
        {
            var context = CreateContext(manager);
            context.TargetAddresses = new System.Collections.Generic.HashSet<string>();
            if (tiles != null)
            {
                foreach (var t in tiles)
                {
                    if (t == null || string.IsNullOrEmpty(t.Address)) continue;
                    context.TargetAddresses.Add(t.Address);
                }
            }

            var tileAddressableConfigMaker = new TileAddressableConfigMaker(context);
            var tileEditorProcedure = new TileGenEditorProcedure();
            var setupBuildPath = new SetUpTileBuildPath(context);
            var applyEditingTilesToPrefabs = new ApplyEditingTilesToPrefabs(context);
            var registerEditingTilePrefabsForBuild = new RegisterEditingTilePrefabsForBuild(context);
            var saveAndRegisterMetaData = new SaveAndRegisterMetaData(context);
            var initializeTileManager = new InitializeTileManagerAndFocus();
            var cleanUpTempFolder = new TileCleanupTempFolder(context);
            var exportUnityPackage = new ExportUnityPackageOfPrefabs(context);

            onTileGenerateStarts = new IOnTileGenerateStart[]
            {
                tileEditorProcedure,
                setupBuildPath,
                tileAddressableConfigMaker,
                applyEditingTilesToPrefabs,
                registerEditingTilePrefabsForBuild,
            };

            beforeTileAssetBuilds = new IBeforeTileAssetBuild[]
            {
                new DeleteOldBundlesBeforeBuild(context),
                saveAndRegisterMetaData,
                new RemoveEditingTileComponentBeforeBuild(),
                tileEditorProcedure,
            };

            afterTileAssetBuilds = new IAfterTileAssetBuild[]
            {
                initializeTileManager,
                tileAddressableConfigMaker,
                exportUnityPackage,
                cleanUpTempFolder,
                new CleanupEditingTilesInScene(),
                tileEditorProcedure,
            };

            if (!StartTileGeneration())
            {
                Debug.LogError("failed on startTileGeneration.");
                return;
            }

            if (!await BeforeTileAssetsBuilds())
            {
                Debug.LogError("failed on BeforeTileAssetsBuilds.");
                return;
            }
            AddressablesUtility.BuildAddressables(context.BuildMode);
            if (!AfterTileAssetsBuilds())
            {
                Debug.LogError("failed on AfterTileAssetsBuilds.");
                return;
            }
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