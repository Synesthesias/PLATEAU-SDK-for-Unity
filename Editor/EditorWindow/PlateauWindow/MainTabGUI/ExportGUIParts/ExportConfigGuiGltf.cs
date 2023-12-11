using System;
using System.IO;
using PLATEAU.CityExport.Exporters;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
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

        public ICityExporter GetExporter() => cityExporterGltf;

    }
}
