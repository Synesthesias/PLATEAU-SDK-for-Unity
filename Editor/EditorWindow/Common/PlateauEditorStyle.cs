using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // private static readonly Color mainButtonColorTint = new Color(140f / 255f, 235f / 255f, 255f / 255f);
        private const string cyanBackgroundDark = "#292e30";
        private const string cyanBackgroundLight = "#abc4c9";
        private static readonly ColorLightDark colorDarkBoxBackground = new ColorLightDark("#515151", "#191919");
        private static readonly ColorLightDark colorButtonMain = new ColorLightDark("#005858", "#005858");
        private static readonly ColorLightDark colorButtonSub = new ColorLightDark("#E4E4E4", "#676767");
        private static readonly ColorLightDark colorDefaultFont = new ColorLightDark("#090909", "#C4C4C4");
        private const string colorDarkBoxSelectedElement = "#676767";
        private const string colorDarkBoxClickedElement = "#303030";
        private const string colorLogoBackground = "#676767";
        private const string colorLogoLine = "#D2D2D2";
        private const string imageNameLogo = "logo_for_unity.png";

        private static readonly ImagePathLightDark imageGradationLong =
            new ImagePathLightDark("light_gradation_long.png", "dark_gradation_long.png");

        private static readonly ImagePathLightDark imageGradationShortInverted =
            new ImagePathLightDark("light_gradation_short_inverted.png", "dark_gradation_short_inverted.png");

        private static readonly ImagePathLightDark imageGradationShort =
            new ImagePathLightDark("light_gradation_short.png", "dark_gradation_short.png");

        private static readonly ImagePathLightDark imageIconBuilding =
            new ImagePathLightDark("light_icon_building.png", "dark_icon_building.png");

        private const string imageRoundButton = "round-button.png";
        private const string imageRoundWindowWide = "round-window-wide.png";
        private static readonly Dictionary<string, Texture2D> cachedTexture = new Dictionary<string, Texture2D>();

        /// <summary>
        /// 見出しを表示します。
        /// 行頭にアイコンを表示します。アイコンのパスは引数で <see cref="imageDirPath"/> からの相対パスで指定します。
        /// パスが null の場合はアイコンを表示しません。
        /// </summary>
        public static void Heading(string text, string imageIconRelativePath)
        {
            const float height = 40;
            var boxStyle = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };
            using (new EditorGUILayout.HorizontalScope(boxStyle, GUILayout.Height(height)))
            {
                // 行頭のアイコン
                if (imageIconRelativePath != null)
                {
                    var imageIcon = LoadTexture(imageIconRelativePath);
                    var iconWidth = imageIcon.width * height / imageIcon.height;
                    var iconStyle = new GUIStyle(EditorStyles.label)
                    {
                        fixedHeight = height,
                        fixedWidth = iconWidth,
                    };
                    var iconContent = new GUIContent(imageIcon);
                    LabelSizeFit(iconContent, iconStyle);
                }
                // テキスト
                var textStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold
                };
                var textContent = new GUIContent(text);
                var textWidth = textStyle.CalcSize(textContent).x;
                CenterAlignVertical(() =>
                    {
                        EditorGUILayout.LabelField(textContent, textStyle, GUILayout.MaxWidth(textWidth));
                    }
                    , GUILayout.Height(height), GUILayout.Width(textWidth)
                );
                // 行末の線の画像
                CenterAlignVertical(() =>
                    {
                        var imageLine = LoadTexture(imageGradationLong.RelativePath);
                        EditorGUILayout.LabelField(new GUIContent(imageLine));
                    }
                    , GUILayout.Height(height)
                );

            }
        }

        /// <summary>
        /// ラベルの幅 = ラベルの中身の幅　となるラベルを描画します。
        /// </summary>
        public static void LabelSizeFit(GUIContent content, GUIStyle style)
        {
            var width = style.CalcSize(content).x;
            EditorGUILayout.LabelField(content, style, GUILayout.Width(width));
        }

        /// <summary> 見出し1のスタイルで文字を表示します。 </summary>
        public static void Heading1(string text)
        {
            GUILayout.Box(text, StyleHeading1);

        }

        /// <summary> 見出し2のスタイルで文字を表示します。 </summary>
        public static void Heading2(string text)
        {
            GUILayout.Box(text, StyleHeading2);
        }

        /// <summary> 見出し3のスタイルで文字を表示します。 </summary>
        public static void Heading3(string text)
        {
            GUILayout.Box(text, StyleHeading3);
        }

        /// <summary> 見出し1のスタイルです。 </summary>
        private static GUIStyle StyleHeading1 => new GUIStyle("ShurikenModuleTitle")
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            fixedHeight = 30,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(3, 2, 4, 4)
        };

        /// <summary> 見出し2のスタイルです。 </summary>
        private static GUIStyle StyleHeading2 => new GUIStyle("ShurikenModuleTitle")
        {
            fontSize = 13,
            fixedHeight = 25,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(15, 12, 4, 4)
        };

        /// <summary> 見出し3のスタイルです。 </summary>
        private static GUIStyle StyleHeading3 => new GUIStyle("ShurikenModuleTitle")
        {
            fontSize = 12,
            fixedHeight = 22,
            contentOffset = new Vector2(4f, -4f),
            margin = new RectOffset(27, 12, 4, 4)
        };

        public static string IconPathBuilding => imageIconBuilding.RelativePath;

        public static void Separator(int indentLevel)
        {
            const float lineWidth = 1;
            const int marginY = 15;
            var horizontalStyle = new GUIStyle
            {
                margin = new RectOffset(0, 0, marginY, marginY)
            };
            using (new GUILayout.HorizontalScope(horizontalStyle))
            {
                GUILayout.Space(indentLevel * 15);
                var boxStyle = new GUIStyle(EditorStyles.label)
                {
                    normal =
                    {
                        background = ColoredTexture(colorDarkBoxSelectedElement)
                    },
                    margin = new RectOffset(0, 0, 0, 0) 
                };
                GUILayout.Box("", boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(lineWidth));
            }
        }

        /// <summary> ボタンのスタイルです。押されたときにtrueを返します。 </summary>
        public static bool MainButton(string text)
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal =
                {
                    background = LoadTexture(imageRoundWindowWide),
                    textColor = colorDefaultFont.Dark
                },
                margin = new RectOffset(20, 20, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            var isButtonPushed = ButtonWithColorTint(new GUIContent(text), colorButtonMain.Color, buttonStyle, GUILayout.Height(60));
            return isButtonPushed;
        }

        public static bool MiniButton(string text)
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal =
                {
                    background = LoadTexture(imageRoundWindowWide),
                    textColor = colorDefaultFont.Dark
                },
                margin = new RectOffset(10, 10, 5, 5),
                padding = new RectOffset(5, 5, 3, 3)
            };
            bool isButtonPushed = ButtonWithColorTint(new GUIContent(text), colorButtonMain.Color, buttonStyle, GUILayout.Height(40));
            return isButtonPushed;
        }

        /// <summary> 複数行のラベルを表示します。 </summary>
        private static void MultiLineLabel(string text)
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

        public static EditorGUILayout.VerticalScope VerticalScopeDarkBox()
        {
            return new EditorGUILayout.VerticalScope(new GUIStyle(StyleDarkBox));
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

        private static GUIStyle ContentStyleLevel2 => new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(16, 8, 8, 8)
        };

        /// <summary>
        /// ContentStyle入れ子の3段目のスタイルです。
        /// 青っぽい色に寄せた背景色のBoxStyleを返します。
        /// エディタのテーマが Dark か Light かに応じて異なる色を返します。
        /// </summary>
        private static GUIStyle ContentStyleLevel3 => new GUIStyle(ContentStyleLevel1(false))
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

        private static GUIStyle StyleDarkBox => new GUIStyle
        {
            normal =
            {
                // background = ColoredTexture(colorDarkBoxBackground)
                background = LoadTexture(imageRoundWindowWide),
            },
            margin = new RectOffset(15, 15, 15, 15),
            padding = new RectOffset(5, 5, 15, 15)
        };


        private static GUIStyle StyleLogoBackground => new GUIStyle
        {
            normal =
            {
                background = ColoredTexture(colorLogoBackground)
            },
            margin = new RectOffset(0, 0, 15, 15)
        };
        

        /// <summary>
        /// タブ形式の選択GUIで、タブの中身が画像のものを表示します。
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
            
            var buttonIcons = imagePathsRelative
                .Select(path => (Texture)LoadTexture(path))
                .ToArray();
            var buttonBackground = LoadTexture(imageRoundButton);
            float toolbarButtonHeight = buttonBackground.height * buttonWidth / buttonBackground.width;

            var iconContents = new GUIContent[tabCount];
            for (int i = 0; i < tabCount; i++)
            {
                iconContents[i] = new GUIContent(buttonIcons[i]);
            }

            var baseStyle = new GUIStyle(EditorStyles.label)
            {
                imagePosition = ImagePosition.ImageAbove,
                fixedHeight = toolbarButtonHeight,
                // fixedWidth = buttonWidth,
                stretchWidth = true,
                stretchHeight = true,
                normal =
                {
                    background = ColoredTexture(ColorUtility.ToHtmlStringRGB(colorDarkBoxBackground.Color)),
                }
            };
            var prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = colorDarkBoxBackground.Color;
            
            // 枠を表示します。
            using (new EditorGUILayout.HorizontalScope(new GUIStyle(StyleDarkBox), GUILayout.MaxWidth(ScreenDrawableWidth), GUILayout.MinWidth(20 * tabCount)))
            {
                GUI.backgroundColor = prevBackgroundColor;
                EditorGUILayout.Space();
                // タブ形式の選択ボタンを描画します。
                // GUILayout.Toolbar() で実装できると思いきや、サイズ調整が上手くいかなかったので
                // 代わりにタブの数だけ Button を描画することにします。
                
                // ボタンごとのループです。
                for (int i = 0; i < tabCount; i++)
                {
                    var buttonStyle = new GUIStyle(baseStyle);
                    Color buttonBackgroundColorTint;
                    if (i == currentTabIndex)
                    {
                        
                        ColorUtility.TryParseHtmlString(colorDarkBoxSelectedElement, out var selectedColorTint);
                        buttonBackgroundColorTint = selectedColorTint;
                        buttonStyle.normal.background = buttonBackground;
                        
                    }
                    else
                    {
                        buttonBackgroundColorTint = colorDarkBoxBackground.Color;
                        buttonStyle.active.background = ColoredTexture(colorDarkBoxClickedElement);
                    }

                    if (ButtonWithColorTint(iconContents[i], buttonBackgroundColorTint, buttonStyle,
                            GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(toolbarButtonHeight),
                            GUILayout.MinWidth(20)))
                    {
                        nextTabIndex = i;
                    }
                }
            
                GUI.backgroundColor = prevBackgroundColor;
            
                EditorGUILayout.Space();
            }

            return nextTabIndex;
        }

        /// <summary>
        /// 選択式のタブを表示します。
        /// 選択されたタブの番号を返します。
        /// </summary>
        public static int Tabs(int currentTabIndex, params string[] tabNames)
        {
            const int height = 40;
            int tabCount = tabNames.Length;
            int nextTabIndex = currentTabIndex;
            var boxStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(30, 30, 0, 0)
            };
            using (new EditorGUILayout.HorizontalScope(boxStyle,  GUILayout.Height(height)))
            {
                var baseStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    normal =
                    {
                        background = LoadTexture(imageRoundWindowWide)
                    },
                    margin =
                    {
                        left = -5,
                        right = -5
                    },
                    fixedHeight = height,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                // ボタンごとのループ
                for (int i = 0; i < tabCount; i++)
                {
                    var buttonStyle = new GUIStyle(baseStyle);
                    if (i == currentTabIndex)
                    {
                        // 選択中のボタンは背景が暗いので、テキストは白っぽい色にします。
                        buttonStyle.normal.textColor = colorDefaultFont.Dark;
                    }
                    var backgroundColorTint = i == currentTabIndex ? colorButtonMain.Color : colorButtonSub.Color;
                    if (ButtonWithColorTint(new GUIContent(tabNames[i]), backgroundColorTint, buttonStyle, GUILayout.Height(height)))
                    {
                        nextTabIndex = i;
                    }
                }
            }

            return nextTabIndex;
        }

        /// <summary>
        /// 背景色をベースに対する乗算で指定するボタンです。
        /// </summary>
        private static bool ButtonWithColorTint(GUIContent buttonContent, Color backgroundColorTint, GUIStyle buttonStyle, params GUILayoutOption[] options)
        {
            var prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColorTint;
            bool isPushed = GUILayout.Button(buttonContent, buttonStyle, options);
            GUI.backgroundColor = prevBackgroundColor;
            return isPushed;
        }

        /// <summary>
        /// スクリーンの幅からスクロールバーを除いたものを返します。
        /// </summary>
        private static int ScreenDrawableWidth => Screen.width - 15;

        public static void SubTitle(string text)
        {
            var lineImageL =
                LoadTexture(imageGradationShortInverted.RelativePath);
            var lineImageR =
                LoadTexture(imageGradationShort.RelativePath);
            
            CenterAlignHorizontal(() =>
            {
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
            });
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
                var cachedTex = cachedTexture[colorCode];
                if (cachedTex != null)
                {
                    return cachedTexture[colorCode];
                }
            }
            Texture2D tex = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString(colorCode, out Color col);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            cachedTexture[colorCode] = tex;
            return tex;
        }

        public static void MainLogo()
        {
            const int logoMaxWidth = 300;
            var tex = LoadTexture(imageNameLogo);
            if (tex is null) return;
            float width = Math.Min(Math.Min(tex.width, ScreenDrawableWidth), logoMaxWidth);
            float height = tex.height * width / tex.width;
            using (new EditorGUILayout.VerticalScope(new GUIStyle(StyleLogoBackground)))
            {
                LogoLine();
                const int imageTopMargin = 10;
                const int imageBottomMargin = 0;
                var imageStyle = new GUIStyle(EditorStyles.label)
                {
                    fixedHeight = height,
                    fixedWidth = width - 15,
                    margin = new RectOffset(0, 0, imageTopMargin, imageBottomMargin)
                };
                var imageContent = new GUIContent(tex);
                var logoSize = imageStyle.CalcSize(imageContent);
                CenterAlignHorizontal(() =>
                    {
                        EditorGUILayout.LabelField(imageContent, imageStyle, GUILayout.MaxWidth(width), GUILayout.MaxHeight(height));
                    }
                    , GUILayout.Height(logoSize.y + imageTopMargin + imageBottomMargin)
                    , GUILayout.Width(ScreenDrawableWidth)
                );
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

        public static void CenterAlignHorizontal(Action drawFunc, params GUILayoutOption[] layoutOptions)
        {
            using (new EditorGUILayout.HorizontalScope(layoutOptions))
            {
                GUILayout.FlexibleSpace();
                drawFunc();
                GUILayout.FlexibleSpace();
            }
        }

        private static void CenterAlignVertical(Action drawFunc, params GUILayoutOption[] layoutOptions)
        {
            using (new EditorGUILayout.VerticalScope(layoutOptions))
            {
                GUILayout.FlexibleSpace();
                drawFunc();
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// テクスチャをロードし、キャッシュに追加してから返します。
        /// すでにキャッシュにあれば、ファイルロードの代わりにキャッシュから返します。
        /// 画像ファイルのパスがキャッシュのキーとなります。
        /// </summary>
        /// <param name="relativePath">画像ファイルの相対パスで、 <see cref="imageDirPath"/>を基準としたパスを指定します。</param>
        private static Texture2D LoadTexture(string relativePath)
        {
            string assetPath = Path.Combine(imageDirPath, relativePath);
            if (cachedTexture.TryGetValue(assetPath, out var cacheHitTexture))
            {
                if (cacheHitTexture != null)
                {
                    return cacheHitTexture;
                }
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex is null)
            {
                Debug.LogError($"Texture is not found : assetPath = {assetPath}");
            }
            cachedTexture[assetPath] = tex;
            return tex;
        }

        /// <summary>
        /// Unityのテーマがダークかライトかによってパスを切り替えるクラスです。
        /// </summary>
        private class ImagePathLightDark
        {
            private readonly string lightModePathRelative;
            private readonly string darkModePathRelative;
            public ImagePathLightDark(string lightModePathRelative, string darkModePathRelative)
            {
                this.lightModePathRelative = lightModePathRelative;
                this.darkModePathRelative = darkModePathRelative;
            }

            public string RelativePath =>
                EditorGUIUtility.isProSkin ? this.darkModePathRelative : this.lightModePathRelative;
        }

        /// <summary>
        /// Unityのテーマがライトかダークかによって色を切り替えるクラスです。
        /// </summary>
        private class ColorLightDark
        {
            private readonly Color lightModeColor;
            private readonly Color darkModeColor;

            public ColorLightDark(string lightModeColorCode, string darkModeColorCode)
            {
                ColorUtility.TryParseHtmlString(lightModeColorCode, out this.lightModeColor);
                ColorUtility.TryParseHtmlString(darkModeColorCode, out this.darkModeColor);
            }

            /// <summary>
            /// 現在のUnityテーマに応じた色を返します。
            /// </summary>
            public Color Color =>
                EditorGUIUtility.isProSkin ? this.darkModeColor : this.lightModeColor;

            public Color Dark => this.darkModeColor;
            // public Color Light => this.lightModeColor;
        }
    }
}