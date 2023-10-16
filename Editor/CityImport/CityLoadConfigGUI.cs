using System;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport.PackageLodSettingGUIs;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityLoadConfig"/> を設定するGUIです。
    /// </summary>
    internal class CityLoadConfigGUI
    {
        private readonly IEditorDrawable[] guiComponents;

        public CityLoadConfigGUI(CityLoadConfig cityLoadConf, PackageToLodDict availablePackageLodsArg)
        {
            if(cityLoadConf == null) throw new ArgumentNullException(nameof(cityLoadConf));
            // パッケージ種ごとの設定GUI、その下に基準座標設定GUIが表示されるようにGUIコンポーネントを置きます。
            guiComponents = new IEditorDrawable[]
            {
                new PackageLoadSettingGUIList(availablePackageLodsArg, cityLoadConf),
                new PositionConfGUI(cityLoadConf)
            };
        }

        /// <summary>
        /// <see cref="CityLoadConfig"/> を設定するGUIを描画します。
        /// </summary>
        public void Draw()
        {
            foreach (var guiComponent in guiComponents)
            {
                guiComponent.Draw();
            }
        }

        /// <summary>
        /// インポートの基準座標を選択するGUIです。
        /// </summary>
        private class PositionConfGUI : IEditorDrawable
        {
            private CityLoadConfig conf;
            public PositionConfGUI(CityLoadConfig conf)
            {
                this.conf = conf;
            }
            
            public void Draw()
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.Heading("基準座標系からのオフセット値(メートル)", null);

                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        var refPoint = conf.ReferencePoint;
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                        {
                            if (PlateauEditorStyle.MiniButton("範囲の中心点を入力", 140))
                            {
                                GUI.FocusControl("");
                                refPoint = conf.SetReferencePointToExtentCenter();
                            }
                        });

                        refPoint.X = EditorGUILayout.DoubleField("X (東が正方向)", refPoint.X);
                        refPoint.Y = EditorGUILayout.DoubleField("Y (高さ)", refPoint.Y);
                        refPoint.Z = EditorGUILayout.DoubleField("Z (北が正方向)", refPoint.Z);
                        conf.ReferencePoint = refPoint;
                    }
                }
            }
        }
    }
}
