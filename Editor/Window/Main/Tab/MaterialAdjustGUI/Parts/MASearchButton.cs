using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分けの検索ボタンです。
    /// ただし、検索済みの場合は代わりに「再選択」ボタンを描画します。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    internal class MASearchButton : Element
    {
        private readonly CityMaterialAdjustGUI adjustGui;
        private readonly EditorWindow parentEditorWindow;

        public MASearchButton(CityMaterialAdjustGUI adjustGui, EditorWindow parentEditorWindow)
        {
            this.adjustGui = adjustGui;
            this.parentEditorWindow = parentEditorWindow;
        }
        
        public override void DrawContent()
        {
            var currentAdjuster = adjustGui.CurrentSearcher;
            using (PlateauEditorStyle.VerticalScopeWithPadding(0, 0, 15, 0))
            {
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    if (!adjustGui.IsSearched)
                    {
                        if (PlateauEditorStyle.MiniButton("検索", 150))
                        {
                            using var progressBar = new ProgressBar("検索中です...");
                            progressBar.Display(0.4f);
                            SearchArg searchArg = adjustGui.GenerateSearchArg();
                            bool searchSucceed = currentAdjuster.Search(searchArg);
                            adjustGui.IsSearched = searchSucceed;
                            parentEditorWindow.Repaint();
                        }
                    }
                });
            }
        }
        
        public override void Dispose(){}
    }
}