using System;
using System.IO;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityExport
{
    /// <summary>
    /// Unityのモデルを <see cref="PolygonMesh.Model"/> (中間形式) にしてから 3Dモデルファイルに出力します。
    /// </summary>
    public static class UnityModelExporter
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

                VertexConverterBase vertexConverter = options.TransformType switch
                {
                    MeshExportOptions.MeshTransformType.Local => VertexConverterFactory.LocalCoordinateSystemConverter(options.MeshAxis, rootPos),
                    MeshExportOptions.MeshTransformType.PlaneCartesian => VertexConverterFactory.PlaneCartesianCoordinateSystemConverter(options.MeshAxis, referencePoint.ToUnityVector(), rootPos),
                    _ => throw new Exception("Unknown transform type.")
                };
                
                // Unity のメッシュを中間データ構造(Model)に変換します。
                var convertTargets = new GameObject[childTrans.childCount];
                for (int j = 0; j < childTrans.childCount; j++)
                {
                    convertTargets[j] = childTrans.GetChild(j).gameObject;
                }

                IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter = options.ExportTextures
                    ? new UnityMeshToDllSubMeshWithTexture(options.ExportDefaultTextures)
                    : new UnityMeshToDllSubMeshWithEmptyMaterial();

                bool InvertMesh = (options.MeshAxis == CoordinateSystem.ENU || options.MeshAxis == CoordinateSystem.WUN);
                using var model = UnityMeshToDllModelConverter.Convert(convertTargets, unityMeshToDllSubMeshConverter, options.ExportHiddenObjects, vertexConverter, InvertMesh);
                
                // Model をファイルにして出力します。
                // options.PlateauModelExporter は、ファイルフォーマットに応じて FbxModelExporter, GltfModelExporter, ObjModelExporter のいずれかです。
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(childName);
                options.Exporter.Export(destDir, fileNameWithoutExtension, model);
            }
        }

    }
}
