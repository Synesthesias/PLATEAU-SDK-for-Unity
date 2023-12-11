using System.IO;
using PLATEAU.CityExport.Exporters;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// OBJ形式でエクスポートする設定GUIとエクスポーターです。（なお設定GUIは、今のところOBJ固有の項目がないので空です。）
    /// </summary>
    internal class ExportConfigGuiObj : IExportConfigGUI
    {
        private CityExporterObj cityExporterObj = new CityExporterObj();

        
        public void Draw()
        {
            // OBJファイルに固有の設定項目はありません。
        }

        public ICityExporter GetExporter() => cityExporterObj;

    }
}
