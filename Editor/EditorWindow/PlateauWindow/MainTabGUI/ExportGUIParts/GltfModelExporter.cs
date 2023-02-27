using System;
using System.IO;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// Model(中間形式)をGLTFファイルにエクスポートします。
    /// </summary>
    internal class GltfModelExporter : IPlateauModelExporter
    {
        public GltfFileFormat GltfFileFormat { get; set; } = GltfFileFormat.GLB;
        
        
        public void DrawConfigGUI()
        {
            GltfFileFormat = (GltfFileFormat)EditorGUILayout.EnumPopup("Gltfフォーマット", GltfFileFormat);
        }

        public void Export(string destDir, string fileNameWithoutExtension, Model model)
        {
            using (var gltfWriter = new GltfWriter())
            {
                string fileExtension = GltfFileFormat switch
                {
                    GltfFileFormat.GLB => ".glb",
                    GltfFileFormat.GLTF => ".gltf",
                    _ => throw new ArgumentException("Unknown gltf file format.")
                };
                string dirPath = Path.Combine(destDir, fileNameWithoutExtension);
                Directory.CreateDirectory(dirPath);
                string gltfFilePath = Path.Combine(dirPath, fileNameWithoutExtension + fileExtension);
                string textureDir = Path.Combine(dirPath, "textures");
                        
                gltfWriter.Write(gltfFilePath, model, new GltfWriteOptions(GltfFileFormat, textureDir));
            }
        }
    }
}
