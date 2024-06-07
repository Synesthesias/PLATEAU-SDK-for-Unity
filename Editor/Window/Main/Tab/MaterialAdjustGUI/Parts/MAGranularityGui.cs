using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.Editor.Window.Common;
using System;
using System.Linq;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分け画面のうち、分割結合に関する一般設定のGUIです。
    /// なおMAはMaterialAdjustの略です。
    /// GUIの値を受け取るにはコンストラクタでコールバックを登録します。
    /// </summary>
    internal class MAGranularityGui : Element
    {
        private MAGranularity granularity;

        private MAGranularity Granularity // set時にコールバックを呼びます
        {
            get
            {
                return granularity;
            }
            set
            {
                bool isChanged = granularity != value;
                granularity = value;
                if (isChanged)
                {
                    onValueChanged?.Invoke(value);
                }
            }
        }

        private bool doChangeGranularity = false;
        
        // 粒度の選択肢のうち、DoNotChangeは「粒度を変更する」のチェックがないことで表現するので、ポップアップから選べるのはそれ以外とします。
        private static string[] granularityChoices = 
            ((MAGranularity[])Enum.GetValues(typeof(MAGranularity)))
            .Where(g => g != MAGranularity.DoNotChange)
            .Select(g => g.ToJapaneseString())
            .ToArray();

        private int selectedGranularityIndex = 2;

        private Action<MAGranularity> onValueChanged;

        public MAGranularityGui(Action<MAGranularity> onValueChanged) : base("")
        {
            this.onValueChanged = onValueChanged;
            onValueChanged?.Invoke(Granularity);
        }
        
        public override void DrawContent()
        {
            doChangeGranularity = EditorGUILayout.ToggleLeft("粒度を変更する", doChangeGranularity);
            if (doChangeGranularity)
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    selectedGranularityIndex = EditorGUILayout.Popup("粒度", selectedGranularityIndex, granularityChoices);
                    Granularity = (MAGranularity)(selectedGranularityIndex + 1); // +1 はDoNotChangeが選択肢にない分
                }
            }
            else
            {
                Granularity = MAGranularity.DoNotChange;
            }
            
        }

        public override void Dispose()
        {
            
        }
    }
}