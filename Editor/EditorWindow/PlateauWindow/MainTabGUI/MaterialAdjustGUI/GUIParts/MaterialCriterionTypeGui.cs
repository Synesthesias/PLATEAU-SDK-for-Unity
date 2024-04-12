using PLATEAU.CityAdjust.MaterialAdjust;

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
    }
}