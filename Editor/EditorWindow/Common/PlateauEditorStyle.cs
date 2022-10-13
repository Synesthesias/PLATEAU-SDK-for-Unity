using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.Client.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.Common
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

        public static void Separator(int indentLevel)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(indentLevel * 15);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }
        }

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

        /// <summary> 複数行のラベルを表示します。 </summary>
        public static void MultiLineLabel(string text)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };
            EditorGUILayout.LabelField(text, style);
        }

        /// <summary> 複数行のラベルを表示して Box で囲みます。 </summary>
        public static void MultiLineLabelWithBox(string text)
        {
            using (VerticalScopeLevel2())
            {
                MultiLineLabel(text);
            }
        }

        public static Vector2 ScrollableMultiLineLabel(string text, float maxHeight, Vector2 scrollPos)
        {
            int lineCount = text.Count(c => c.Equals('\n')) + 1;
            maxHeight = Math.Min(maxHeight, lineCount * 15);

            // ScrollView の内側のスタイル
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            // ScrollView の外側のスタイル
            var boxStyle = ContentStyleLevel2;
            boxStyle.padding.bottom = 4;
            boxStyle.margin.bottom = 4;
            boxStyle.fixedHeight = maxHeight + labelStyle.padding.top + labelStyle.padding.bottom + labelStyle.margin.top + labelStyle.margin.bottom + boxStyle.padding.top + boxStyle.padding.bottom;

            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
                {
                    scrollPos = scrollView.scrollPosition;

                    EditorGUILayout.LabelField(text, labelStyle);
                }
            }

            return scrollPos;
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
        /// IDisposable な VerticalScope を作り、中のGUIコンテンツを Box で囲みます。
        /// Box の位置には <see cref="HeaderDrawer"/> の見出しの深さがインデントとして反映されます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel1(bool useHeaderDepth = true)
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLevel1(useHeaderDepth));
        }

        /// <summary>
        /// 中のGUIコンテンツをグレーの Box で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel2()
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLevel2);
        }

        /// <summary>
        /// 中のGUIコンテンツを青の Box で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel3()
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLevel3);
        }

        /// <summary>
        /// GUIのコンテンツをまとめるのに利用できます。
        /// </summary>
        private static GUIStyle ContentStyleLevel1(bool useHeaderDepth = true)
        {
            int marginLeft = useHeaderDepth ? HeaderDrawer.Depth * 12 : 12;
            var style = new GUIStyle
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(marginLeft, 8, 8, 8)
            };

            return style;
        }

        public static GUIStyle ContentStyleLevel2
        {
            get
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset(16, 8, 8, 8)
                };

                return style;
            }
        }

        /// <summary>
        /// ContentStyle入れ子の3段目のスタイルです。
        /// 青っぽい色に寄せた背景色のBoxStyleを返します。
        /// エディタのテーマが Dark か Light かに応じて異なる色を返します。
        /// </summary>
        private static GUIStyle ContentStyleLevel3
        {
            get
            {
                GUIStyle style = new GUIStyle(ContentStyleLevel1(false));
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
            int nextTabIndex;
            using (VerticalScopeLevel1())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    nextTabIndex = GUILayout.Toolbar(
                        currentTabIndex,
                        tabNames,
                        "LargeButton",
                        GUI.ToolbarButtonSize.Fixed
                    );
                }
            }

            GUI.backgroundColor = prevColor;
            return nextTabIndex;
        }

        public static int TabWithImages(int currentTabIndex, string[] imagePathsRelative)
        {
            const float buttonWidth = 110;
            int nextTabIndex = currentTabIndex;
            int tabCount = imagePathsRelative.Length;
            if (tabCount <= 0) return nextTabIndex;
            
            var images = imagePathsRelative
                .Select(relative => Path.Combine(PathUtil.EditorWindowImagePath, relative))
                .Select(path => (Texture)LoadTexture(path))
                .ToArray();
            // float toolbarButtonWidth = images[0].width;
            float toolbarButtonHeight = images[0].height * buttonWidth / images[0].width;

            var contents = new GUIContent[tabCount];
            for (int i = 0; i < tabCount; i++)
            {
                contents[i] = new GUIContent(images[i]);
            }

            var style = new GUIStyle(EditorStyles.toolbarButton);
            style.imagePosition = ImagePosition.ImageAbove;
            
            
            //
            // var style = new GUIStyle(GUI.skin.button);
            // style.padding.bottom = 0;
            // style.padding.top = 0;
            // style.padding.left = 0;
            // style.padding.right = 0;
            style.fixedWidth = buttonWidth;
            style.fixedHeight = toolbarButtonHeight;
            // style.imagePosition.
            //
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                for (int i = 0; i < tabCount; i++)
                {
                    if (GUILayout.Button(contents[i], style))
                    {
                        nextTabIndex = i;
                    }
                }
                EditorGUILayout.Space();
            }
            // using (VerticalScopeLevel1())
            // {
            //     using (new EditorGUILayout.HorizontalScope())
            //     {
            //         var style = new GUIStyle(EditorStyles.largeLabel);
            //         style.fixedHeight = toolbarButtonHeight;
            //         style.fixedWidth = toolbarButtonWidth;
            //         // style.stretchHeight = true;
            //         // style.stretchWidth = true;
            //         nextTabIndex = GUILayout.Toolbar(
            //             currentTabIndex,
            //             images,
            //             style,
            //             GUI.ToolbarButtonSize.Fixed,
            //             GUILayout.Width(toolbarButtonWidth * tabCount),
            //             GUILayout.Height(toolbarButtonHeight)
            //         );
            //     }
            // }
            // int nextTabIndex = currentTabIndex;
            // float widthPerTab = 65f;
            // float imageHeight = 60f;
            // float textHeight = 16f;
            // int count = Math.Min(tabNames.Length, imagePathsRelative.Length);
            // using (new EditorGUILayout.HorizontalScope())
            // {
            //     EditorGUILayout.Space();
            //     using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(widthPerTab * count)))
            //     {
            //         EditorGUILayout.Space();
            //         for (int i = 0; i < count; i++)
            //         {
            //             using (new EditorGUILayout.VerticalScope())
            //             {
            //                 // // 中央揃えで画像を描きます。
            //                 // using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidthPerTab)))
            //                 // {
            //                 //     EditorGUILayout.Space();
            //                 //     string imagePath = Path.Combine(PathUtil.EditorWindowImagePath, imagePathsRelative[i]);
            //                 //     var image = LoadTexture(imagePath);
            //                 //     var imageMaxWidth = Math.Min(maxWidthPerTab, image.width);
            //                 //     EditorGUILayout.LabelField(new GUIContent(image), GUILayout.MaxWidth(imageMaxWidth), GUILayout.MaxHeight(imageHeight));
            //                 //     EditorGUILayout.Space();
            //                 // }
            //                 //
            //                 // // 画像の下に中央揃えで文字を置きます。
            //                 // using (new EditorGUILayout.HorizontalScope())
            //                 // {
            //                 //     EditorGUILayout.Space();
            //                 //     var style = new GUIStyle(EditorStyles.label);
            //                 //     style.alignment = TextAnchor.MiddleCenter;
            //                 //     EditorGUILayout.LabelField(tabNames[i], style, GUILayout.MaxWidth(maxWidthPerTab), GUILayout.MaxHeight(textHeight));
            //                 //     EditorGUILayout.Space();
            //                 // }
            //                 
            //                 string imagePath = Path.Combine(PathUtil.EditorWindowImagePath, imagePathsRelative[i]);
            //                 var image = LoadTexture(imagePath);
            //                 var style = new GUIStyle(EditorStyles.toolbarButton);
            //                 style.margin.bottom = 0;
            //                 style.margin.top = 0;
            //                 var layout = new GUILayoutOption[] { GUILayout.MaxWidth(widthPerTab) };
            //                 GUILayout.Button(image, style, layout);
            //                 GUILayout.Button(tabNames[i], style, layout);
            //             }
            //         }
            //
            //         EditorGUILayout.Space();
            //         // var images = imagePathsRelative
            //         //     .Select(relative => Path.Combine(PathUtil.EditorWindowImagePath, relative))
            //         //     .Select(path => (Texture)LoadTexture(path))
            //         //     .ToArray();
            //         // var tabs = new GUIContent[count];
            //         // for (int i = 0; i < count; i++)
            //         // {
            //         //     tabs[i] = new GUIContent(tabNames[i], images[i]);
            //         // }
            //         // nextTabIndex = GUILayout.Toolbar(
            //         //     currentTabIndex,
            //         //     tabs,
            //         //     GUILayout.Height(100)
            //         // );
            //     }
            //
            //     EditorGUILayout.Space();
            // }
            //
            return nextTabIndex;
        }


        /// <summary>
        /// 背景用に単色のテクスチャを作ります。
        /// </summary>
        private static Texture2D ColoredBackground(string colorCode)
        {
            // 作ったテクスチャはなるべく使い回します。
            // 毎フレーム Texture を new していると エラー「Resource ID out of range」が出るためです。 
            if (cachedTexture.ContainsKey(colorCode))
            {
                return cachedTexture[colorCode];
            }
            Texture2D tex = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString(colorCode, out Color col);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            cachedTexture.Add(colorCode, tex);
            return tex;
        }

        public static void MainLogo()
        {
            var tex = LoadTexture(Path.Combine(PathUtil.EditorWindowImagePath, "logo 1.png"));
            if (tex is null) return;
            float width = Math.Min(tex.width, Screen.width);
            float height = tex.height * width / tex.width;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(0);
                EditorGUILayout.LabelField(new GUIContent(tex), GUILayout.Width(width), GUILayout.Height(height));
                EditorGUILayout.Space(0);
            }
            
        }

        private static Texture2D LoadTexture(string path)
        {
            if (cachedTexture.TryGetValue(path, out var cacheHitTexture))
            {
                return cacheHitTexture;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex is null)
            {
                Debug.LogError($"Texture is not found : path = {path}");
            }
            cachedTexture.Add(path, tex);
            return tex;
        }
    }
}