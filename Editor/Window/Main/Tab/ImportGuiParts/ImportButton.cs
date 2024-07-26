using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Threading;

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

                        // ここでインポートします。
                        var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token);

                        task.ContinueWith((_) => { Interlocked.Decrement(ref numCurrentRunningTasks); });
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
    }
}
