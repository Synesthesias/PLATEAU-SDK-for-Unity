using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.EditorWindowCommon
{
    /// <summary>
    /// マウスホイールでスクロール可能な <see cref="IEditorWindowContents"/> です。
    /// </summary>
    public abstract class ScrollableEditorWindowContents : IEditorWindowContents
    {
        private Vector2 scrollPosition;
        public void DrawGUI()
        {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            DrawScrollable();
            EditorGUILayout.EndScrollView();
        }

        public abstract void DrawScrollable();
    }
}