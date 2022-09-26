using System.Collections.Generic;
using System.Linq;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.ProgressDisplay
{
    public class ProgressDisplayWindow : UnityEditor.EditorWindow, IProgressDisplay
    {
        private List<Progress> progressList = new List<Progress>();
        
        public static ProgressDisplayWindow Open()
        {
            var window = GetWindow<ProgressDisplayWindow>("計算状況");
            window.Show();
            return window;
        }

        public void OnGUI()
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                foreach (var progress in this.progressList)
                {
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        EditorGUILayout.LabelField(progress.Name);
                        float sliderLower = 0f;
                        float sliderUpper = progress.Percentage;
                        EditorGUILayout.MinMaxSlider($"{progress.PercentageStr}", ref sliderLower, ref sliderUpper, 0f, 100f );
                        EditorGUILayout.LabelField(progress.Message);
                    }
                }
            }
        }

        public void SetProgress(string progressName, float percentage, string message)
        {
            var matched = this.progressList.FirstOrDefault(progress => progress.Name == progressName);
            if (matched == null)
            {
                this.progressList.Add(new Progress(progressName, percentage, message));
            }
            else
            {
                matched.Percentage = percentage;
                matched.Message = message;
            }
            Repaint();
        }


        // TODO IProgressあたりに移動
        public class Progress
        {
            public string Name;
            public float Percentage;
            public string Message;
            public string PercentageStr => this.Percentage.ToString("00") + "%";

            public Progress(string name, float percentage, string message)
            {
                this.Name = name;
                this.Percentage = percentage;
                this.Message = message;
            }
        }
    }
}
