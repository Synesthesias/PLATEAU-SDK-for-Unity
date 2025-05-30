using PLATEAU.CityExport.Exporters;
using PLATEAU.Geometries;
using PLATEAU.MeshWriter;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGuiParts
{
    /// <summary>
    /// Model(中間形式)をFBXファイルに出力します。
    /// </summary>
    internal class ExportConfigGuiFbx : IExportConfigGUI
    {
        private CityExporterFbx cityExporterFbx = new CityExporterFbx();
        
        public void Draw()
        {
            cityExporterFbx.FbxFileFormat = (FbxFileFormat)EditorGUILayout.EnumPopup("FBXフォーマット", cityExporterFbx.FbxFileFormat);
        }

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            cityExporterFbx.CoordinateSystem = coordinateSystem;
        }

        public ICityExporter GetExporter() => cityExporterFbx;

    }
}
