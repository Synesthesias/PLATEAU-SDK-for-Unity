
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.Util
{
    /// <summary>
    /// Editorの場合にUnityのダイアログを表示します。
    /// Runtimeの場合に代わりにログを出力します。
    /// </summary>
    public class Dialogue
    {
        /// <summary>
        /// Editorの場合、ダイアログで2択の選択肢を提示し、結果をboolで返します。
        /// </summary>
        public static bool Display(string message, string ok, string cancel)
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialog("PLATEAU SDK", message, ok, cancel);
#else
            Debug.Log("Dialogue called in runtime. Assuming OK. message = " + message);
            return true;
#endif
        }

        /// <summary> Editorの場合、キャンセルボタンのないダイアログを表示します。 </summary>
        public static void Display(string message, string ok)
        {
            #if UNITY_EDITOR
            EditorUtility.DisplayDialog("PLATEAU SDK", message, ok);
            #else
            Debug.Log("Dialogue called in runtime. message = " + message);
            #endif
        }
    }
}