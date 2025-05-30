using PLATEAU.CityExport.Exporters;
using PLATEAU.Geometries;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGuiParts
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

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            // 空
        }

        public ICityExporter GetExporter() => cityExporterObj;

    }
}
