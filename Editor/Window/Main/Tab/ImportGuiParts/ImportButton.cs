using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.ImportGuiParts
{
    /// <summary>
    /// 「モデルをインポート」ボタンの描画と実行を行います。
    /// </summary>
    internal static class ImportButton
    {
        /// <summary>
        /// 「モデルをインポート」のキャンセル用Tokenソース
        /// インポートタスクは1本なので発行するトークンも１つ
        /// </summary>
        private static CancellationTokenSource cancellationTokenSrc;

        private static int numCurrentRunningTasks;

        /// <summary>
        /// 「モデルをインポート」ボタンの描画と実行を行います。
        /// </summary>
        public static void Draw(CityImportConfig config, IProgressDisplay progressDisplay)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (numCurrentRunningTasks <= 0)
                {
                    // ボタンを描画します。
                    if (PlateauEditorStyle.MainButton("モデルをインポート"))
                    {
                        // ボタンを実行します。
                        Interlocked.Increment(ref numCurrentRunningTasks);

                        cancellationTokenSrc = new CancellationTokenSource();

                        // Addressableの事前処理
                        var context = ProcessAddressablePreSetup(progressDisplay, config);
    
                        // ここでインポートします。
                        var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token, 
                            (placedObjects) =>
                            {
                                if (context != null && context.IsValid())
                                {
                                    // 各GMLファイルのインポート完了時に都市オブジェクトを処理
                                    ProcessCityObjectsForAddressable(placedObjects, context, progressDisplay);
                                }
                            });
                        
                        task.ContinueWith((_) =>
                        {
                            if (context != null && context.IsValid())
                            {
                                // Addressableの事後処理
                                DynamicTileExporter.CompleteDynamicTileProcessing(context);
                            }

                            Interlocked.Decrement(ref numCurrentRunningTasks);
                        });
                        
                        task.ContinueWithErrorCatch();
                    }
                }
                else if (cancellationTokenSrc?.Token != null && cancellationTokenSrc.Token.IsCancellationRequested)
                {
                    if (PlateauEditorStyle.CancelButton("キャンセル中…")){}
                }
                else
                {
                    //Cancel ボタンを描画します。
                    if (PlateauEditorStyle.CancelButton("インポートをキャンセルする"))
                    {
                        bool dialogueResult = Dialogue.Display($"インポートをキャンセルしますか？", "はい", "いいえ");
                        if (dialogueResult)
                        {
                            cancellationTokenSrc?.Cancel();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Addressableの事前処理を実行します。
        /// </summary>
        /// <returns>成功時はDynamicTileProcessingContext、失敗時はnullを返します。</returns>
        private static DynamicTileProcessingContext ProcessAddressablePreSetup(IProgressDisplay progressDisplay, CityImportConfig config)
        {
            progressDisplay.SetProgress("Addressable事前処理", 0f, "Addressableセットアップ中...");
            var context = DynamicTileExporter.SetupAddressablePreProcessing(
                config.DynamicTileImportConfig);

            if (context == null || !context.IsValid())
            {
                Debug.LogError("Addressableの事前処理に失敗しました。");
                return null;
            }

            progressDisplay.SetProgress("Addressable事前処理", 100f, "完了");
            return context;
        }

        /// <summary>
        /// 都市オブジェクトをAddressableアセットとして処理します。
        /// </summary>
        private static void ProcessCityObjectsForAddressable(
            List<GameObject> placedObjects,
            DynamicTileProcessingContext context,
            IProgressDisplay progressDisplay)
        {
            if (placedObjects == null || !placedObjects.Any() || context == null || !context.IsValid()) return;

            // 都市オブジェクトを取得
            var cityObjectGroups = placedObjects
                .Select(obj => obj.GetComponent<PLATEAUCityObjectGroup>())
                .Where(group => group != null)
                .ToList();

            string outputPath = context.AssetConfig?.AssetPath ?? "Assets/PLATEAUPrefabs/";

            // ディレクトリの存在確認
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            // 都市オブジェクトを一括処理
            DynamicTileExporter.ProcessCityObjects(
                cityObjectGroups,
                context,
                errorMessage => Debug.LogError($"DynamicTileExporter error: {errorMessage}")
            );
        }
    }
}
