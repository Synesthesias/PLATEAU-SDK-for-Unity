namespace PLATEAU.Editor.EditorWindow.Common
{
    /// <summary>
    /// EditorWindow に描画するインターフェイスです。
    /// <see cref="Draw"/> で描画します。 
    /// </summary>
    internal interface IEditorDrawable
    {
        /// <summary>
        /// EditorWindow用のコンテンツを描画します。
        /// </summary>
        public void Draw();
    }
}