using PLATEAU.CityAdjust.MaterialAdjust;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// 属性情報によるマテリアル分けのGuiです。
    /// </summary>
    internal class MaterialCriterionAttrGui : MaterialCriterionGuiBase
    {

        public MaterialCriterionAttrGui() : base(new MaterialAdjusterByAttr())
        {
        }
        
    }
}