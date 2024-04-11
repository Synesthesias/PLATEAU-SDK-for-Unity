using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// 地物型によるマテリアル分けのGUIです。
    /// </summary>
    internal class MaterialCriterionTypeGui : MaterialCriterionGuiBase
    {

        public MaterialCriterionTypeGui() : base(new MaterialAdjusterByType())
        {
        }
        

        public override void DrawBeforeTargetSelect()
        {
            // 何もしません
        }
    }
}