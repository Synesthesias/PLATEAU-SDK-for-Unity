using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.CityImport.Load.FileCopy;
using PLATEAU.CityImport.Setting;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityImport.Load
{
    /// <summary>
    /// GMLファイルに記載された都市モデルを Unity にインポートします。
    /// </summary>
    internal static class CityImporter
    {
        // インポート設定のうち、Unityで共通するものです。
        private const CoordinateSystem meshAxes = CoordinateSystem.EUN;
        private const float unitScale = 1.0f;

        /// <summary>
        /// <see cref="CityImporter"/> クラスのメインメソッドです。
        /// GMLファイルから都市モデルを読み、そのメッシュをUnity向けに変換してシーンに配置します。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        public static async Task ImportAsync(CityLoadConfig config, IProgressDisplay progressDisplay)
        {
            string sourcePath = config.SourcePathBeforeImport;
            string destPath = PathUtil.plateauSrcFetchDir;
            string destFolderName = Path.GetFileName(sourcePath);

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"インポート元パスが存在しません。 sourcePath = {sourcePath}");
                return;
            }
            
            var collection = new UdxFileCollection();
            progressDisplay.SetProgress("GMLファイル検索", 10f, "");
            var targetGmls = await Task.Run(() => CityFilesCopy.FindTargetGmls(
                sourcePath, config, out collection
            ));
            progressDisplay.SetProgress("GMLファイル検索", 100f, "完了");

            if (targetGmls.Count <= 0)
            {
                Debug.LogError("該当するGMLファイルがありません。");
                return;
            }

            foreach (var gml in targetGmls)
            {
                progressDisplay.SetProgress(Path.GetFileName(gml.Path), 0f, "未処理");
            }

            var rootTrans = new GameObject(destFolderName).transform;

            // 各GMLファイルで共通する設定です。
            var referencePoint = CalcCenterPoint(collection, config.CoordinateZoneID);
            
            // ルートのGameObjectにコンポーネントを付けます。 
            var cityModelComponent = rootTrans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
            cityModelComponent.GeoReference =
                new GeoReference(referencePoint, unitScale, meshAxes, config.CoordinateZoneID);
            
            // GMLファイルを同時に処理する最大数です。
            // 並列数が 4 くらいだと、1つずつ処理するよりも、全部同時に処理するよりも速いという経験則です。
            var sem = new SemaphoreSlim(4);
            
            await Task.WhenAll(targetGmls.Select(async gmlInfo =>
            {
                await sem.WaitAsync(); 
                try
                {
                    // ここはメインスレッドで呼ぶ必要があります。
                    await ImportGml(gmlInfo, destPath, config, collection, rootTrans, progressDisplay, referencePoint);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    sem.Release();
                }

            }));


            foreach (var gmlInfo in targetGmls) gmlInfo.Dispose();

        }

        /// <summary>
        /// GMLファイルを1つインポートします。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        private static async Task ImportGml(
            GmlFileInfo gmlInfo, string destPath, CityLoadConfig conf,
            UdxFileCollection collection, Transform rootTrans, IProgressDisplay progressDisplay,
            PlateauVector3d referencePoint)
        {
            string gmlName = Path.GetFileName(gmlInfo.Path);
            progressDisplay.SetProgress(gmlName, 0f, "インポート処理中");

            destPath = destPath.Replace('\\', '/');
            if (!destPath.EndsWith("/")) destPath += "/";
            
            // GMLと関連ファイルを StreamingAssets にコピーします。
            // ここは別スレッドで実行可能です。
            await Task.Run(() => collection.Fetch(destPath, gmlInfo));
            // ここでメインスレッドに戻ります。
            progressDisplay.SetProgress(gmlName, 20f, "GMLファイルをロード中");

            // GMLと関連ファイルをコピーしたので、パスをコピー後のものに更新します。
            // 元パスが　AAA/ルートフォルダ名/udx/パッケージ名/111.gml　だったとすると、
            // AAAの部分だけ置き換えます。
            string gmlPathBefore = Path.GetFullPath(gmlInfo.Path).Replace('\\', '/');
            string pathToReplace = Path.GetFullPath(Path.Combine(gmlPathBefore, "../../../../")).Replace('\\', '/');
            string gmlPathAfter = gmlPathBefore.Replace(pathToReplace, destPath);
            gmlInfo.Path = gmlPathAfter;
            
            using var cityModel = await LoadGmlAsync(gmlInfo);
            if (cityModel == null)
            {
                progressDisplay.SetProgress(gmlName, 0f, "失敗 : GMLファイルのパースに失敗しました。");
                return;
            }

            var gmlTrans = new GameObject(Path.GetFileName(gmlPathAfter)).transform;
            gmlTrans.parent = rootTrans;
            var package = gmlInfo.Package;
            var packageConf = conf.GetConfigForPackage(package);
            var meshExtractOptions = MeshExtractOptions.DefaultValue();
            meshExtractOptions.ReferencePoint = referencePoint;
            meshExtractOptions.MeshAxes = meshAxes;
            meshExtractOptions.MeshGranularity = packageConf.meshGranularity;
            meshExtractOptions.MaxLOD = packageConf.maxLOD;
            meshExtractOptions.MinLOD = packageConf.minLOD;
            meshExtractOptions.ExportAppearance = packageConf.includeTexture;
            meshExtractOptions.GridCountOfSide = 10; // TODO gridCountOfSideはユーザーが設定できるようにしたほうが良い
            meshExtractOptions.UnitScale = unitScale;
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
                cityModel, meshExtractOptions, gmlTrans, progressDisplay, gmlName, packageConf.doSetMeshCollider
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

        private static bool ShouldExcludeCityObjectOutsideExtent(PredefinedCityModelPackage package)
        {
            if (package == PredefinedCityModelPackage.Relief) return false;
            return true;
        }

        private static bool ShouldExcludeTrianglesOutsideExtent(PredefinedCityModelPackage package)
        {
            return !ShouldExcludeCityObjectOutsideExtent(package);
        }
        
        

        private static PlateauVector3d CalcCenterPoint(UdxFileCollection collection, int coordinateZoneID)
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            return collection.CalcCenterPoint(geoReference);
        }

        private static async Task<CityModel> LoadGmlAsync(GmlFileInfo gmlInfo)
        {
            string gmlPath = gmlInfo.Path;

            // GMLをパースした結果を返しますが、失敗した時は null を返します。
            var cityModel = await Task.Run(() => ParseGML(gmlPath));

            return cityModel;

        }
        
        /// <summary> gmlファイルをパースします。 </summary>
        /// <param name="gmlAbsolutePath"> gmlファイルのパスです。 </param>
        /// <returns><see cref="CityGML.CityModel"/> を返します。ロードに問題があった場合は null を返します。</returns>
        private static CityModel ParseGML(string gmlAbsolutePath)
        {
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
                Debug.LogError($"GMLファイルのロードに失敗しました。 : {gmlAbsolutePath}.\n{e.Message}");
            }

            return cityModel;
        }
    }
}
