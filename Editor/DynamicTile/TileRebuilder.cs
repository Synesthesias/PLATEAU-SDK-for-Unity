using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile.TileModule;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.DynamicTile
{
    /// <summary>
    /// タイルを再生成します。
    /// これは次の手順を踏みます。
    /// ・<see cref="TilePrefabsToScene(PLATEAU.DynamicTile.PLATEAUTileManager)"/>を利用し、セットバンドル化のもととなったプレハブをロードします。
    /// ・シーンに配置されたプレハブインスタンスが外部から変更されるものとします。
    /// ・<see cref="Rebuild"/>を利用し、その変更をプレハブに再適用して再度アセットバンドル化します。
    /// この処理を動作確認するためにUnityのメニューバーから実行する機能が<see cref="TileRebuilderMenuItem"/>にあります。
    /// </summary>
    public class TileRebuilder
    {
        private IOnTileGenerateStart[] onTileGenerateStarts;
        private IBeforeTileAssetBuild[] beforeTileAssetBuilds;
        private IAfterTileAssetBuild[] afterTileAssetBuilds;
        private IAfterTileAssetBuild[] afterTileAssetCancelled;
        private IOnTileGenerationCancelled[] onTileGenerationCancelled;
        private IOnTileBuildFailed[] onTileBuildFailed;
        public const string EditingTilesParentName = "EditingTiles";

        /// <summary>
        /// タイルの元となったプレハブをすべてシーンに配置します。
        /// </summary>
        public async Task TilePrefabsToScene(PLATEAUTileManager manager, CancellationToken ct)
        {
            await TilePrefabsToSceneInternal(manager, null, ct);
        }
        
        /// <summary>
        /// タイルの元となったプレハブのうち、引数で指定されたものをシーンに配置します。
        /// </summary>
        public async Task TilePrefabsToScene(PLATEAUTileManager manager, IEnumerable<PLATEAUDynamicTile> tiles, CancellationToken ct)
        {
            await TilePrefabsToSceneInternal(manager, tiles, ct);
        }

        private async Task TilePrefabsToSceneInternal(PLATEAUTileManager manager, IEnumerable<PLATEAUDynamicTile> tiles, CancellationToken ct)
        {
            var context = CreateContext(manager);
            if (context.IsExcludeAssetFolder)
            {
                // unitypackageを読み込み
                var ok = await EditorAsync.ImportPackageAsync(context.UnityPackagePath, ct);
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

            var targetSet = new HashSet<string>();
            bool isFilter = tiles != null;
            if (isFilter)
            {
                foreach (var t in tiles) if (t != null && !string.IsNullOrEmpty(t.Address)) targetSet.Add(t.Address);
            }

            // フォルダ配下の全てのPrefabを検索して配置
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootAssetPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                var address = Path.GetFileNameWithoutExtension(path);
                if (isFilter && (string.IsNullOrEmpty(address) || !targetSet.Contains(address))) continue;
                
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null) continue;
                instance.transform.SetParent(parent, false);
                instance.name = Path.GetFileNameWithoutExtension(path);

                GameObjectUtil.AssureComponent<PLATEAUEditingTile>(instance);
            }

            //　Rebuildを行わない場合のキャンセル処理
            afterTileAssetCancelled = new IAfterTileAssetBuild[]
            {
                new TileCleanupTempFolder(context), // 不要なフォルダを消します。
                new CleanupEditingTilesInScene(), // シーン上のEditingTilesをクリーンアップ
                new TileRebuildCancellation()　// タイル生成中フラグを元に戻す
            };

            await Task.CompletedTask;
        }

        /// <summary>
        /// 編集中のプレハブに対する変更を適用し、すべて再ビルドします。
        /// </summary>
        public async Task Rebuild(PLATEAUTileManager manager)
        {
            await RebuildInternal(manager, null);
        }

        /// <summary>
        /// 編集中のプレハブに対する変更を適用し、引数で指定されたタイルのみを対象に差分ビルドします。
        /// </summary>
        public async Task RebuildByTiles(PLATEAUTileManager manager, IEnumerable<PLATEAUDynamicTile> tiles)
        {
            await RebuildInternal(manager, tiles);
        }

        private async Task RebuildInternal(PLATEAUTileManager manager, IEnumerable<PLATEAUDynamicTile> tiles)
        {
            var dummyCancelToken = new CancellationTokenSource().Token;
            var context = CreateContext(manager);
            if (tiles != null)
            {
                context.TargetAddresses = new HashSet<string>();
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
            var deleteOldBundles = new DeleteOldBundlesBeforeBuild(context);
            var restoreAddressablesState = new RestoreAddressablesStateForRebuild(context);

            // 事前処理
            // 動的タイル出力の事前処理を列挙します。
            onTileGenerateStarts = new IOnTileGenerateStart[]
            {
                tileEditorProcedure, // エディタ上での準備
                restoreAddressablesState, // Addressableの差分ビルド設定を復元
                setupBuildPath, // Addressableビルドパスを設定
                tileAddressableConfigMaker, // Addressableの設定を行います。
                applyEditingTilesToPrefabs, // 編集中タイルをプレハブに適用します。
                registerEditingTilePrefabsForBuild, // 編集中タイルのプレハブをAddressableとメタに登録します。
            };
            
            // ビルド直前
            // タイルをビルドする直前の処理を列挙します。
            beforeTileAssetBuilds = new IBeforeTileAssetBuild[]
            {
                saveAndRegisterMetaData, // メタデータを保存・登録
                new RemoveEditingTileComponentBeforeBuild(), // ビルド直前に PLATEAUEditingTile を除去
                deleteOldBundles, // 増分ビルドによって古くなるバンドルファイルを削除
                tileEditorProcedure // エディタ上での準備。処理順の都合上、配列の最後にしてください。
            };
            
            // ビルド直後
            // タイルをビルドしたあとの処理を列挙します。
            afterTileAssetBuilds = new IAfterTileAssetBuild[]
            {
                tileAddressableConfigMaker, // Addressableの設定を消去し元に戻します。
                exportUnityPackage, // 生成したプレハブ群をUnityPackageとしてエクスポートします。cleanupTempFolderより前に行ってください。
                cleanUpTempFolder, // 不要なフォルダを消します。
                new CleanupEditingTilesInScene(), // シーン上のEditingTilesをクリーンアップ
                initializeTileManager,
                tileEditorProcedure // エディタ上での後始末。処理の都合上、配列の最後にしてください。
            };

            onTileGenerationCancelled = new IOnTileGenerationCancelled[]
            {
                tileEditorProcedure
            };

            onTileBuildFailed = new IOnTileBuildFailed[] { tileEditorProcedure };


            // 実行
            if (!StartTileGeneration())
            {
                Debug.LogError("failed on startTileGeneration.");
                Cancel();
                return;
            }

            if (!await BeforeTileAssetsBuilds(dummyCancelToken))
            {
                Debug.LogError("failed on BeforeTileAssetsBuilds.");
                Cancel();
                return;
            }
            
            var prevStripUnusedMeshComponents = PlayerSettings.stripUnusedMeshComponents; // 元の設定を保存
            try
            {
                PlayerSettings.stripUnusedMeshComponents = false; // UV4を使うために必要
                // addressableビルド
                AddressablesUtility.BuildAddressables(context.BuildMode);
            }
            catch (Exception ex) 
            {
                Cancel();
                foreach(var f in onTileBuildFailed) f.OnTileBuildFailed();
                Debug.LogError($"Addressables build failed: {ex}");
                return;
            }
            finally
            {
                PlayerSettings.stripUnusedMeshComponents = prevStripUnusedMeshComponents; // 元に戻す
            }

            if (!AfterTileAssetsBuilds())
            {
                Debug.LogError("failed on AfterTileAssetsBuilds.");
                Cancel();
                return;
            }
        }

        /// <summary>
        /// GameObjectをプレハブとして保存します。
        /// 保存先はDynamicTileProcessingContext.PrefabsTempSavePathとします。
        /// </summary>
        public async Task SavePrefabAsset(GameObject src)
        {
            string outputPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            AssetPathUtil.CreateDirectoryIfNotExist(AssetPathUtil.GetFullPath(outputPath)); // ルートを確実に作成
            string saveFolderPath = Path.Combine(outputPath, src.name);
            var saveFolderFullPath = Path.GetFullPath(saveFolderPath);
            var saveFolderTempPath = saveFolderFullPath + "_temp";

            var assetConfig = ConvertToAssetConfig.DefaultValue;
            assetConfig.SrcGameObj = src;
            assetConfig.AssetPath = saveFolderTempPath;
            assetConfig.ConvertFromFbx = true;
            assetConfig.ConvertTerrain = true;

            var comp = src.GetComponent<PLATEAUEditingTile>();
            if (comp != null)
            {
                Object.DestroyImmediate(comp, true);
            }

            if (Directory.Exists(saveFolderTempPath))
                Directory.Delete(saveFolderTempPath, true);
            AssetPathUtil.CreateDirectoryIfNotExist(saveFolderTempPath);

            var convertedObjects = new ConvertToAsset().ConvertCore(assetConfig, new DummyProgressBar());
            if (convertedObjects == null || convertedObjects.Count == 0 || convertedObjects[0] == null)
            {
                Debug.LogWarning($"{src.name} の変換に失敗しました。");
                return;
            }
            // Prefab 保存
            string prefabPath = saveFolderPath + ".prefab";
            var rootGo = convertedObjects.FirstOrDefault();
            var prefabAsset = PrefabUtility.SaveAsPrefabAsset(rootGo, prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"{src.name} プレハブの保存に失敗しました。");
                return;
            }
            // Temp フォルダを AssetDatabase 経由で移動し、DB を同期
            var tempAssetsPath = AssetPathUtil.GetAssetPath(saveFolderTempPath);
            var finalAssetsPath = AssetPathUtil.GetAssetPath(saveFolderFullPath);
            if (AssetDatabase.IsValidFolder(finalAssetsPath))
            {
                AssetDatabase.DeleteAsset(finalAssetsPath);
            }
            var moveError = AssetDatabase.MoveAsset(tempAssetsPath, finalAssetsPath);
            if (!string.IsNullOrEmpty(moveError))
            {
                Debug.LogWarning($"フォルダの移動に失敗しました: {moveError}");
            }
            AssetDatabase.Refresh();

            await Task.CompletedTask;
        }

        /// <summary>
        /// TilePrefabsToScene実行後にRebuildせずに終了したい場合のキャンセル処理
        /// Rebuild時に作成されたtempデータを削除
        /// </summary>
        /// <returns></returns>
        public bool CancelRebuild()
        {
            if (afterTileAssetCancelled == null || afterTileAssetCancelled.Length <= 0)
                return false;

            foreach (var after in afterTileAssetCancelled)
            {
                bool ok = after.AfterTileAssetBuild();
                if (!ok)
                {
                    Debug.LogError("failed to exec afterTileAssetCancelled.");
                    return false;
                }
            }
            return true;
        }

        private void Cancel()
        {
            foreach(var c in onTileGenerationCancelled) c.OnTileGenerationCancelled();
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

        private async Task<bool> BeforeTileAssetsBuilds(CancellationToken ct)
        {
            foreach (var before in beforeTileAssetBuilds)
            {
                var ok = await before.BeforeTileAssetBuildAsync(ct);
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