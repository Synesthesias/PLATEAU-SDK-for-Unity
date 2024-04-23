using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.PolygonMesh;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// マテリアル分け画面のうち、分割結合に関する一般設定のGUIです。
    /// なおMAはMaterialAdjustの略です。
    /// </summary>
    internal class MAGranularityGui : Element
    {
        public MeshGranularity MeshGranularity { get; private set;}
        
        public bool DoDestroySrcObjs { get; private set; }
        private readonly DestroyOrPreserveSrcGui destroyOrPreserveSrcGUI = new();
        
        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                MeshGranularity = GranularityGUI.Draw("粒度", MeshGranularity);
                destroyOrPreserveSrcGUI.Draw();
                DoDestroySrcObjs = destroyOrPreserveSrcGUI.Current ==
                                   DestroyOrPreserveSrcGui.PreserveOrDestroy.Destroy;
            }
        }

        public override void Dispose()
        {
            
        }
    }
}