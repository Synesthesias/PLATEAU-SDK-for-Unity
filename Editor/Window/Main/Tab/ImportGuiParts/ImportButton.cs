using PLATEAU.CityAdjust.ChangeActive;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PLATEAU.Editor.Window.Main.Tab.ImportGuiParts
{
    /// <summary>
    /// 「モデルをインポート」ボタンの描画と実行を行います。
    /// </summary>
    internal class ImportButton
    {
        /// <summary>
        /// 「モデルをインポート」のキャンセル用Tokenソース
        /// インポートタスクは1本なので発行するトークンも１つ
        /// </summary>
        private CancellationTokenSource cancellationTokenSrc;

        private int numCurrentRunningTasks;
        private ImportToDynamicTile importToDynamicTile;
        

        /// <summary>
        /// 「モデルをインポート」ボタンの描画と実行を行います。
        /// </summary>
        public void Draw(CityImportConfig config, IProgressDisplay progressDisplay)
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

                        switch (config.DynamicTileImportConfig.ImportType)
                        {
                            case ImportType.Scene:
                                // シーン上へのインポートを実行します。
                                ExecuteNormalImport(config, progressDisplay);
                                break;
                            case ImportType.DynamicTile:
                                // 動的タイル形式でのインポートを実行します。
                                importToDynamicTile = new ImportToDynamicTile(progressDisplay);
                                var task = importToDynamicTile.ExecAsync(config, cancellationTokenSrc.Token);
                                task.ContinueWith((_) => Interlocked.Decrement(ref numCurrentRunningTasks));
                                task.ContinueWithErrorCatch();
                                break;
                            default:
                                throw new Exception("invalid import type.");
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

                            if (config.DynamicTileImportConfig.ImportType != ImportType.DynamicTile)
                            {
                                return;
                            }
                            // 動的タイルインポートのクリーンアップ
                            importToDynamicTile?.CancelImport();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通常のインポート処理を実行します
        /// </summary>
        private void ExecuteNormalImport(CityImportConfig config, IProgressDisplay progressDisplay)
        {

            var postGmlProcessors = new List<IPostGmlImportProcessor>
            {
                new CityDuplicateProcessor() // 重複した低LODを非表示にします。
            };
            var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token, postGmlProcessors);
            PLATEAU.Util.Dialogue.Display("インポートが完了しました", "OK");
            window.Repaint();
            
            task.ContinueWith((_) => { Interlocked.Decrement(ref numCurrentRunningTasks); });
            task.ContinueWithErrorCatch();
        }
        
    }
}
