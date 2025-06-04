using System.IO;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Util;
using System;
using UnityEngine;

namespace PLATEAU.CityExport
{
    /// <summary>
    /// Unityのモデルを <see cref="PolygonMesh.Model"/> (中間形式) にしてから 3Dモデルファイルに出力します。
    /// </summary>
    public class UnityModelExporter
    {
        public void Export(string destDir, PLATEAUInstancedCityModel instancedCityModel, MeshExportOptions options,
            IProgressBar progress)
        {
            // 前提チェック
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
            progress.Display("エクスポート準備中...", 0.1f);
            var trans = instancedCityModel.transform;
            int numChild = trans.childCount;
            int activeChildCount = 0;

            // アクティブな子オブジェクトの数をカウント
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                if (options.ExportHiddenObjects || childTrans.gameObject.activeInHierarchy)
                {
                    activeChildCount++;
                }
            }

            int processedCount = 0;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                var childName = childTrans.name;

                if ((!options.ExportHiddenObjects) && (!childTrans.gameObject.activeInHierarchy))
                {
                    continue;
                }

                // 進行状況を表示 (0.1～0.9の範囲で更新)
                float progressFloat = 0.1f + (processedCount * 0.8f / activeChildCount);
                progress.Display($"エクスポート中... ({processedCount + 1}/{activeChildCount})", progressFloat);

                using var geoReference = instancedCityModel.GeoReference;

                var vertexConverter = VertexConverterFactory.CreateByExportOptions(
                    options, geoReference.ReferencePoint, trans.position
                );

                // Unity のメッシュを中間データ構造(Model)に変換します。
                var convertTargets = new UniqueParentTransformList();
                for (int j = 0; j < childTrans.childCount; j++)
                {
                    convertTargets.Add(childTrans.GetChild(j));
                }

                IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter;
                if (options.ExportTextures)
                {
                    unityMeshToDllSubMeshConverter =
                        new UnityMeshToDllSubMeshWithTexture(options.ExportDefaultTextures);
                }
                else
                {
                    unityMeshToDllSubMeshConverter = new UnityMeshToDllSubMeshWithEmptyMaterial();
                }

                bool invertMesh =
                    (options.MeshAxis == CoordinateSystem.ENU || options.MeshAxis == CoordinateSystem.WUN);

                // Disposeの必要があるConverterであれば、自動Disposeします。Dispose処理がなければ何もしません。
                using (unityMeshToDllSubMeshConverter as IDisposable)
                    
                using (var model = UnityMeshToDllModelConverter.Convert(convertTargets, unityMeshToDllSubMeshConverter,
                           options.ExportHiddenObjects, vertexConverter, invertMesh))
                {
                    // Model をファイルにして出力します。
                    // options.PlateauModelExporter は、ファイルフォーマットに応じて FbxModelExporter, GltfModelExporter, ObjModelExporter のいずれかです。
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(childName);

                    // ここでエクスポートします
                    options.Exporter.Export(destDir, fileNameWithoutExtension, model);
                }

                processedCount++;
            }

            progress.Display("エクスポート完了", 1.0f);
        }
    }
}