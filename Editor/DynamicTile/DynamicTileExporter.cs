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
    }
}