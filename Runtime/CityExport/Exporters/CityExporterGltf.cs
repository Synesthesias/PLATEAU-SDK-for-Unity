using System;
using System.IO;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityExport.Exporters
{
    /// <summary>
    /// <see cref="Model"/>をGLTF形式のファイルにします。
    /// </summary>
    public class CityExporterGltf : ICityExporter
    {
        public GltfFileFormat GltfFileFormat { get; set; } = GltfFileFormat.GLB;
        
        public void Export(string destDir, string fileNameWithoutExtension, Model model)
        {
            using var gltfWriter = new GltfWriter();
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