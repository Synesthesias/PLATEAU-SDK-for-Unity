using PLATEAU.CityImport.Config;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.GUIParts
{
    /// <summary>
    /// インポートの基準座標を選択するGUIです。
    /// </summary>
    internal class PositionConfGUI : IEditorDrawable
    {
        private readonly CityLoadConfig conf;
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
        public void Dispose() { }
    }
}