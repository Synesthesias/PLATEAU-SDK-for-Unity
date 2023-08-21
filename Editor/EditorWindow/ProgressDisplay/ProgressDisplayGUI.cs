using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.ProgressDisplay
{
    /// <summary>
    /// 進捗表示のGUIを描画します。
    /// </summary>
    internal class ProgressDisplayGUI : IProgressDisplay
    {
        private readonly ConcurrentBag<DisplayedProgress> progressBag = new ConcurrentBag<DisplayedProgress>();
        private readonly SynchronizationContext mainThreadContext;
        private UnityEditor.EditorWindow ParentEditorWindow { get; set; }

        /// <summary>
        /// メインスレッドから読んでください。
        /// </summary>
        public ProgressDisplayGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.mainThreadContext = SynchronizationContext.Current;
            ParentEditorWindow = parentEditorWindow;
        }

        private bool IsEmpty => this.progressBag.IsEmpty;

        public void Draw()
        {
            PlateauEditorStyle.Separator(0);
            
            if (IsEmpty) return;
            
            PlateauEditorStyle.CenterAlignHorizontal(() =>
            {
                PlateauEditorStyle.LabelSizeFit(new GUIContent("インポート処理"));
            });
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                foreach (var progress in this.progressBag)
                {
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                        {
                            PlateauEditorStyle.LabelSizeFit(new GUIContent(progress.Name));
                        });
                        float sliderLower = 0f;
                        float sliderUpper = progress.Percentage;
                        EditorGUILayout.MinMaxSlider("", ref sliderLower, ref sliderUpper, 0f, 100f );
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                        {
                            PlateauEditorStyle.LabelSizeFit(new GUIContent($"{progress.PercentageStr}"));
                        });
                        PlateauEditorStyle.CenterAlignHorizontal(() =>
                        { 
                            PlateauEditorStyle.LabelSizeFit(new GUIContent(progress.Message));
                        });
                        
                    }
                }
            }
        }

        public void SetProgress(string progressName, float percentage, string message)
        {
            var matched = this.progressBag.FirstOrDefault(progress => progress.Name == progressName);
            if (matched == null)
            {
                this.progressBag.Add(new DisplayedProgress(progressName, percentage, message));
            }
            else
            {
                matched.Percentage = percentage;
                matched.Message = message;
            }

            if (ParentEditorWindow != null)
            {
                this.mainThreadContext.Post(_ =>
                {
                    ParentEditorWindow.Repaint();
                }, null);
            }
        }
    }
}
