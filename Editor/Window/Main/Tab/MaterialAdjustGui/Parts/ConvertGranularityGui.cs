using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.Editor.Window.Common;
using PLATEAU.GranularityConvert;
using System;
using System.Linq;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// マテリアル分け画面のうち、分割結合に関する一般設定のGUIです。
    /// なおMAはMaterialAdjustの略です。
    /// GUIの値を受け取るにはコンストラクタでコールバックを登録します。
    /// </summary>
    internal class ConvertGranularityGui : Element
    {
        private ConvertGranularity granularity;

        private ConvertGranularity Granularity // set時にコールバックを呼びます
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
                    onGranularityChanged?.Invoke(value);
                }
            }
        }

        private bool doChangeGranularity = false;
        
        private static string[] granularityChoices = 
            ((ConvertGranularity[])Enum.GetValues(typeof(ConvertGranularity)))
            .Select(g => g.ToJapaneseString())
            .ToArray();

        private int selectedGranularityIndex = 2;

        private Action<ConvertGranularity> onGranularityChanged;
        private Action<bool> onDoChangeGranularityChanged;

        public ConvertGranularityGui(Action<ConvertGranularity> onGranularityChanged, Action<bool> onDoChangeGranularityChanged) : base("")
        {
            this.onGranularityChanged = onGranularityChanged;
            this.onDoChangeGranularityChanged = onDoChangeGranularityChanged;
            onGranularityChanged?.Invoke(Granularity);
            onDoChangeGranularityChanged?.Invoke(doChangeGranularity);
        }
        
        public override void DrawContent()
        {
            bool prevDoChange = doChangeGranularity;
            doChangeGranularity = EditorGUILayout.ToggleLeft("粒度を変更する", doChangeGranularity);
            if (prevDoChange != doChangeGranularity)
            {
                onDoChangeGranularityChanged?.Invoke(doChangeGranularity);
            }
            if (doChangeGranularity)
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    selectedGranularityIndex = EditorGUILayout.Popup("粒度", selectedGranularityIndex, granularityChoices);
                    Granularity = (ConvertGranularity)(selectedGranularityIndex);
                }
            }
            
        }

        public override void Dispose()
        {
            
        }
    }
}