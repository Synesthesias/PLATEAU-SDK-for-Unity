using System.Collections.Generic;
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
        private const string cyanBackgroundDark = "#292e30";
        private const string cyanBackgroundLight = "#abc4c9";
        private static readonly Dictionary<string, Texture2D> cachedTexture = new Dictionary<string, Texture2D>();

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
            var isButtonPushed = DrawButton(text, mainButtonColorTint, new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(20, 20, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            });
            return isButtonPushed;
        }
        
        public static bool MiniButton(string text)
        {
            bool isButtonPushed = DrawButton(text, mainButtonColorTint, new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(10, 10, 5, 5),
                padding = new RectOffset(5, 5, 3, 3)
            });
            return isButtonPushed;
        }
        
        /// <summary> 色指定でボタンを描画します。 </summary>
        private static bool DrawButton(string text, Color buttonColorTint, GUIStyle style)
        {
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonColorTint;
            var isButtonPushed = GUILayout.Button(text, style);
            GUI.backgroundColor = prevColor;
            return isButtonPushed;
        }

        /// <summary>
        /// IDisposable な VerticalScope を作り、中のGUIコンテンツを BoxStyle で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel1()
        {
            return new EditorGUILayout.VerticalScope(BoxStyleLevel1);
        }

        public static EditorGUILayout.VerticalScope VerticalScopeLevel2()
        {
            return new EditorGUILayout.VerticalScope(BoxStyleLevel2);
        }

        /// <summary>
        /// GUIのコンテンツをまとめるのに利用できるboxです。
        /// </summary>
        private static GUIStyle BoxStyleLevel1
        {
            get
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset(32, 8, 8, 8)
                };
                
                return style;
            }
        }
        
        /// <summary>
        /// box入れ子の2段目のスタイルです。
        /// 青っぽい色に寄せた背景色のBoxStyleを返します。
        /// エディタのテーマが Dark か Light かに応じて異なる色を返します。
        /// </summary>
        private static GUIStyle BoxStyleLevel2 {
            get {
                GUIStyle style = new GUIStyle(BoxStyleLevel1);
                string colorCode = EditorGUIUtility.isProSkin ? cyanBackgroundDark : cyanBackgroundLight;
                style.normal.background = ColoredBackground(colorCode);
                style.padding.top = style.padding.bottom = 10;
                style.margin.left = 16;
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
            using (VerticalScopeLevel1())
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
        
        
        /// <summary>
        /// 背景用に単色のテクスチャを作ります。
        /// </summary>
        private static Texture2D ColoredBackground(string colorCode) {
            // 作ったテクスチャはなるべく使い回します。
            // 毎フレーム Texture を new していると エラー「Resource ID out of range」が出るためです。 
            if (cachedTexture.ContainsKey(colorCode)) {
                return cachedTexture[colorCode];
            }
            Texture2D tex = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString(colorCode, out Color col);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            cachedTexture.Add(colorCode, tex);
            return tex;
        }
    }
}