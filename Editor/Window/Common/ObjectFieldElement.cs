using System;
using UnityEditor;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// ObjectFieldを<see cref="Element"/>でラップしたものです。
    /// </summary>
    internal class ObjectFieldElement<T> : Element where T : UnityEngine.Object
    {
        private readonly string label;
        private readonly Action<T> onSelectionChanged;
        public T SelectedObject { get; private set; }
        
        public ObjectFieldElement(string name, string label, Action<T> onSelectionChanged) : base(name)
        {
            this.label = label;
            this.onSelectionChanged = onSelectionChanged;
        }
        
        public override void DrawContent()
        {
            var nextSelected = EditorGUILayout.ObjectField(label, SelectedObject, typeof(T), true) as T;
            if (nextSelected != SelectedObject)
            {
                SelectedObject = nextSelected;
                onSelectionChanged?.Invoke(nextSelected);
            }
        }

        public override void Reset() { }
        public override void Dispose()
        {
        }
    }
}