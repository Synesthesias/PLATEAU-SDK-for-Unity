using System;
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
            string filePathWithoutExtension = Path.Combine(destDir, fileNameWithoutExtension);
            switch (options.FileFormat)
            {
                case MeshExportOptions.MeshFileFormat.Obj:
                    using (var objWriter = new ObjWriter())
                    {
                        objWriter.Write(filePathWithoutExtension + ".obj", model);
                    }
                    break;
                case MeshExportOptions.MeshFileFormat.Gltf:
                    using (var gltfWriter = new GltfWriter())
                    {
                        gltfWriter.Write(filePathWithoutExtension, model, options.GltfWriteOptions);
                    }
                    break;
                case MeshExportOptions.MeshFileFormat.Fbx:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options), "Unknown FileFormat to export.");
            }
        }
    }
}
