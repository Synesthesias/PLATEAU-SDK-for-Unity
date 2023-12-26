using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    /// <summary>
    /// 元のオブジェクトを削除するか残すか選択するGUIを描画します。
    /// </summary>
    internal class DestroyOrPreserveSrcGUI
    {
        public enum PreserveOrDestroy
        {
            Preserve, Destroy
        }
        
        private static readonly string[] DestroySrcOptions = { "残す", "削除する" };
        public PreserveOrDestroy Current { get; private set; } = PreserveOrDestroy.Preserve;

        public void Draw()
        {
            Current = (PreserveOrDestroy)
                PlateauEditorStyle.PopupWithLabelWidth(
                    "元のオブジェクトを", (int)Current, DestroySrcOptions, 90);
        }
    }
}