using System;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// 非推奨です。
    /// EditorWindow に描画するインターフェイスです。
    /// <see cref="Draw"/> で描画します。
    /// 非推奨の理由 : UIToolkitに移行中なので、IGuiCreatableを利用して下さい。
    /// </summary>
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