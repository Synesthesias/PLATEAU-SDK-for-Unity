using PLATEAU.MeshWriter;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// Model(中間形式)をGLTFファイルにエクスポートします。
    /// </summary>
    internal class GltfExportConfig : PlateauModelExporter
    {
        public GltfFileFormat GltfFileFormat { get; set; } = GltfFileFormat.GLB;
        
        public void DrawConfigGUI()
        {
            GltfFileFormat = (GltfFileFormat)EditorGUILayout.EnumPopup("Gltfフォーマット", GltfFileFormat);
        }

        
    }
}
