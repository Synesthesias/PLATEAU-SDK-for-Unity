using System;
using System.IO;
using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport.ModelConvert;
using PLATEAU.Native;
using UnityEngine;

namespace PLATEAU.Editor.CityExport
{
    /// <summary>
    /// Unityのモデルを <see cref="PolygonMesh.Model"/> (中間形式) にしてから 3Dモデルファイルに出力します。
    /// </summary>
    internal static class UnityModelExporter
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
                // options.PlateauModelExporter は、ファイルフォーマットに応じて FbxModelExporter, GltfModelExporter, ObjModelExporter のいずれかです。
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(childName);
                options.PlateauModelExporter.Export(destDir, fileNameWithoutExtension, model);
            }
        }
    }
}
