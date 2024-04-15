using System;
using System.Linq;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分けの基準（属性情報で分ける、地物型で分ける）を選択するGUIを表示し、
    /// 選択に応じて対応するマテリアル分けインスタンスを返します。
    /// </summary>
    internal class MaterialCriterionGui
    {
        private int selectedCriterionId;
        private readonly MaterialCriterion[] criterionOptions = (MaterialCriterion[])Enum.GetValues(typeof(MaterialCriterion));
        private readonly string[] criterionOptionsDisplay;
        
        // マテリアル分けの基準が属性情報の場合と地物型の場合で、2つのインスタンスを用意します。
        private readonly MaterialAdjustByCriterion materialGuiByType = new(new MaterialAdjustExecutorByType());
        private readonly MaterialAdjustByCriterion materialGuiByAttr = new(new MaterialAdjustExecutorByAttr());
        
        public MaterialCriterion SelectedCriterion => criterionOptions[selectedCriterionId];

        /// <summary>
        /// 選択中のマテリアル分け基準に応じた、マテリアル分けインスタンスを返します。
        /// </summary>
        public MaterialAdjustByCriterion CurrentAdjuster
        {
            get
            {
                return SelectedCriterion switch
                {
                    MaterialCriterion.ByType => materialGuiByType,
                    MaterialCriterion.ByAttribute => materialGuiByAttr,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public MaterialCriterionGui()
        {
            criterionOptionsDisplay = criterionOptions.Select(co => co.ToDisplayString()).ToArray();
        }

        /// <summary>
        /// マテリアル分け基準の選択GUIを描画します。
        /// </summary>
        public void Draw()
        {
            PlateauEditorStyle.Heading("マテリアル分類", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 50;
                selectedCriterionId = EditorGUILayout.Popup("分類", selectedCriterionId, criterionOptionsDisplay);
            }
        }
    }

    /// <summary>
    /// マテリアル分けの基準です。
    /// </summary>
    internal enum MaterialCriterion
    {
        ByType, ByAttribute
    }

    internal static class MaterialCriterionExtension
    {
        public static string ToDisplayString(this MaterialCriterion criterion)
        {
            return criterion switch
            {
                MaterialCriterion.ByType => "地物型",
                MaterialCriterion.ByAttribute => "属性情報",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}