using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.Util
{
    /// <summary>
    /// Editorの場合、ダイアログで進捗を表示します。
    /// Runtimeの場合、何もしません。
    /// Dispose時にダイアログを閉じるので、閉じるために利用時に using var progressDisplay = new ProgressDisplayDialogue(); のように
    /// usingを付けてください。付け忘れるとダイアログが閉じません。
    /// </summary>
    public class ProgressDisplayDialogue : IProgressDisplay, IDisposable
    {
        public void SetProgress(string progressName, float percentage, string message)
        {
            #if UNITY_EDITOR
            string progressNameFormatted = string.IsNullOrEmpty(progressName) ? "" : progressName +":\n";
            EditorUtility.DisplayProgressBar("PLATEAU", progressNameFormatted + message, percentage/ 100f);
            #endif
        }

        public void Dispose()
        {
            #if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
            #endif
        }
    }
}