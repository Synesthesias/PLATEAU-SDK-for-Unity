using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor
{
    /// <summary>
    /// PlateauのUnityエディター用のユーティリティクラスです。
    /// スタイルがあります。
    /// </summary>
    public static class EditorUtil {
        
        /// <summary> 見出し1のスタイルで文字を表示します。 </summary>
        public static void Heading1(string text) {
            GUILayout.Box(text, styleHeading1);
        }

        /// <summary> 見出し1のスタイルです。 </summary>
        private static readonly GUIStyle styleHeading1 = new GUIStyle("ShurikenModuleTitle") {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            fixedHeight = 28,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(3, 2, 4, 4)
        };

    }
}
