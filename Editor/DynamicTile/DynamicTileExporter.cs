using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Addressables;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.DynamicTile.TileModule;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 都市モデルをDynamicTile用にプレハブ化し、一括でアセットバンドルとして出力する。
    /// </summary>
    internal class DynamicTileExporter : IPostTileImportProcessor
    {

        public const string AddressableLabel = "DynamicTile";
        public const string AddressableAddressBase = "PLATEAUTileMeta";
        
        
        private DynamicTileProcessingContext Context { get; }
        private readonly IProgressDisplay progressDisplay;
        
        /// <summary> タイルエクスポートの事前処理を行うクラス </summary>
        private IOnTileGenerateStart[] onTileGenerateStarts;

        /// <summary> タイルアセットのビルドの直前に行う処理 </summary>
        private IBeforeTileAssetBuild[] beforeTileAssetBuilds;
        
        /// <summary> タイルエクスポートの事後処理（ビルド後）を行うクラス </summary>
        private IAfterTileAssetBuild[] afterTileAssetBuilds;

        private IOnOneTileImported[] onOneTileImported;
        
        public DynamicTileExporter(
            DynamicTileProcessingContext context,
            IProgressDisplay progressDisplay,
            IOnTileGenerateStart[] onTileGenerateStarts,
            IOnOneTileImported[] onOneTileImported,
            IBeforeTileAssetBuild[] beforeTileAssetBuilds,
            IAfterTileAssetBuild[] afterTileAssetBuilds)
        {
            Context = context;
            this.progressDisplay = progressDisplay;
            this.onTileGenerateStarts = onTileGenerateStarts;
            this.onOneTileImported = onOneTileImported;
            this.beforeTileAssetBuilds = beforeTileAssetBuilds;
            this.afterTileAssetBuilds = afterTileAssetBuilds;
        }

        /// <summary>
        /// 動的タイルの事前処理を行います。
        /// 成否を返します。
        /// </summary>
        public bool SetupPreProcessing(CityImportConfig cityConfig)
        {
            if (cityConfig == null)
            {
                Debug.LogError("CityImportConfigがnullです。");
                return false;
            }

            PLATEAUEditorEventListener.disableProjectChangeEvent = true; // タイル生成中フラグを設定
            var succeeded = false;
            try
            {
                
                // 与えられた事前処理を実装します。
                foreach (var before in onTileGenerateStarts)
                {
                    var result = before.OnTileGenerateStart();
                    if (!result) return false;
                }
                

                if (!Context.IsValid())
                {
                    Debug.LogError("context is invalid.");
                    return false;
                }
                
                succeeded = true;
                return true; 
            }
            finally
            {
                // 失敗時は常にフラグを戻す（成功時は CompleteProcessing 側の finally で戻す）
                if (!succeeded)
                {
                    PLATEAUEditorEventListener.disableProjectChangeEvent = false;
                }
            }
        }

        

        /// <summary>
        /// メタデータを保存し、Addressableとして登録する
        /// </summary>
        /// <returns>登録されたmetaStoreのアドレスを返します</returns>
        private static string SaveAndRegisterMetaData(PLATEAUDynamicTileMetaStore metaStore, string assetPath, string groupName)
        {
            if (metaStore == null)
            {
                Debug.LogWarning("メタデータがnullです。");
                return null;
            }

            // metaStoreの名前をグループ名に基づいて変更
            string shorterGroupName = groupName.Replace(DynamicTileProcessingContext.AddressableGroupBaseName + "_", "");
            string addressName = $"{AddressableAddressBase}_{shorterGroupName}";
            
            // assetPathが既に相対パスであることを確認し、必要に応じて変換
            string normalizedAssetPath = AssetPathUtil.NormalizeAssetPath(assetPath);

            string dataPath = Path.Combine(normalizedAssetPath, addressName + ".asset");
            // Path.Combineは環境によってバックスラッシュを使うため、フォワードスラッシュに統一
            dataPath = dataPath.Replace('\\', '/');

            // 既存アセットとの衝突を回避
            // dataPath = AssetDatabase.GenerateUniqueAssetPath(dataPath);

            // 既に存在する場合は新規作成を行わない（前と同じフォルダに追加で生成するケースが該当）
            var existing = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(metaStore, dataPath);
                AssetDatabase.SaveAssets();
            }

            // メタデータをAddressableに登録
            AddressablesUtility.RegisterAssetAsAddressable(
                dataPath,
                addressName,
                groupName,
                new List<string> { AddressableLabel });
            return addressName;
        }

        /// <summary>
        /// DynamicTileの完了処理を行います（メタストア保存、Addressable処理、マネージャー設定）
        /// 成否を返します。
        /// </summary>
        public bool CompleteProcessing()
        {
            if (Context == null || !Context.IsValid())
            {
                Debug.LogError("DynamicTileProcessingContextが無効です。");
                return false;
            }

            try
            {
                
                foreach (var before in beforeTileAssetBuilds)
                {
                    before.BeforeTileAssetBuild();
                }
                
                // メタデータを保存
                var metaAddress = SaveAndRegisterMetaData(Context.MetaStore, Context.AssetConfig.AssetPath, Context.AddressableGroupName);
                

                // アセットバンドルのビルド時に「シーンを保存しますか」とダイアログが出てくるのがうっとうしいので前もって保存して抑制します。
                // 保存については処理前にダイアログでユーザーに了承を得ています。
                EditorSceneManager.SaveOpenScenes();

                // Addressablesのビルドを実行
                AddressablesUtility.BuildAddressables(false);

                // ビルド後の処理で与えられたものを実行します
                foreach (var after in afterTileAssetBuilds)
                {
                    bool result = after.AfterTileAssetBuild();
                    if (!result) return false;
                }
                
                // 一時フォルダーを削除
                CleanupTempFolder();
                
                
                // 上で自動保存しておてメタアドレスを保存しないのは中途半端なのでここでも保存します。
                EditorSceneManager.SaveOpenScenes();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"動的タイルのエクスポート中にエラーが発生しました: {ex.Message}");
                return false;
            }
            finally
            {
                PLATEAUEditorEventListener.disableProjectChangeEvent = false; // タイル生成中フラグを設定
                
                // シーンをEdit
                var scene = EditorSceneManager.GetActiveScene();
                if (!scene.isDirty)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }

        /// <summary>
        /// 一時フォルダーを削除します
        /// </summary>
        public static void CleanupTempFolder()
        {
            // FIXME 現状、Assets外のアプリビルド後に動かすためにはここはコメントアウトする必要あり
            
            // var assetPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            // if (AssetDatabase.DeleteAsset(assetPath))
            // {
            //     AssetDatabase.Refresh();
            //     Debug.Log($"一時フォルダーを削除しました: {assetPath}");
            // }
            // else
            // {
            //     Debug.Log($"一時フォルダーなし: {assetPath}"); // Assets内のケース
            // }
        }

        /// <summary>
        /// 進行中のAddressable化をキャンセルします。
        /// </summary>
        public void Cancel()
        {
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
            // Contextの破棄
            if (Context != null)
            {
                // 作成途中のAddressableグループを削除
                if (!string.IsNullOrEmpty(Context.AddressableGroupName))
                {
                    AddressablesUtility.RemoveGroup(Context.AddressableGroupName);
                }

                // 一時フォルダーを削除
                CleanupTempFolder();
            }
        }

        // IPostTileImportProcessor による処理
        #region IPostTileImportProcessor

        /// <summary>
        /// インポートしつつ動的タイルにするモードにおいて、GML1つがインポートされた後に呼ばれます。
        /// 動的タイルにします。
        /// <see cref="IPostTileImportProcessor"/> のコールバック実装です。
        /// </summary>
        public void OnTileImported(TileImportResult importResult)
        {
            foreach (var o in onOneTileImported)
            {
                o.OnOneTileImported(importResult);
            }
            
        }

        #endregion 

        //　IPostGmlImportProcessorによる処理(レガシー)
        // #region IPostGmlImportProcessor
        //
        // /// <summary>
        // /// インポートしつつ動的タイルにするモードにおいて、GML1つがインポートされた後に呼ばれます。
        // /// 動的タイルにします。
        // /// <see cref="IPostGmlImportProcessor"/>を実装するものです。
        // /// </summary>
        public void OnGmlImported(GmlImportResult importResult)
        {
            Debug.LogError("legacy OnGmlImported is called.");
        //     var placedObjects = importResult.GeneratedObjects;
        //     var meshCode = importResult.GridCode;
        //     int totalGmlCount = importResult.TotalGmlCount;
        //     if (placedObjects == null || !placedObjects.Any() || Context == null || !Context.IsValid()) return;
        //     Context.GmlCount = totalGmlCount;
        //
        //     // 実際の処理委譲
        //     ProcessCityObjects(
        //         placedObjects,
        //         Context,
        //         meshCode);
        }
        //
        // /// <summary>
        // /// 新しい都市オブジェクト処理メソッド（コールバック付き）
        // /// </summary>
        // private void ProcessCityObjects(
        //     List<GameObject> placedObjects,
        //     DynamicTileProcessingContext context,
        //     string meshCode)
        // {
        //     if (placedObjects == null || !placedObjects.Any() || context == null || !context.IsValid()) return;
        //
        //     // 都市オブジェクトを取得
        //     var cityObjectGroups = placedObjects
        //         .Select(obj => obj.GetComponent<PLATEAUCityObjectGroup>())
        //         .Where(group => group != null)
        //         .ToList();
        //
        //     if (cityObjectGroups.Count == 0) return;
        //
        //     string outputPath = context.AssetConfig?.AssetPath ?? "Assets/PLATEAUPrefabs/";
        //     var outputDirFullPath = Path.IsPathRooted(outputPath)
        //                     ? outputPath
        //                     : AssetPathUtil.GetFullPath(outputPath);
        //
        //     // ディレクトリの存在確認
        //     if (!Directory.Exists(outputDirFullPath))
        //     {
        //         Directory.CreateDirectory(outputDirFullPath);
        //     }
        //
        //     // 進捗表示
        //     int loadedGmlCount = Context.IncrementAndGetLoadedGmlCount();
        //     float progress = 10f + ((float)loadedGmlCount / Context.GmlCount) * 70f;
        //     for (int i = 0; i < cityObjectGroups.Count; i++)
        //     {
        //         var cityObject = cityObjectGroups[i];
        //         if (cityObject == null) continue;
        //
        //         // オブジェクト名にメッシュコードを追加
        //         cityObject.name = meshCode + "_" + cityObject.name;
        //
        //         // プログレスバー更新
        //         progressDisplay?.SetProgress(ImportToDynamicTile.TileProgressTitle, progress,
        //             $"動的タイルを生成中... {cityObject.name}");
        //
        //         // 個別のオブジェクトを処理
        //         ProcessCityObject(
        //             cityObject,
        //             context.AssetConfig,
        //             context.AddressableGroupName,
        //             context.MetaStore,
        //             errorMessage => Debug.LogError($"DynamicTileExporter error: {errorMessage}")
        //         );
        //     }
        // }
        //
        // /// <summary>
        // /// 都市オブジェクト１つを処理し、プレハブ化・Addressable登録を行う
        // /// </summary>
        // /// <param name="cityObject">処理対象の都市オブジェクト</param>
        // /// <param name="assetConfig">変換設定</param>
        // /// <param name="groupName">Addressableグループ名</param>
        // /// <param name="metaStore">メタデータストア</param>
        // /// <param name="onError">エラー時のコールバック</param>
        // /// <returns>処理が成功した場合はtrue</returns>
        // public static bool ProcessCityObject(
        //     PLATEAUCityObjectGroup cityObject,
        //     ConvertToAssetConfig assetConfig,
        //     string groupName,
        //     PLATEAUDynamicTileMetaStore metaStore,
        //     Action<string> onError = null)
        // {
        //     if (cityObject == null || cityObject.gameObject == null)
        //     {
        //         Debug.LogWarning($"GameObjectがnullです。");
        //         return false;
        //     }
        //
        //     assetConfig.SrcGameObj = cityObject.gameObject;
        //
        //     var baseFolderAssetPath = Path.Combine(assetConfig.AssetPath, $"{cityObject.gameObject.name}_11");
        //     var saveFolderAssetPath = AssetPathUtil.CreateDirectoryWithIncrementalNameIfExist(baseFolderAssetPath);
        //
        //     var saveFolderFullPath = AssetPathUtil.GetFullPath(saveFolderAssetPath);
        //     var convertedObject = PrepareAndConvert(assetConfig, saveFolderFullPath, onError);
        //     if (convertedObject == null)
        //     {
        //         Debug.LogWarning($"{cityObject.gameObject.name} の変換に失敗しました。");
        //         return false;
        //     }
        //
        //     string prefabPath = saveFolderAssetPath + ".prefab";
        //     var prefabAsset = PrefabUtility.SaveAsPrefabAsset(convertedObject, prefabPath);
        //     if (prefabAsset == null)
        //     {
        //         Debug.LogWarning($"{convertedObject.name} プレハブの保存に失敗しました。");
        //         return false;
        //     }
        //
        //     //低解像度のプレハブを生成 (Tile生成処理追加後に仕様が変わるのでとりあえずベタ実装）
        //     var prefab1Data = new MultiResolutionPrefabCreator.Result { SavePath = prefabPath, Prefab = prefabAsset, Bounds = convertedObject.GetComponentInChildren<Renderer>() == null ? default : convertedObject.GetComponentInChildren<Renderer>().bounds, ZoomLevel = 11 }; // ↑で生成済みなのでResultのみ
        //     var prefab2Data = MultiResolutionPrefabCreator.CreateFromGameObject(convertedObject, assetConfig.AssetPath, 2, 10);
        //     var prefab4Data = MultiResolutionPrefabCreator.CreateFromGameObject(convertedObject, assetConfig.AssetPath, 4, 9);
        //
        //     var prefabs = new List<MultiResolutionPrefabCreator.Result> { prefab1Data, prefab2Data, prefab4Data };
        //     prefabs = prefabs.Where(p => p != null && p.Prefab != null).ToList();
        //
        //     RegisterAssets(prefabs, groupName, cityObject, metaStore);
        //
        //     // シーン上のオブジェクトを削除
        //     GameObject.DestroyImmediate(convertedObject);
        //
        //     return true;
        // }
        //
        // /// <summary>
        // /// アセット登録
        // /// </summary>
        // /// <param name="results"></param>
        // /// <param name="groupName"></param>
        // /// <param name="cityObject"></param>
        // /// <param name="metaStore"></param>
        // private static void RegisterAssets(
        //     IList<MultiResolutionPrefabCreator.Result> results,
        //     string groupName,
        //     PLATEAUCityObjectGroup cityObject,
        //     PLATEAUDynamicTileMetaStore metaStore)
        // {
        //     foreach (var res in results)
        //     {
        //         if (res == null || res.Prefab == null)
        //         {
        //             Debug.Log($"Skipped null prefab.");
        //             continue;
        //         }
        //
        //         var prefabAsset = res.Prefab;
        //         string prefabPath;
        //         if (string.IsNullOrEmpty(res.SavePath))
        //         {
        //             prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
        //         }
        //         else
        //         {
        //             prefabPath = Path.IsPathRooted(res.SavePath)
        //             ? AssetPathUtil.GetAssetPath(res.SavePath)
        //             : AssetPathUtil.NormalizeAssetPath(res.SavePath);
        //         }
        //
        //         var bounds = res.Bounds;
        //         var zoomLevel = res.ZoomLevel;
        //         var lod = cityObject.Lod;
        //
        //         // プレハブをAddressableに登録
        //         // TODO : タイルごとにAddress名を設定する
        //         var address = prefabAsset.name;
        //         AddressablesUtility.RegisterAssetAsAddressable(
        //             prefabPath,
        //             address,
        //             groupName,
        //             new List<string> { AddressableLabel });
        //
        //         Debug.Log($"プレハブをAddressableに登録しました: {address} path : {prefabPath}");
        //
        //         // メタ情報を登録
        //         metaStore.AddMetaInfo(address, bounds, lod, zoomLevel);
        //     }
        // }
        //
        // #endregion

    }
    
    
}