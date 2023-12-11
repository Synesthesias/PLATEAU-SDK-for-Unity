using System.IO;
using PLATEAU.CityExport.Exporters;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// Model(中間形式)をOBJファイルにエクスポートします。
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
