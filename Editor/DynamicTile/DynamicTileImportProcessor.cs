using PLATEAU.CityImport.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 動的タイルインポート処理を担当するクラス
    /// </summary>
    public static class DynamicTileImportProcessor
    {
        private const string DynamicTileGenerationText = "動的タイル生成処理";
        
        /// <summary>
        /// 動的タイルインポートの事前処理を調整します。
        /// </summary>
        /// <returns>成功時はDynamicTileProcessingContext、失敗時はnullを返します。</returns>
        public static DynamicTileProcessingContext SetupPreProcessing(IProgressDisplay progressDisplay, CityImportConfig config)
        {
            try
            {
                Debug.Log("DynamicTileImportProcessor.SetupPreProcessing 開始");
                progressDisplay?.SetProgress(DynamicTileGenerationText, 0f, "動的タイル生成を開始中...");
            
                if (config?.DynamicTileImportConfig == null)
                {
                    Debug.LogError("CityImportConfigまたはDynamicTileImportConfigがnullです。");
                    return null;
                }

                var context = DynamicTileExporter.SetupPreProcessing(config.DynamicTileImportConfig);
                if (context == null || !context.IsValid())
                {
                    Debug.LogError("動的タイルの事前処理に失敗しました。");
                    return null;
                }
            
                progressDisplay?.SetProgress(DynamicTileGenerationText, 10f, "動的タイル生成を開始中...");
                Debug.Log("DynamicTileImportProcessor.SetupPreProcessing 完了");
                return context;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SetupPreProcessing中にエラーが発生しました: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 都市オブジェクトの処理を調整します。
        /// </summary>
        public static void ProcessCityObjects(
            List<GameObject> placedObjects,
            DynamicTileProcessingContext context,
            IProgressDisplay progressDisplay,
            string meshCode,
            int loadedGmlCount)
        {
            if (placedObjects == null || !placedObjects.Any() || context == null || !context.IsValid()) return;

            // 進捗計算: 10%から始まり、GML処理完了ごとに進む（最大80%）
            float progress = 10f + ((float)loadedGmlCount / context.GmlCount) * 70f;

            // 実際の処理をDynamicTileExporterに委譲
            DynamicTileExporter.ProcessCityObjects(
                placedObjects,
                context,
                meshCode,
                (cityObjectName) => 
                    progressDisplay?.SetProgress(DynamicTileGenerationText, progress, $"動的タイルを生成中... {cityObjectName}")
            );
        }
        
        /// <summary>
        /// インポート完了後の事後処理を調整します。
        /// </summary>
        public static async void HandleCompletionAsync(
            System.Threading.Tasks.Task task, 
            DynamicTileProcessingContext context, 
            IProgressDisplay progressDisplay,
            Action onFinally = null)
        {
            try
            {
                await task;
                
                if (context != null && context.IsValid() && context.GmlCount > 0)
                {
                    progressDisplay?.SetProgress(DynamicTileGenerationText, 90f, "最終処理を実行中...");
                    
                    // 実際の完了処理をDynamicTileExporterに委譲
                    DynamicTileExporter.CompleteProcessing(context);
                    
                    progressDisplay?.SetProgress(DynamicTileGenerationText, 100f, "動的タイル生成完了");
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("動的タイルインポート処理がキャンセルされました。");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"動的タイルインポート処理中にエラーが発生しました: {ex.Message}");
            }
            finally
            {
                onFinally?.Invoke();
            }
        }

        /// <summary>
        /// キャンセル時の共通処理
        /// </summary>
        public static void HandleCancellation(
            IProgressDisplay progressDisplay,
            DynamicTileProcessingContext context = null,
            float progress = 0f)
        {
            progressDisplay?.SetProgress(DynamicTileGenerationText, progress, "キャンセルされました。");
            Debug.Log("動的タイル生成がキャンセルされました。");
            
            // Contextの破棄
            if (context != null)
            {
                // 作成途中のAddressableグループを削除
                if (!string.IsNullOrEmpty(context.AddressableGroupName))
                {
                    AddressablesUtility.RemoveGroup(context.AddressableGroupName);
                }
                
                // 一時フォルダーを削除
                DynamicTileExporter.CleanupTempFolder();
            }
        }
    }
}