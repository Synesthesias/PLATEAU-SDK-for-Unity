using PLATEAU.Editor.Window.Common;

namespace PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts
{
    /// <summary>
    /// 元のオブジェクトを削除するか残すか選択するGUIを描画します。
    /// </summary>
    internal class DestroyOrPreserveSrcGui : Element
    {
        public enum PreserveOrDestroy
        {
            Preserve, Destroy
        }
        
        private static readonly string[] DestroySrcOptions = { "新規追加（元オブジェクトは残す）", "置き換える（元オブジェクトは削除）" };
        public PreserveOrDestroy Current { get; private set; } = PreserveOrDestroy.Destroy;

        public override void DrawContent()
        {
            Current = (PreserveOrDestroy)
                PlateauEditorStyle.PopupWithLabelWidth(
                    "オブジェクト配置", (int)Current, DestroySrcOptions, 90);
        }

        public bool DoDestroySrcObjs => Current == PreserveOrDestroy.Destroy;

        public override void Dispose(){}
    }
}