using PLATEAU.CityExport.Exporters;
using PLATEAU.Geometries;
using PLATEAU.MeshWriter;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGuiParts
{
    /// <summary>
    /// Model(中間形式)をGLTFファイルにエクスポートします。
    /// </summary>
    internal class ExportConfigGuiGltf : IExportConfigGUI
    {
        private CityExporterGltf cityExporterGltf = new CityExporterGltf();
        
        
        public void Draw()
        {
            cityExporterGltf.GltfFileFormat = (GltfFileFormat)EditorGUILayout.EnumPopup("Gltfフォーマット", cityExporterGltf.GltfFileFormat);
        }

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            // 今のところ設定不要
        }

        public ICityExporter GetExporter() => cityExporterGltf;

    }
}
