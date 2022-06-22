using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindowCommon
{
    /// <summary>
    /// PlateauのUnityエディターのスタイルです。
    /// </summary>
    internal static class PlateauEditorStyle
    {
        private static readonly Color mainButtonColorTint = new Color(140f / 255f, 235f / 255f, 255f / 255f);

        /// <summary> 見出し1のスタイルで文字を表示します。 </summary>
        public static void Heading1(string text)
        {
            GUILayout.Box(text, styleHeading1);
        }

        /// <summary> 見出し2のスタイルで文字を表示します。 </summary>
        public static void Heading2(string text)
        {
            GUILayout.Box(text, styleHeading2);
        }
        
        /// <summary> 見出し3のスタイルで文字を表示します。 </summary>
        public static void Heading3(string text)
        {
            GUILayout.Box(text, styleHeading3);
        }

        /// <summary> 見出し1のスタイルです。 </summary>
        private static readonly GUIStyle styleHeading1 = new GUIStyle("ShurikenModuleTitle")
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            fixedHeight = 30,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(3, 2, 4, 4)
        };
        
        /// <summary> 見出し2のスタイルです。 </summary>
        private static readonly GUIStyle styleHeading2 = new GUIStyle("ShurikenModuleTitle")
        {
            fontSize = 13,
            fixedHeight = 25,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(15, 12, 4, 4)
        };
        
        /// <summary> 見出し3のスタイルです。 </summary>
        private static readonly GUIStyle styleHeading3 = new GUIStyle("ShurikenModuleTitle")
        {
            fontSize = 12,
            fixedHeight = 22,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(27, 12, 4, 4)
        };

        /// <summary> ボタンのスタイルです。押されたときにtrueを返します。 </summary>
        public static bool MainButton(string text)
        {
            var isButtonPushed = DrawButton(text, mainButtonColorTint);
            return isButtonPushed;
        }

        /// <summary>
        /// IDisposable な VerticalScope を作り、中のGUIコンテンツを BoxStyle で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScope()
        {
            return new EditorGUILayout.VerticalScope(BoxStyle);
        }

        /// <summary>
        /// GUIのコンテンツをまとめるのに利用できるboxです。
        /// </summary>
        private static GUIStyle BoxStyle
        {
            get
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset(8, 8, 8, 8)
                };
                
                return style;
            }
        }

        /// <summary>
        /// タブ形式で複数のボタンから選ぶGUIを表示し、選択されたタブのインデックスを返します。
        /// 引数には現在のタブのインデックスと、paramsで各タブの表示名を与えます。
        /// </summary>
        public static int Tabs(int currentTabIndex, params string[] tabNames)
        {
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = mainButtonColorTint;
            int newTabIndex;
            using (VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    newTabIndex = GUILayout.Toolbar(
                        currentTabIndex,
                        tabNames,
                        "LargeButton",
                        GUI.ToolbarButtonSize.Fixed
                    );
                }

                EditorGUILayout.LabelField($"Selected : {tabNames[newTabIndex]}");
            }

            GUI.backgroundColor = prevColor;
            return newTabIndex;
        }

        /// <summary> 色指定でボタンを描画します。 </summary>
        private static bool DrawButton(string text, Color buttonColorTint)
        {
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonColorTint;
            var style = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(20, 20, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            var isButtonPushed = GUILayout.Button(text, style);
            GUI.backgroundColor = prevColor;
            return isButtonPushed;
        }
    }
}