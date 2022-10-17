using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string imageDirPath = "Packages/com.synesthesias.plateau-unity-sdk/Images";
        private static readonly Color mainButtonColorTint = new Color(140f / 255f, 235f / 255f, 255f / 255f);
        private const string cyanBackgroundDark = "#292e30";
        private const string cyanBackgroundLight = "#abc4c9";
        private const string colorDarkBoxBackground = "#191919";
        private const string colorDarkBoxSelectedElement = "#676767";
        private const string colorDarkBoxClickedElement = "#303030";
        private const string colorLogoBackground = "#676767";
        private const string colorLogoLine = "#D2D2D2";
        private const string imageNameLogo = "logo-for-unity.png";
        private const string imageNameGradationDarkLong = "dark_gradation_long.png";
        private const string imageNameGradationDarkShortInverted = "dark_gradation_short_inverted.png";
        private const string imageNameGradationDarkShort = "dark_gradation_short.png";
        private static readonly Dictionary<string, Texture2D> cachedTexture = new Dictionary<string, Texture2D>();

        public static void Heading(string text)
        {

            using (new EditorGUILayout.HorizontalScope())
            {
                var textStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 14
                };
                var textContent = new GUIContent(text);
                var textWidth = textStyle.CalcSize(textContent).x;
                EditorGUILayout.LabelField(textContent, textStyle, GUILayout.MaxWidth(textWidth));
                var image = LoadTexture(ImagePath(imageNameGradationDarkLong));
                EditorGUILayout.LabelField(new GUIContent(image));
            }
        }

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
            var boxStyle = contentStyleLevel2;
            boxStyle.padding.bottom = 4;
            boxStyle.margin.bottom = 4;
            boxStyle.fixedHeight = maxHeight + labelStyle.padding.top + labelStyle.padding.bottom +
                                   labelStyle.margin.top + labelStyle.margin.bottom + boxStyle.padding.top +
                                   boxStyle.padding.bottom;

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
            return new EditorGUILayout.VerticalScope(contentStyleLevel2);
        }

        /// <summary>
        /// 中のGUIコンテンツを青の Box で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel3()
        {
            return new EditorGUILayout.VerticalScope(contentStyleLevel3);
        }

        public static EditorGUILayout.VerticalScope VerticalScopeDarkBox()
        {
            return new EditorGUILayout.VerticalScope(styleDarkBox);
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

        private static readonly GUIStyle contentStyleLevel2 = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(16, 8, 8, 8)
        };

        /// <summary>
        /// ContentStyle入れ子の3段目のスタイルです。
        /// 青っぽい色に寄せた背景色のBoxStyleを返します。
        /// エディタのテーマが Dark か Light かに応じて異なる色を返します。
        /// </summary>
        private static readonly GUIStyle contentStyleLevel3 = new GUIStyle(ContentStyleLevel1(false))
        {
            normal =
            {
                background = ColoredTexture(EditorGUIUtility.isProSkin ? cyanBackgroundDark : cyanBackgroundLight)
            },
            margin =
            {
                left = 16
            },
            padding =
            {
                top = 10,
                bottom = 10
            }
        };

        private static readonly GUIStyle styleDarkBox = new GUIStyle
        {
            normal =
            {
                background = ColoredTexture(colorDarkBoxBackground)
            },
            margin = new RectOffset(15, 15, 15, 15),
            padding = new RectOffset(5, 5, 15, 15)
        };


        private static readonly GUIStyle styleLogoBackground = new GUIStyle
        {
            normal =
            {
                background = ColoredTexture(colorLogoBackground)
            },
            margin = new RectOffset(0, 0, 15, 15)
        };

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

        /// <summary>
        /// タブ形式の選択GUIで、タブの中身が画像です。
        /// </summary>
        /// <param name="currentTabIndex">現在選択されているタブの番号です。</param>
        /// <param name="imagePathsRelative"><see cref="imageDirPath"/> からの相対パスで画像を指定します。</param>
        /// <param name="buttonWidth">ボタン1つあたりの横幅(px)です。</param>
        /// <returns>選択されたタブの番号です。</returns>
        public static int TabWithImages(int currentTabIndex, string[] imagePathsRelative, float buttonWidth)
        {
            int nextTabIndex = currentTabIndex;
            int tabCount = imagePathsRelative.Length;
            if (tabCount <= 0) return nextTabIndex;
            
            var images = imagePathsRelative
                .Select(ImagePath)
                .Select(path => (Texture)LoadTexture(path))
                .ToArray();
            float toolbarButtonHeight = images[0].height * buttonWidth / images[0].width;

            var contents = new GUIContent[tabCount];
            for (int i = 0; i < tabCount; i++)
            {
                contents[i] = new GUIContent(images[i]);
            }

            var baseStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                imagePosition = ImagePosition.ImageAbove,
                fixedHeight = toolbarButtonHeight,
                normal =
                {
                    background = ColoredTexture(colorDarkBoxBackground),
                }
            };
            // タブ形式の選択ボタンを描画します。
            // GUILayout.Toolbar() で実装できると思いきや、サイズ調整が上手くいかなかったので
            // 代わりにタブの数だけ Button を描画することにします。
            using (new EditorGUILayout.HorizontalScope(styleDarkBox))
            {
                EditorGUILayout.Space();
                // ボタンごとのループです。
                for (int i = 0; i < tabCount; i++)
                {
                    var buttonStyle = new GUIStyle(baseStyle);
                    if (i == currentTabIndex)
                    {
                        buttonStyle.normal.background = ColoredTexture(colorDarkBoxSelectedElement);
                    }
                    else
                    {
                        buttonStyle.active.background = ColoredTexture(colorDarkBoxClickedElement);
                    }

                    if (GUILayout.Button(contents[i], buttonStyle, GUILayout.MaxWidth(buttonWidth)))
                    {
                        nextTabIndex = i;
                    }
                }

                EditorGUILayout.Space();
            }

            return nextTabIndex;
        }

        public static void SubTitle(string text)
        {
            var lineImageL =
                LoadTexture(ImagePath(imageNameGradationDarkShortInverted));
            var lineImageR =
                LoadTexture(ImagePath(imageNameGradationDarkShort));
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent(lineImageL), GUILayout.MaxWidth(lineImageL.width));
                var textContent = new GUIContent(text);
                var textStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                float textWidth = textStyle.CalcSize(textContent).x;
                EditorGUILayout.LabelField(text, textStyle, GUILayout.MaxWidth(textWidth));
                EditorGUILayout.LabelField(new GUIContent(lineImageR), GUILayout.MaxWidth(lineImageR.width));
                EditorGUILayout.Space();
            }
        }
        

        /// <summary>
        /// 背景用に単色のテクスチャを作ります。
        /// </summary>
        /// <param name="colorCode">黒は "#000000", 白は "#ffffff" で表されるカラーコードです。</param>
        private static Texture2D ColoredTexture(string colorCode)
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
            var tex = LoadTexture(ImagePath(imageNameLogo));
            if (tex is null) return;
            float width = Math.Min(tex.width, Screen.width);
            float height = tex.height * width / tex.width;
            using (new EditorGUILayout.VerticalScope(styleLogoBackground))
            {
                LogoLine();
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space(0);
                    EditorGUILayout.LabelField(new GUIContent(tex), GUILayout.Width(width), GUILayout.Height(height));
                    EditorGUILayout.Space(0);
                }
                LogoLine();
            }

            void LogoLine()
            {
                var style = new GUIStyle()
                {
                    normal =
                    {
                        background = ColoredTexture(colorLogoLine)
                    },
                    margin = new RectOffset(0,0,0,0),
                    fixedHeight = 1
                };
                GUILayout.Box("", style);
            }
        }

        /// <summary>
        /// テクスチャをロードし、キャッシュに追加してから返します。
        /// すでにキャッシュにあれば、ファイルロードの代わりにキャッシュから返します。
        /// 引数文字列がキャッシュのキーとなります。
        /// </summary>
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

        private static string ImagePath(string imageName)
        {
            return Path.Combine(imageDirPath, imageName);
        }
    }
}