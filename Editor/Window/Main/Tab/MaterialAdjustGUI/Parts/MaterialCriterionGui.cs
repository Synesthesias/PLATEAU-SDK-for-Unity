using System;
using System.Linq;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分けの基準（属性情報で分ける、地物型で分ける）を選択するGUIを表示します。
    /// </summary>
    internal class MaterialCriterionGui : Element
    {
        private int selectedCriterionID;

        private int SelectedCriterionID
        {
            get
            {
                return selectedCriterionID;
            }
            set
            {
                bool isChanged = selectedCriterionID != value;
                selectedCriterionID = value;
                if (isChanged)
                {
                    onCriterionChanged?.Invoke(SelectedCriterion);
                }
            }
        }
        
        private readonly MaterialCriterion[] criterionOptions =
            ((MaterialCriterion[])Enum.GetValues(typeof(MaterialCriterion)))
            .Where(c => c!=MaterialCriterion.None)
            .ToArray();
        private readonly string[] criterionOptionsDisplay;
        
        private MaterialCriterion SelectedCriterion => criterionOptions[selectedCriterionID];
        private Action<MaterialCriterion> onCriterionChanged;

        

        public MaterialCriterionGui(Action<MaterialCriterion> onCriterionChanged)
        {
            this.onCriterionChanged = onCriterionChanged;
            criterionOptionsDisplay = criterionOptions.Select(co => co.ToDisplayString()).ToArray();
            SelectedCriterionID = 0;
            onCriterionChanged?.Invoke(SelectedCriterion);
        }

        /// <summary>
        /// マテリアル分け基準の選択GUIを描画します。
        /// </summary>
        public override void DrawContent()
        {
            PlateauEditorStyle.Heading("マテリアル分類", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 50;
                SelectedCriterionID = EditorGUILayout.Popup("分類", SelectedCriterionID, criterionOptionsDisplay);
            }
        }

        public override void Dispose()
        {
            
        }
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