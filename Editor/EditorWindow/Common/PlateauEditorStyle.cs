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
        // 実装の留意点:
        // 【利用する GUIStyle について】
        // GUIStyle を作るとき、 new GUIStyle(baseStyle) という表記になります。
        // ここで baseStyle は EditorStyles.Label 等になります。
        // baseStyle で EditorStyles.toolbarButton など、ボタン系のものを指定することは推奨しません。
        // Unity標準のボタン系スタイルは、Windowsのディスプレイ設定で 拡大/縮小 を 100% 以外に設定したときに
        // 背景画像を設定していても Unity デフォルト背景に置き換わる問題があります。
        // ボタンの背景として角丸画像を利用する PlateauEditorStyle ではなおさら問題となります。
        // そのため、 baseStyle には EditorStyles.label を推奨します。
        // label からボタンっぽい見た目にするには、 alignment = TextAnchor.MiddleCenter に設定すれば良いです。


        private static readonly string imageDirPath = PathUtil.SdkPathToAssetPath("Images");
        private const string CyanBackgroundDark = "#292e30";
        private const string CyanBackgroundLight = "#abc4c9";
        private static readonly ColorLightDark colorDarkBoxBackground = new ColorLightDark("#515151", "#191919");
        private static readonly ColorLightDark colorButtonMain = new ColorLightDark("#005858", "#005858");
        private static readonly ColorLightDark colorButtonCancel = new ColorLightDark("#B70000", "#960000");
        private static readonly ColorLightDark colorButtonSub = new ColorLightDark("#E4E4E4", "#676767");
        private static readonly ColorLightDark colorDefaultFont = new ColorLightDark("#090909", "#C4C4C4");
        private static readonly ColorLightDark colorDefaultBackground = new ColorLightDark("#C8C8C8", "#383838");
        private static readonly ColorLightDark colorFoldOutBackground = new ColorLightDark("#BBBBBB", "#3E3E3E");
        private static readonly ColorLightDark colorNumberDisplayBackground = new ColorLightDark("#E4E4E4", "#191919");
        private const string ColorDarkBoxSelectedElement = "#676767";
        private const string ColorDarkBoxClickedElement = "#303030";
        private const string ColorLogoBackground = "#676767";
        private const string ColorLogoLine = "#D2D2D2";
        private const string ImageNameLogo = "logo_for_unity.png";

        private static readonly ImagePathLightDark imageGradationLong =
            new ImagePathLightDark("light_gradation_long.png", "dark_gradation_long.png");

        private static readonly ImagePathLightDark imageGradationShortInverted =
            new ImagePathLightDark("light_gradation_short_inverted.png", "dark_gradation_short_inverted.png");

        private static readonly ImagePathLightDark imageGradationShort =
            new ImagePathLightDark("light_gradation_short.png", "dark_gradation_short.png");

        private static readonly ImagePathLightDark imageIconBuilding =
            new ImagePathLightDark("light_icon_building.png", "dark_icon_building.png");

        private const string ImageRoundButton = "round-button.png";
        private const string ImageRoundWindowWide = "round-window-wide.png";
        private const string ImageRoundTab = "round-tab.png";     
        private const string ImageRoundLineFrame = "round_line_frame.png";

        public static readonly Color AreaGizmoBoxColor = new Color(0f, 84f / 255f, 1f);
        
        private static readonly Dictionary<string, Texture2D> cachedTexture = new Dictionary<string, Texture2D>();

        private static UnityEditor.EditorWindow currentWindow;

        /// <summary>
        /// このクラスを使うための設定として、EditorWindow の OnGUI の開始時にこのメソッドを呼ぶ必要があります。
        /// </summary>
        public static void SetCurrentWindow(UnityEditor.EditorWindow window)
        {
            currentWindow = window;
        } 

        /// <summary>
        /// 見出しを表示します。
        /// 行頭にアイコンを表示します。アイコンのパスは引数で <see cref="imageDirPath"/> からの相対パスで指定します。
        /// パスが null の場合はアイコンを表示しません。
        /// </summary>
        public static void Heading(string text, string imageIconRelativePath)
        {
            const float Height = 40;
            var boxStyle = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };
            using (new EditorGUILayout.HorizontalScope(boxStyle, GUILayout.Height(Height)))
            {
                // 行頭のアイコン
                if (imageIconRelativePath != null)
                {
                    var imageIcon = LoadTexture(imageIconRelativePath);
                    var iconWidth = imageIcon.width * Height / imageIcon.height;
                    var iconStyle = new GUIStyle(EditorStyles.label)
                    {
                        fixedHeight = Height,
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
                var textWidth = textStyle.CalcSize(textContent).x + 6;
                CenterAlignVertical(() =>
                    {
                        EditorGUILayout.LabelField(textContent, textStyle, GUILayout.MaxWidth(textWidth));
                    }
                    , GUILayout.Height(Height), GUILayout.Width(textWidth)
                );
                // 行末の線の画像
                CenterAlignVertical(() =>
                    {
                        var imageLine = LoadTexture(imageGradationLong.RelativePath);
                        EditorGUILayout.LabelField(new GUIContent(imageLine));
                    }
                    , GUILayout.Height(Height)
                );

            }
        }

        /// <summary>
        /// ラベルの幅 = ラベルの中身の幅　となるラベルを描画します。
        /// </summary>
        public static void LabelSizeFit(GUIContent content, GUIStyle style = null)
        {
            style ??= new GUIStyle(EditorStyles.label);
            var width = style.CalcSize(content).x;
            EditorGUILayout.LabelField(content, style, GUILayout.Width(width));
        }

        private static void NumberDisplay(int num)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    background = LoadTexture(ImageRoundButton)
                },
                fontStyle = FontStyle.Bold
            };
            var prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = colorNumberDisplayBackground.Color;
            LabelSizeFit(new GUIContent($"  {num}  "), style);
            GUI.backgroundColor = prevBackgroundColor;
        }

        public static string IconPathBuilding => imageIconBuilding.RelativePath;

        public static void Separator(int indentLevel)
        {
            const float LineWidth = 1;
            const int MarginY = 15;
            var horizontalStyle = new GUIStyle
            {
                margin = new RectOffset(0, 0, MarginY, MarginY)
            };
            using (new GUILayout.HorizontalScope(horizontalStyle))
            {
                GUILayout.Space(indentLevel * 15);
                var boxStyle = new GUIStyle(EditorStyles.label)
                {
                    normal =
                    {
                        background = ColoredTexture(ColorDarkBoxSelectedElement)
                    },
                    margin = new RectOffset(0, 0, 0, 0) 
                };
                GUILayout.Box("", boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(LineWidth));
            }
        }

        /// <summary> ボタンのスタイルです。押されたときにtrueを返します。 </summary>
        public static bool MainButton(string text)
        {
            var buttonStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    background = LoadTexture(ImageRoundWindowWide),
                    textColor = colorDefaultFont.Dark
                },
                alignment = TextAnchor.MiddleCenter
            };
            bool isButtonPushed = false;
            CenterAlignHorizontal(() =>
            {
                isButtonPushed = ButtonWithColorTint(new GUIContent(text), colorButtonMain.Color, buttonStyle, GUILayout.Height(60), GUILayout.MaxWidth(240));
            });
            return isButtonPushed;
        }

        public static bool CancelButton(string text)
        {
            var buttonStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    background = LoadTexture(ImageRoundWindowWide),
                    textColor = colorDefaultFont.Dark
                },
                alignment = TextAnchor.MiddleCenter
            };
            bool isButtonPushed = false;
            CenterAlignHorizontal(() =>
            {
                isButtonPushed = ButtonWithColorTint(new GUIContent(text), colorButtonCancel.Color, buttonStyle, GUILayout.Height(60), GUILayout.MaxWidth(240));
            });
            return isButtonPushed;
        }

        public static bool MiniButton(string text, int width)
        {
            var buttonStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    background = LoadTexture(ImageRoundWindowWide),
                    textColor = colorDefaultFont.Dark
                },
                alignment = TextAnchor.MiddleCenter
            };
            bool isButtonPushed = ButtonWithColorTint(new GUIContent(text), colorButtonMain.Color, buttonStyle,
                GUILayout.Height(40), GUILayout.MaxWidth(width));
            return isButtonPushed;
        }

        /// <summary> 複数行のラベルを表示します。 </summary>
        private static void MultiLineLabel(string text)
        {
            EditorGUILayout.LabelField(text, new GUIStyle(StyleMultiLineLabel));
        }

        private static GUIStyle StyleMultiLineLabel => new GUIStyle(EditorStyles.label)
        {
            wordWrap = true
        };

        /// <summary> 複数行のラベルを表示して Box で囲みます。 </summary>
        public static void MultiLineLabelWithBox(string text, params GUILayoutOption[] options)
        {
            using (VerticalScopeLevel2(options))
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
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel1(int indent = 0)
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLevel1(indent));
        }

        /// <summary>
        /// 中のGUIコンテンツをグレーの Box で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel2(params GUILayoutOption[] options)
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLevel2, options);
        }

        /// <summary>
        /// 中のGUIコンテンツを青の Box で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeLevel3()
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLevel3);
        }

        /// <summary>
        /// 中のGUIコンテンツを白線 で囲みます。
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalLineFrame()
        {
            return new EditorGUILayout.VerticalScope(ContentStyleLineFrame);
        }

        public static EditorGUILayout.VerticalScope VerticalScopeDarkBox()
        {
            return new EditorGUILayout.VerticalScope(new GUIStyle(StyleDarkBox));
        }

        /// <summary>
        /// Padding付きのGUIコンテンツを　作成します
        /// </summary>
        public static EditorGUILayout.VerticalScope VerticalScopeWithPadding(int left, int right, int top, int bottom)
        {
            GUIStyle ContentStylePadding = new GUIStyle()
            {
                padding = new RectOffset(left, right, top, bottom),
            };
            return new EditorGUILayout.VerticalScope(new GUIStyle(ContentStylePadding));
        }

        /// <summary>
        /// LOD選択スライダーを描画します。
        /// </summary>
        /// <param name="label">ラベルです。</param>
        /// <param name="minVal">ユーザーが選択したスライダーの最小値を ref で返します。</param>
        /// <param name="maxVal">ユーザーが選択したスライダーの最大値を ref で返します。</param>
        /// <param name="minLODLimit">選択可能な範囲の最小値です。</param>
        /// <param name="maxLODLimit">選択可能な範囲の最大値です。</param>
        public static void LODSlider(string label, ref int minVal, ref int maxVal, int minLODLimit, int maxLODLimit)
        {
            if (minLODLimit == maxLODLimit)
            {
                (minVal, maxVal) = (minLODLimit, maxLODLimit);
                return;
            }

            (float sliderValMin, float sliderValMax) = (minVal, maxVal);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(150));
                NumberDisplay(minVal);
                EditorGUILayout.MinMaxSlider("", ref sliderValMin, ref sliderValMax, minLODLimit, maxLODLimit);
                NumberDisplay(maxVal);
            }

            minVal = (int)Mathf.Round(sliderValMin);
            maxVal = (int)Mathf.Round(sliderValMax);
        }

        /// <summary>
        /// GUIのコンテンツをまとめるのに利用できます。
        /// </summary>
        private static GUIStyle ContentStyleLevel1(int indent)
        {
            int marginLeft = Math.Min(1, indent) * 12;
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
        private static GUIStyle ContentStyleLevel3 => new GUIStyle(ContentStyleLevel1(0))
        {
            normal =
            {
                background = ColoredTexture(EditorGUIUtility.isProSkin ? CyanBackgroundDark : CyanBackgroundLight)
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

        private static GUIStyle ContentStyleLineFrame => new GUIStyle(GUI.skin.box)
        {
            normal =
            {
                background = LoadTexture(ImageRoundLineFrame),
            },
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(8, 8, 0, 0),
            border = new RectOffset(5, 5, 5, 5),
        };

        private static GUIStyle StyleDarkBox => new GUIStyle
        {
            normal =
            {
                background = LoadTexture(ImageRoundWindowWide),
            },
            margin = new RectOffset(15, 15, 15, 15),
            padding = new RectOffset(5, 5, 15, 15)
        };


        private static GUIStyle StyleLogoBackground => new GUIStyle
        {
            normal =
            {
                background = ColoredTexture(ColorLogoBackground)
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
                .Select(path => (UnityEngine.Texture)LoadTexture(path))
                .ToArray();
            var buttonBackground = LoadTexture(ImageRoundButton);
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
                        
                        ColorUtility.TryParseHtmlString(ColorDarkBoxSelectedElement, out var selectedColorTint);
                        buttonBackgroundColorTint = selectedColorTint;
                        buttonStyle.normal.background = buttonBackground;
                        
                    }
                    else
                    {
                        buttonBackgroundColorTint = colorDarkBoxBackground.Color;
                        buttonStyle.active.background = ColoredTexture(ColorDarkBoxClickedElement);
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
            const int Height = 40;
            int tabCount = tabNames.Length;
            int nextTabIndex = currentTabIndex;
            var boxStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(30, 30, 0, 0)
            };
            using (new EditorGUILayout.HorizontalScope(boxStyle,  GUILayout.Height(Height)))
            {
                var baseStyle = new GUIStyle(EditorStyles.label)
                {
                    normal =
                    {
                        background = LoadTexture(ImageRoundWindowWide)
                    },
                    margin =
                    {
                        left = -5,
                        right = -5
                    },
                    fixedHeight = Height,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
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
                    if (ButtonWithColorTint(new GUIContent(tabNames[i]), backgroundColorTint, buttonStyle, GUILayout.Height(Height)))
                    {
                        nextTabIndex = i;
                    }
                }
            }

            return nextTabIndex;
        }

        /// <summary>
        /// フレーム上部に配置するタブを表示します。
        /// 選択されたタブの番号を返します。
        /// </summary>
        public static int TabsForFrame(int currentTabIndex, params string[] tabNames)
        {
            const int Height = 41;
            int tabCount = tabNames.Length;
            int nextTabIndex = currentTabIndex;
            var boxStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(15, 15, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
            };
            using (new EditorGUILayout.HorizontalScope(boxStyle, GUILayout.Height(Height)))
            {
                var baseStyle = new GUIStyle(EditorStyles.label)
                {
                    normal =
                    {
                        background = LoadTexture(ImageRoundTab)
                    },
                    margin =
                    {
                        left = 5,
                        right = 5, 
                    },
                    border =
                    {
                        left =3,
                        right =3,
                        top = 4,
                    },
                    padding =
                    {
                        top = 3,
                    },
                    fixedHeight = Height,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
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
                    if (ButtonWithColorTint(new GUIContent(tabNames[i]), backgroundColorTint, buttonStyle, GUILayout.Height(Height-2)))
                    {
                        nextTabIndex = i;
                    }
                }
            }
            return nextTabIndex;
        }

        public static bool FoldOut(bool foldOutState, string headerText, Action drawInner, float offsetWidth = 0)
        {
            // ヘッダーを描画します。
            var style = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fixedWidth = ScreenDrawableWidth - offsetWidth,
            };
            var textContent = new GUIContent(headerText);
            var colorTint = new Color(
                colorFoldOutBackground.Color.r / colorDefaultBackground.Color.r,
                colorFoldOutBackground.Color.g / colorDefaultBackground.Color.g,
                colorFoldOutBackground.Color.b / colorDefaultBackground.Color.b);
            var prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = colorTint;
            foldOutState = EditorGUILayout.Foldout(foldOutState, textContent, true, style);
            GUI.backgroundColor = prevBackgroundColor;

            // 折りたたみ可能な中身を表示します。
            if (foldOutState)
            {
                drawInner();
            }
            return foldOutState;
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
        private static float ScreenDrawableWidth
        {
            get
            {
                if (currentWindow == null)
                {
                    Debug.LogError($"{nameof(PlateauEditorStyle)}.{nameof(SetCurrentWindow)} がコールされていません。各ウィンドウのGUI開始時にコールする必要があります。");
                    return 400; // デフォルト値。ウィンドウサイズの取得ができなかったので、代わりになんとなく横幅としてありそうな値を返します。
                }
                return currentWindow.position.width - 15;
            }
        }

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

        public static void CategoryTitle(string text) 
        {
            var contentStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(0, 0, 0, 0)
            };
            using (new EditorGUILayout.VerticalScope(contentStyle))
            {
                var textContent = new GUIContent(text);
                var textStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                EditorGUILayout.LabelField(text, textStyle);
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
                var cachedTex = cachedTexture[colorCode];
                if (cachedTex != null)
                {
                    return cachedTexture[colorCode];
                }
            }
            Texture2D tex = new Texture2D(4, 4);
            ColorUtility.TryParseHtmlString(colorCode, out Color col);
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            cachedTexture[colorCode] = tex;
            return tex;
        }

        public static void MainLogo()
        {
            const int LogoMaxWidth = 300;
            var tex = LoadTexture(ImageNameLogo);
            if (tex == null) return;
            float width = Math.Min(Math.Min(tex.width, ScreenDrawableWidth), LogoMaxWidth);
            float height = tex.height * width / tex.width;
            using (new EditorGUILayout.VerticalScope(new GUIStyle(StyleLogoBackground)))
            {
                LogoLine();
                const int ImageTopMargin = 10;
                const int ImageBottomMargin = 0;
                var imageStyle = new GUIStyle(EditorStyles.label)
                {
                    fixedHeight = height,
                    fixedWidth = width - 15,
                    margin = new RectOffset(0, 0, ImageTopMargin, ImageBottomMargin)
                };
                var imageContent = new GUIContent(tex);
                var logoSize = imageStyle.CalcSize(imageContent);
                CenterAlignHorizontal(() =>
                    {
                        EditorGUILayout.LabelField(imageContent, imageStyle, GUILayout.MaxWidth(width), GUILayout.MaxHeight(height));
                    }
                    , GUILayout.Height(logoSize.y + ImageTopMargin + ImageBottomMargin)
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
                        background = ColoredTexture(ColorLogoLine)
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

        public static void RightAlign(Action drawFunc, params GUILayoutOption[] layoutOptions)
        {
            using (new EditorGUILayout.HorizontalScope(layoutOptions))
            {
                GUILayout.FlexibleSpace();
                drawFunc();
            }
        }

        public static void CenterAlignVertical(Action drawFunc, params GUILayoutOption[] layoutOptions)
        {
            using (new EditorGUILayout.VerticalScope(layoutOptions))
            {
                GUILayout.FlexibleSpace();
                drawFunc();
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// テキストフィールドを表示します。
        /// ただし、入力された値が defaultValue の場合、表示上は defaultValueDisplay になり、実際は defaultValue を返します。
        /// </summary>
        public static string TextFieldWithDefaultValue(string label, string currentValue, string defaultValue, string defaultValueDisplay)
        {
            string display = currentValue == defaultValue ? defaultValueDisplay : currentValue;
            string nextValue = EditorGUILayout.TextField(label, display);
            if (nextValue == defaultValueDisplay) return defaultValue;
            return nextValue;
        }

        /// <summary>
        /// EditorGUILayout.Popup の、
        /// ラベルの幅を指定できる版です。
        /// </summary>
        public static int PopupWithLabelWidth(string label, int selectedIndex, string[] displayedOptions, int labelWidth)
        {
            var prevWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
            var ret = EditorGUILayout.Popup(label, selectedIndex, displayedOptions);
            EditorGUIUtility.labelWidth = prevWidth;
            return ret;
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
            if (tex == null)
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