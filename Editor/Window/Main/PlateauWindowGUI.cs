using PLATEAU.Editor.Window.Common;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAUウィンドウで最上位のタブとそのコンテンツを保持・描画します。
    /// </summary>
    internal class PlateauWindowGUI : IEditorDrawable
    {
        private readonly TabWithImage tabs;
       
        // 最上位タブの内容を注入します
        public PlateauWindowGUI(TabWithImage tabs)
        {
            this.tabs = tabs;
        }

        public void Draw()
        {
            tabs.DrawTab();
            PlateauEditorStyle.MainLogo();
            tabs.DrawContent();
        }
        

        public void Dispose()
        {
            tabs.Dispose();
        }
    }
}
