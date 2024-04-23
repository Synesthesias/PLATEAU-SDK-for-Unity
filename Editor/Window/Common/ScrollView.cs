using System;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// スクロールビューです。
    /// メソッド<see cref="Draw"/>の引数に描画関数を渡すと、スクロールビュー内で描画します。
    /// </summary>
    public class ScrollView
    {
        private Vector2 scrollPosition;
        private readonly GUILayoutOption[] options;
        
        public ScrollView(params GUILayoutOption[] options)
        {
            this.options = options;
        }

        public void Draw(Action drawAction)
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, options);
            scrollPosition = scrollView.scrollPosition;
            drawAction();
        }
    }
}