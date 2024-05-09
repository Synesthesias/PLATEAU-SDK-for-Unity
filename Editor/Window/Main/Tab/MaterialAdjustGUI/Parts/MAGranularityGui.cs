using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.PolygonMesh;
using System;
using System.Linq;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分け画面のうち、分割結合に関する一般設定のGUIです。
    /// なおMAはMaterialAdjustの略です。
    /// </summary>
    internal class MAGranularityGui : Element
    {
        public MAGranularity Granularity { get; private set;}
        public bool IsExecButtonVisible { get; set; } = true;
        private bool doChangeGranularity = false;
        private IGranularityConvertExecutor executor;
        
        // 粒度の選択肢のうち、DoNotChangeは「粒度を変更する」のチェックがないことで表現するので、ポップアップから選べるのはそれ以外とします。
        private static string[] granularityChoices = 
            ((MAGranularity[])Enum.GetValues(typeof(MAGranularity)))
            .Where(g => g != MAGranularity.DoNotChange)
            .Select(g => g.ToJapaneseString())
            .ToArray();

        private int selectedGranularityIndex = 1;

        public MAGranularityGui(IGranularityConvertExecutor executor) : base("")
        {
            this.executor = executor;
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
                
                if (IsExecButtonVisible)
                {
                    if (PlateauEditorStyle.MainButton("粒度変更を実行"))
                    {
                        executor.ExecGranularityConvert();
                    }
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

    internal interface IGranularityConvertExecutor
    {
        void ExecGranularityConvert();
    }
}