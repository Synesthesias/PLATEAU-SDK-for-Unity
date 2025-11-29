using System.IO;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Geometries;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            if (!IsValidArgs(ref destDir, instancedCityModel))
            {
                return;
            }

            using var geo = instancedCityModel.GeoReference;
            ExportInternal(destDir, instancedCityModel, geo, options, progress);
        }

        public async Task Export(string destDir, PLATEAUTileManager tileManager, MeshExportOptions options,
            IProgressBar progress, CancellationToken cancellationToken)
        {
             // 前提チェック
            if (!IsValidArgs(ref destDir, tileManager))
            {
                return;
            }

            if (tileManager.State != PLATEAUTileManager.ManagerState.Operating)
            {
                await tileManager.InitializeTiles();
            }

            using var geo = tileManager.CityModel.GeoReference;
            await ExportInternal(destDir,tileManager.DynamicTiles.ToArray(),tileManager, geo, options, progress);
            
        }

        /// <summary>
        /// シーン配置の都市モデルのエクスポート処理
        /// </summary>
        /// <param name="destDir"></param>
        /// <param name="target"></param>
        /// <param name="geo"></param>
        /// <param name="options"></param>
        /// <param name="progress"></param>
        private void ExportInternal(string destDir, PLATEAUInstancedCityModel target, GeoReference geo, MeshExportOptions options, IProgressBar progress)
        {
             // Unityのシーンから情報を読みます。
            progress?.Display("エクスポート準備中...", 0.1f);
            var trans = target.transform;
            int numChild = trans.childCount;

            int processedCount = 0;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);

                if ((!options.ExportHiddenObjects) && (!childTrans.gameObject.activeInHierarchy))
                {
                    continue;
                }

                // 進行状況を表示 (0.1～0.9の範囲で更新)
                float progressFloat = 0.1f + (processedCount * 0.8f / numChild);
                progress?.Display($"エクスポート中... ({processedCount + 1}/{numChild})", progressFloat);
                ExportMesh(destDir, childTrans, trans.position, geo, options);

                processedCount++;
            }

            progress?.Display("エクスポート完了", 1.0f);
        }

        /// <summary>
        /// DynamicTileのエクスポート処理
        /// </summary>
        /// <param name="destDir"></param>
        /// <param name="dynamicTile"></param>
        /// <param name="tileManager"></param>
        /// <param name="geo"></param>
        /// <param name="options"></param>
        /// <param name="progress"></param>
        private async Task ExportInternal(string destDir, PLATEAUDynamicTile[] dynamicTile, PLATEAUTileManager tileManager, GeoReference geo, MeshExportOptions options, IProgressBar progress)
        {
            progress?.Display("エクスポート準備中...", 0.1f);

            int processedCount = 0;
            for (int i = 0; i < dynamicTile.Length; i++)
            {
                //メモリ負荷を抑えるため１タイルごとにLoadして出力する
                var tiles = await tileManager.ForceLoadTiles(new List<string>() { dynamicTile[i].Address }, CancellationToken.None);

                if(tiles == null || !tiles[0].LoadHandle.IsValid() || tiles[0].LoadHandle.Result == null)
                {
                    Debug.LogError($"タイルのロードに失敗しました。アドレス: {dynamicTile[i].Address}");
                    continue;
                }

                var childTrans = tiles[0].LoadHandle.Result.transform;
                
                // 進行状況を表示 (0.1～0.9の範囲で更新)
                float progressFloat = 0.1f + (processedCount * 0.8f / dynamicTile.Length);
                progress?.Display($"エクスポート中... ({processedCount + 1}/{dynamicTile.Length})", progressFloat);
                ExportMesh(destDir, childTrans, tileManager.transform.position, geo, options);
                processedCount++;
            }

            progress?.Display("エクスポート完了", 1.0f);
        }

        /// <summary>
        /// 単一のTransformからメッシュをエクスポートします。
        /// </summary>
        private void ExportMesh(
            string destDir,
            Transform childTrans,
            Vector3 rootPosition,
            GeoReference geo,
            MeshExportOptions options)
        {

            var vertexConverter = VertexConverterFactory.CreateByExportOptions(
                options, geo.ReferencePoint, rootPosition
            );

            // Unity のメッシュを中間データ構造(Model)に変換します。
            var convertTargets = new UniqueParentTransformList();
            for (int j = 0; j < childTrans.childCount; j++)
            {
                convertTargets.Add(childTrans.GetChild(j));
            }

            IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter = options.ExportTextures
                ? new UnityMeshToDllSubMeshWithTexture(options.ExportDefaultTextures)
                : new UnityMeshToDllSubMeshWithEmptyMaterial();

            bool invertMesh =
                (options.MeshAxis == CoordinateSystem.ENU || options.MeshAxis == CoordinateSystem.WUN);

            // Disposeの必要があるConverterであれば、自動Disposeします。Dispose処理がなければ何もしません。
            using (unityMeshToDllSubMeshConverter as IDisposable)
            using (var model = UnityMeshToDllModelConverter.Convert(
                       convertTargets, unityMeshToDllSubMeshConverter,
                       options.ExportHiddenObjects, vertexConverter, invertMesh))
            {
                // Model をファイルにして出力します。
                // options.Exporter は、ファイルフォーマットに応じて FbxModelExporter, GltfModelExporter, ObjModelExporter のいずれかです。
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(childTrans.name);

                // ここでエクスポートします
                options.Exporter.Export(destDir, fileNameWithoutExtension, model);
            }
        }
        
        private bool IsValidArgs(ref string destDir, UnityEngine.Object target)
        {
            if (target == null)
            {
                Debug.LogError("target is null.");
                return false;
            }

            destDir = destDir.Replace('\\', '/');
            if (!Directory.Exists(destDir))
            {
                Debug.LogError($"Destination Path is not a folder. destination = '{destDir}'");
                return false;
            }

            return true;
        }
    }
}