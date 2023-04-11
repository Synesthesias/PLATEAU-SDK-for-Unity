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
            
            gmlTrans.parent = rootTrans;
            var package = fetchedGmlFile.Package;
            var packageConf = conf.GetConfigForPackage(package);
            var meshExtractOptions = MeshExtractOptions.DefaultValue();
            int maxLod = packageConf.maxLOD;
            int minLod = packageConf.minLOD;
            meshExtractOptions.ReferencePoint = referencePoint;
            meshExtractOptions.MeshAxes = MeshAxes;
            meshExtractOptions.MeshGranularity = packageConf.meshGranularity;
            meshExtractOptions.MaxLOD = (uint)maxLod;
            meshExtractOptions.MinLOD = (uint)minLod;
            meshExtractOptions.ExportAppearance = packageConf.includeTexture;
            meshExtractOptions.GridCountOfSide = 10; // TODO gridCountOfSideはユーザーが設定できるようにしたほうが良い
            meshExtractOptions.UnitScale = UnitScale;
            meshExtractOptions.CoordinateZoneID = conf.CoordinateZoneID;
            meshExtractOptions.ExcludeCityObjectOutsideExtent = ShouldExcludeCityObjectOutsideExtent(package);
            meshExtractOptions.ExcludeTrianglesOutsideExtent = ShouldExcludeTrianglesOutsideExtent(package);
            meshExtractOptions.Extent = conf.Extent;

            if (!meshExtractOptions.Validate(out var failureMessage))
            {
                progressDisplay.SetProgress(gmlName, 0f, $"失敗 : メッシュ設定に不正な点があります。 : {failureMessage}");
                return;
            }

            // ここはメインスレッドで呼ぶ必要があります。
            bool placingSucceed = await PlateauToUnityModelConverter.ConvertAndPlaceToScene(
                cityModel, meshExtractOptions, gmlTrans, progressDisplay, gmlName, packageConf.doSetMeshCollider, token
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
            var parserParams = new CitygmlParserParams(true, true, false);
            
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

        private static bool ShouldExcludeTrianglesOutsideExtent(PredefinedCityModelPackage package)
        {
            return !ShouldExcludeCityObjectOutsideExtent(package);
        }
    }
}
