using System;
using System.ComponentModel.Design;
using System.IO;
using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport.ModelConvert;
using PLATEAU.Interop;
using PLATEAU.MeshWriter;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.Editor.CityExport
{
    internal static class MeshExporter
    {
        public static void Export(string destDir, PLATEAUInstancedCityModel instancedCityModel, MeshExportOptions options)
        {
            if (instancedCityModel == null)
            {
                Debug.LogError($"{nameof(instancedCityModel)} is null.");
                return;
            }
            destDir = destDir.Replace('\\', '/');
            if (!Directory.Exists(destDir))
            {
                Debug.LogError($"Destination Path is not a folder. destination = '{destDir}'");
                return;
            }
            var trans = instancedCityModel.transform;
            int numChild = trans.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                var childName = childTrans.name;
                if (!childName.EndsWith(".gml")) continue;
                
                using var model = UnityMeshToDllModelConverter.Convert(childTrans.gameObject);
                
                ModelToFile(destDir, Path.GetFileNameWithoutExtension(childName), model, options);
                Debug.Log(model.DebugString());
            }
        }

        public static void ModelToFile(string destDir, string fileNameWithoutExtension, Model model,
            MeshExportOptions options)
        {
            switch (options.FileFormat)
            {
                case MeshFileFormat.OBJ:
                    string filePathWithoutExtension = Path.Combine(destDir, fileNameWithoutExtension);
                    using (var objWriter = new ObjWriter())
                    {
                        objWriter.Write(filePathWithoutExtension + ".obj", model);
                    }
                    break;
                case MeshFileFormat.GLTF:
                    using (var gltfWriter = new GltfWriter())
                    {
                        string fileExtension = options.GltfWriteOptions.GltfFileFormat switch
                        {
                            GltfFileFormat.GLB => ".glb",
                            GltfFileFormat.GLTF => ".gltf",
                            _ => throw new ArgumentException("Unknown gltf file format.")
                        };
                        string dirPath = Path.Combine(destDir, fileNameWithoutExtension);
                        Directory.CreateDirectory(dirPath);
                        string gltfFilePath = Path.Combine(dirPath, fileNameWithoutExtension + fileExtension);
                        options.GltfWriteOptions.TextureDirectoryPath = Path.Combine(dirPath, "textures");
                        
                        // TODO 出力結果をBlenderで見ると面の向きが逆？？
                        gltfWriter.Write(gltfFilePath, model, options.GltfWriteOptions);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options), "Unknown FileFormat to export.");
            }
        }
    }
}
