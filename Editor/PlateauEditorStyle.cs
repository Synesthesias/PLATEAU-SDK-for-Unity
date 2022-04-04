using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor
{
    /// <summary>
    /// PlateauのUnityエディターのスタイルです。
    /// </summary>
    public static class PlateauEditorStyle {
        private static readonly Color mainButtonColorTint = new Color(140f / 255f, 235f / 255f, 255f / 255f);
        
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

        /// <summary> ボタンのスタイルです。押されたときにtrueを返します。 </summary>
        public static bool MainButton(string text) {
            bool isButtonPushed = DrawButton(text, mainButtonColorTint);
            return isButtonPushed;
        }

        /// <summary>
        /// IDisposable な VerticalScope を作り、中のGUIコンテンツを BoxStyle で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScope() {
            return new EditorGUILayout.VerticalScope(BoxStyle);
        }

        /// <summary>
        /// GUIのコンテンツをまとめるのに利用できるboxです。
        /// </summary>
        public static GUIStyle BoxStyle {
            get {
                GUIStyle style = new GUIStyle(GUI.skin.box) {
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset(8, 8, 8, 8)
                };
                return style;
            }
        }
        

        /// <summary> 色指定でボタンを描画します。 </summary>
        private static bool DrawButton(string text, Color buttonColorTint) {
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonColorTint;
            var style = new GUIStyle(GUI.skin.button) {
                margin = new RectOffset(20, 20, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            bool isButtonPushed = GUILayout.Button(text, style);
            GUI.backgroundColor = prevColor;
            return isButtonPushed;
        }
        
        private static Texture2D GenerateColoredTexture(Color color) {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

    }
}
