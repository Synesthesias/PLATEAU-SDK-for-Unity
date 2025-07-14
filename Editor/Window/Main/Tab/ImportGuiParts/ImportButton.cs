using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        /// 動的タイル処理用のコンテキスト（キャンセル時のクリーンアップ用）
        /// </summary>
        private static DynamicTileProcessingContext currentAddressableContext;

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
                        // タスク数をインクリメントし、キャンセルトークンを初期化
                        Interlocked.Increment(ref numCurrentRunningTasks);
                        cancellationTokenSrc = new CancellationTokenSource();

                        if (config.DynamicTileImportConfig.ImportType == ImportType.DynamicTile)
                        {
                            // 動的タイル形式でのインポートを実行します。
                            ExecuteDynamicTileImport(config, progressDisplay);
                        }
                        else
                        {
                            // シーン上へのインポートを実行します。
                            ExecuteNormalImport(config, progressDisplay);
                        }
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

                            if (config.DynamicTileImportConfig.ImportType != ImportType.DynamicTile ||
                                currentAddressableContext == null)
                            {
                                return;
                            }
                            // 動的タイルインポートのクリーンアップ
                            DynamicTileImportProcessor.HandleCancellation(progressDisplay, currentAddressableContext);
                            currentAddressableContext = null; // クリーンアップ後にクリア
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通常のインポート処理を実行します
        /// </summary>
        private static void ExecuteNormalImport(CityImportConfig config, IProgressDisplay progressDisplay)
        {
            var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token, null);
            
            task.ContinueWith((_) => { Interlocked.Decrement(ref numCurrentRunningTasks); });
            task.ContinueWithErrorCatch();
        }

        /// <summary>
        /// 動的タイルのインポート処理を実行します
        /// </summary>
        private static void ExecuteDynamicTileImport(CityImportConfig config, IProgressDisplay progressDisplay)
        {
            // 動的タイルのバリデーション
            if (string.IsNullOrEmpty(config.DynamicTileImportConfig.OutputPath))
            {
                Dialogue.Display("動的タイル（Addressable出力）を選択する場合は、出力先を指定してください", "OK");
                Interlocked.Decrement(ref numCurrentRunningTasks);
                return;
            }

            // 事前処理を実行
            currentAddressableContext = DynamicTileImportProcessor.SetupPreProcessing(progressDisplay, config);
            if (currentAddressableContext == null || !currentAddressableContext.IsValid())
            {
                // 事前処理が失敗した場合はタスク数をデクリメントして終了
                Interlocked.Decrement(ref numCurrentRunningTasks);
                return;
            }
            
            // GMLを1つインポートした事後処理として、動的タイル化を指定
            var postGmlImport = new List<IPostGmlImportProcessor>
            {
                new DynamicTilePostGmlImportProcessor(currentAddressableContext, progressDisplay)
            };
            
            // インポートを実行
            var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token, postGmlImport);

            // 事後処理を設定
            DynamicTileImportProcessor.HandleCompletionAsync(
                task, 
                currentAddressableContext, 
                progressDisplay,
                () => 
                {
                    currentAddressableContext = null; // 完了後にクリア
                    Interlocked.Decrement(ref numCurrentRunningTasks);
                });
            
            task.ContinueWithErrorCatch();
        }
    }
}
