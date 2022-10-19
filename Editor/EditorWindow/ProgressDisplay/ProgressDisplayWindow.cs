using System.Threading;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.ProgressDisplay
{
    internal class ProgressDisplayWindow : UnityEditor.EditorWindow, IProgressDisplay
    {
        private ProgressDisplayGUI progressGUI;
        private SynchronizationContext mainThreadContext;
        private Vector2 scrollPos;

        public static ProgressDisplayWindow Open(SynchronizationContext mainThreadContext)
        {
            var window = GetWindow<ProgressDisplayWindow>("計算状況");
            window.Show();
            window.Init(mainThreadContext);
            return window;
        }

        private void Init(SynchronizationContext mainThreadContextArg)
        {
            this.progressGUI = new ProgressDisplayGUI();
            this.mainThreadContext = mainThreadContextArg;
        }

        public void OnGUI()
        {
            if (this.progressGUI == null)
            {
                EditorGUILayout.LabelField("初期化に失敗しました。ウィンドウを開き直してください。");
                return;
            }
            using (var scrollView = new EditorGUILayout.ScrollViewScope(this.scrollPos))
            {
                this.scrollPos = scrollView.scrollPosition;
                this.progressGUI.Draw();
            }
        }

        public void SetProgress(string progressName, float percentage, string message)
        {
            this.progressGUI.SetProgress(progressName, percentage, message);
            // 再描画はメインスレッドで行う必要があります。
            this.mainThreadContext.Post(__ =>
            {
                Repaint();
            }, null);
        }

    }
}
