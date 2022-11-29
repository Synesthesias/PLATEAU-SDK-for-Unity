using System.Threading;
using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// 「モデルをインポート」ボタンの描画と実行を行います。
    /// </summary>
    internal static class ImportButton
    {
        private static int numCurrentRunningTasks;
        
        /// <summary>
        /// 「モデルをインポート」ボタンの描画と実行を行います。
        /// </summary>
        public static void Draw(CityLoadConfig config, IProgressDisplay progressDisplay)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                // ボタンを描画します。
                if (PlateauEditorStyle.MainButton("モデルをインポート"))
                {
                    // すでにインポートタスクが動いている場合、追加で処理に加えるか尋ねるダイアログを出します。
                    if (numCurrentRunningTasks > 0)
                    {
                        bool dialogueResult = EditorUtility.DisplayDialog("PLATEAU SDK", $"すでに {numCurrentRunningTasks}つのインポート処理を実行中です。\n追加で処理に加えますか？", "はい", "いいえ");
                        if (!dialogueResult)
                        {
                            // 「いいえ」ならキャンセルします。
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }
                    // ボタンを実行します。
                    Interlocked.Increment(ref numCurrentRunningTasks);
                    // ここでインポートします。
                    var task = CityImporter.ImportAsync(config, progressDisplay);
                    task.ContinueWith((t) => { Interlocked.Decrement(ref numCurrentRunningTasks); });
                    task.ContinueWithErrorCatch();
                }
            }
        }        
    }
}
