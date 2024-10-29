using System;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// EditorWindow に描画するインターフェイスです。
    /// <see cref="Draw"/> で描画します。 
    /// </summary>
    [Obsolete("UIToolkitに移行中なので、IGuiCreatableを利用して下さい。")]
    internal interface IEditorDrawable : IDisposable
    {
        /// <summary>
        /// EditorWindow用のコンテンツを描画します。
        /// </summary>
        public void Draw();
    }
    
    internal interface IGuiCreatable : IDisposable
    {
        /// <summary>
        /// EditorWindow用のコンテンツを生成します。
        /// </summary>
        public VisualElement CreateGui();
    }
}