using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.ProgressDisplay
{
    public class ProgressDisplayWindow : UnityEditor.EditorWindow, IProgressDisplay
    {
        private readonly ConcurrentBag<Progress> progressBag = new ConcurrentBag<Progress>();
        private SynchronizationContext mainThreadContext;
        private Vector2 scrollPos;
        
        public static ProgressDisplayWindow Open(SynchronizationContext mainThreadContext)
        {
            var window = GetWindow<ProgressDisplayWindow>("計算状況");
            window.Show();
            window.mainThreadContext = mainThreadContext;
            return window;
        }

        public void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(this.scrollPos))
            {
                this.scrollPos = scrollView.scrollPosition;
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    foreach (var progress in this.progressBag)
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
        }

        public void SetProgress(string progressName, float percentage, string message)
        {
            var matched = this.progressBag.FirstOrDefault(progress => progress.Name == progressName);
            if (matched == null)
            {
                this.progressBag.Add(new Progress(progressName, percentage, message));
            }
            else
            {
                matched.Percentage = percentage;
                matched.Message = message;
            }

            // 再描画はメインスレッドで行う必要があります。
            this.mainThreadContext.Post(__ => Repaint(), null);
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
