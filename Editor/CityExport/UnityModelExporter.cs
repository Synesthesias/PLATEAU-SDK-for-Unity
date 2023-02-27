using System;
using System.IO;
using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport.ModelConvert;
using PLATEAU.MeshWriter;
using PLATEAU.Native;
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
            // Unityのシーンから情報を読みます。
            var trans = instancedCityModel.transform;
            int numChild = trans.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                var childName = childTrans.name;
                if (!childName.EndsWith(".gml")) continue;

                if ((!options.ExportHiddenObjects) && (!childTrans.gameObject.activeInHierarchy))
                {
                    continue;
                }

                using var geoReference = instancedCityModel.GeoReference;
                var referencePoint = geoReference.ReferencePoint;
                var rootPos = trans.position;
                
                UnityMeshToDllModelConverter.VertexConvertFunc vertexConvertFunc = options.TransformType switch
                {
                    MeshExportOptions.MeshTransformType.Local => src =>
                    {
                        // instancedCityModel を基準とする座標にします。
                        var pos = src - rootPos;
                        return new PlateauVector3d(pos.x, pos.y, pos.z);
                    },
                    MeshExportOptions.MeshTransformType.PlaneCartesian => src =>
                    {
                        // 変換時の referencePoint をオフセットします。
                        var pos = referencePoint + new PlateauVector3d(src.x - rootPos.x, src.y - rootPos.y, src.z - rootPos.z);
                        return pos;
                    },
                    _ => throw new Exception("Unknown transform type.")
                };
                
                // Unity のメッシュを中間データ構造(Model)に変換します。
                using var model = UnityMeshToDllModelConverter.Convert(childTrans.gameObject, options.ExportTextures, options.ExportHiddenObjects, false, options.MeshAxis, vertexConvertFunc);
                
                // Model をファイルにして出力します。
                ModelToFile(destDir, Path.GetFileNameWithoutExtension(childName), model, options);
            }
        }

        private static void ModelToFile(string destDir, string fileNameWithoutExtension, Model model,
            MeshExportOptions options)
        {
            switch (options.FileFormat)
            {
                // TODO このへんのcase列挙、スマートにならないか？
                // TODO gltf出力、fbx出力のテストを書く
                case MeshFileFormat.OBJ:
                    string filePathWithoutExtension = Path.Combine(destDir, fileNameWithoutExtension).Replace('\\', '/');
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
                        
                        gltfWriter.Write(gltfFilePath, model, options.GltfWriteOptions);
                    }
                    break;
                case MeshFileFormat.FBX:
                    string destPath = Path.Combine(destDir, fileNameWithoutExtension + ".fbx");
                    FbxWriter.Write(destPath, model, options.FbxWriteOptions);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options), "Unknown FileFormat to export.");
            }
        }
    }
}
