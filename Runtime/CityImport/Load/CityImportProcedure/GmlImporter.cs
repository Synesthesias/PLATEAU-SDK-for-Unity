using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityImport.Load.CityImportProcedure
{
    /// <summary>
    /// PLATEAU インポート処理の一部として、GMLファイルを1つインポートします。
    /// </summary>
    internal static class GmlImporter
    {
        // インポート設定のうち、Unityでは必ずこうなるという定数部分です。
        public const CoordinateSystem MeshAxes = CoordinateSystem.EUN;
        public const float UnitScale = 1.0f;

        /// <summary>
        /// GMLファイルを1つ fetch (ローカルならコピー、サーバーならダウンロード)し、fetch先の <see cref="GmlFile"/> を返します。
        /// </summary>
        internal static async Task<GmlFile> Fetch(GmlFile gmlFile, string destPath, CityLoadConfig conf, IProgressDisplay progressDisplay, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (gmlFile.Path == null)
            {
                return null;
            }
            
            string gmlName = Path.GetFileName(gmlFile.Path);
            destPath = destPath.Replace('\\', '/');
            if (!destPath.EndsWith("/")) destPath += "/";
            
            var fetchedGmlFile = await GmlFetcher.FetchAsync(gmlFile, destPath, gmlName, progressDisplay, conf.DatasetSourceConfig.IsServer, token);

            return fetchedGmlFile;
        }
        
        /// <summary>
        /// fetch済みのGMLファイルを1つインポートします。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        internal static async Task Import(GmlFile fetchedGmlFile , CityLoadConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            PlateauVector3d referencePoint, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string gmlName = Path.GetFileName(fetchedGmlFile.Path);

            progressDisplay.SetProgress(gmlName, 20f, "GMLファイルをロード中");
            string gmlPathAfter = fetchedGmlFile.Path;
            if (gmlPathAfter == null) return;

            using var cityModel = await LoadGmlAsync(fetchedGmlFile, token);

            if (cityModel == null)
            {
                progressDisplay.SetProgress(gmlName, 0f, "失敗 : GMLファイルのパースに失敗しました。");
                return;
            }

            string udxFeature = $"/udx/{fetchedGmlFile.FeatureType}/";
            string relativeGmlPathFromFeature =
                gmlPathAfter.Substring(
                    gmlPathAfter.LastIndexOf(udxFeature,
                        StringComparison.Ordinal) + udxFeature.Length);
            // gmlファイルに対応するゲームオブジェクトの名称は、地物タイプフォルダからの相対パスにします。
            string gmlObjName = relativeGmlPathFromFeature;
            var gmlTrans = new GameObject(gmlObjName).transform;
            var package = fetchedGmlFile.Package;
            var packageConf = conf.GetConfigForPackage(package);
            MeshExtractOptions meshExtractOptions;
            try
            {
                gmlTrans.parent = rootTrans;
                // ユーザーが選択したインポート設定について、C#のclassからC++のstructに変換します。
                meshExtractOptions = new MeshExtractOptions(
                    referencePoint: referencePoint,
                    meshAxes: MeshAxes,
                    meshGranularity: packageConf.meshGranularity,
                    minLOD: (uint)packageConf.minLOD,
                    maxLOD: (uint)packageConf.maxLOD,
                    exportAppearance: packageConf.includeTexture,
                    gridCountOfSide: 10,
                    unitScale: UnitScale,
                    coordinateZoneID: conf.CoordinateZoneID,
                    excludeCityObjectOutsideExtent: ShouldExcludeCityObjectOutsideExtent(package),
                    excludePolygonsOutsideExtent: ShouldExcludePolygonsOutsideExtent(package),
                    extent: conf.Extent,
                    attachMapTile: true,
                    mapTileZoomLevel: 15); // TODO ここで定数で決め打っている部分は、ユーザーが選択できるようにすると良い
            }
            catch (Exception e)
            {
                progressDisplay.SetProgress(gmlName, 0f, $"失敗 : メッシュインポートの設定に失敗しました。\n{e.Message}");
                return;
            }

            // ここはメインスレッドで呼ぶ必要があります。
            bool placingSucceed = await PlateauToUnityModelConverter.ConvertAndPlaceToScene(
                cityModel, meshExtractOptions, gmlTrans, progressDisplay, gmlName, packageConf.doSetMeshCollider, packageConf.doSetAttrInfo, token
            );

            if (placingSucceed)
            {
                progressDisplay.SetProgress(gmlName, 100f, "完了");
            }
            else
            {
                progressDisplay.SetProgress(gmlName, 0f, "失敗 : モデルの変換または配置に失敗しました。");
            }
        }
        
        private static async Task<CityModel> LoadGmlAsync(GmlFile gmlInfo, CancellationToken token)
        {
            string gmlPath = gmlInfo.Path.Replace('\\', '/');

            // GMLをパースした結果を返しますが、失敗した時は null を返します。
            var cityModel = await Task.Run(() => ParseGML(gmlPath, token));

            return cityModel;

        }
        
        /// <summary> gmlファイルをパースします。 </summary>
        /// <param name="gmlAbsolutePath"> gmlファイルのパスです。 </param>
        /// <returns><see cref="CityGML.CityModel"/> を返します。ロードに問題があった場合は null を返します。</returns>
        private static CityModel ParseGML(string gmlAbsolutePath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (!File.Exists(gmlAbsolutePath))
            {
                Debug.LogError($"GMLファイルが存在しません。 : {gmlAbsolutePath}");
                return null;
            }
            var parserParams = new CitygmlParserParams(true, false, true, false);
            
            CityModel cityModel = null;
            try
            {
                cityModel = CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
            }
            catch (Exception e)
            {
                Debug.LogError($"GMLファイルのロードに失敗しました。 : {gmlAbsolutePath}.\n{e.Message}\n{e.StackTrace}");
            }

            return cityModel;
        }
        
        private static bool ShouldExcludeCityObjectOutsideExtent(PredefinedCityModelPackage package)
        {
            if (package == PredefinedCityModelPackage.Relief) return false;
            return true;
        }

        private static bool ShouldExcludePolygonsOutsideExtent(PredefinedCityModelPackage package)
        {
            return !ShouldExcludeCityObjectOutsideExtent(package);
        }
    }
}
