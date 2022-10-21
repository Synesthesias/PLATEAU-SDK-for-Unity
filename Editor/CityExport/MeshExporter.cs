using System;
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
        public static void Export(string destination, PLATEAUInstancedCityModel instancedCityModel, MeshExportOptions options)
        {
            
            var trans = instancedCityModel.transform;
            int numChild = trans.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                var childName = childTrans.name;
                if (!childName.EndsWith(".gml")) continue;
                using var model = UnityMeshToDllModelConverter.Convert(childTrans.gameObject);
                ModelToFile(destination, model, options);
            }
        }

        public static void ModelToFile(string destination, Model model, MeshExportOptions options)
        {
            switch (options.FileFormat)
            {
                case MeshExportOptions.MeshFileFormat.Obj:
                    using (var objWriter = new ObjWriter())
                    {
                        objWriter.Write(destination, model);
                    }
                    break;
                case MeshExportOptions.MeshFileFormat.Gltf:
                    using (var gltfWriter = new GltfWriter())
                    {
                        gltfWriter.Write(destination, model, options.GltfWriteOptions);
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
