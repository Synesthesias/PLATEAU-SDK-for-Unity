using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.CityImport.Load.FileCopy;
using PLATEAU.CityImport.Setting;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityImport.Load
{
    internal static class CityImporter
    {

        public static async Task ImportAsync(PLATEAUCityModelLoader loader, IProgressDisplay progressDisplay)
        {
            string sourcePath = loader.SourcePathBeforeImport; 
            string destPath = PathUtil.plateauSrcFetchDir;
            string destFolderName = Path.GetFileName(sourcePath);
            var targetGmls = CityFilesCopy.FindTargetGmls(
                sourcePath, loader.CityLoadConfig, out var collection
            );
            if (targetGmls.Count <= 0)
            {
                Debug.LogError("該当するGMLファイルがありません。");
                return;
            }
            var rootTrans = new GameObject(destFolderName).transform;

            try
            {
                await Task.WhenAll(targetGmls.Select(async gmlInfo =>
                {
                    string gmlName = Path.GetFileName(gmlInfo.Path);
                    progressDisplay.SetProgress(gmlName, 0f / 3f, "インポート処理中");
                    collection.Fetch(destPath, gmlInfo);
                    progressDisplay.SetProgress(gmlName, 1f / 3f, "ロード中");
                    await LoadAndPlaceGmlAsync(gmlInfo, rootTrans, loader.CityLoadConfig, CalcCenterPoint(collection, loader.CoordinateZoneID));
                }));
            }
            catch (AggregateException ae)
            {
                foreach(var e in ae.InnerExceptions)
                {
                    Debug.LogError(e);
                }
            }

            foreach(var gmlInfo in targetGmls) gmlInfo.Dispose();

        }

        private static PlateauVector3d CalcCenterPoint(UdxFileCollection collection, int coordinateZoneID)
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            return collection.CalcCenterPoint(geoReference);
        }

        private static async Task LoadAndPlaceGmlAsync(GmlFileInfo gmlInfo, Transform rootTrans, CityLoadConfig config, PlateauVector3d referencePoint)
        {
            string gmlPath = gmlInfo.Path;
            var packageConf = config.GetConfigForPackage(gmlInfo.Package);
            var gmlTrans = new GameObject(Path.GetFileName(gmlPath)).transform;
            gmlTrans.parent = rootTrans;
            using var cityModel = await Task.Run(() => ParseGML(gmlPath));
            if (cityModel == null)
            {
                // TODO GMLのパースに失敗したと分かるメッセージを何か出す
                return;
            }
            var meshExtractOptions = new MeshExtractOptions(
                // TODO gridCountOfSide, Extent はユーザーが設定できるようにしたほうが良い
                referencePoint,
                CoordinateSystem.EUN,
                packageConf.meshGranularity,
                packageConf.maxLOD,
                packageConf.minLOD,
                packageConf.includeTexture,
                5,
                1.0f,
                config.CoordinateZoneID,
                new Extent(new GeoCoordinate(-90, -180, -9999), new GeoCoordinate(90, 180, 9999))
            );

            if (!meshExtractOptions.Validate(out var failureMessage))
            {
                // TODO これはメインスレッドでないと動かない
                Debug.LogError($"メッシュ抽出設定に不正な点があります。 理由 : {failureMessage}"); 
                return;
            }
            
            await PlateauToUnityModelConverter.ConvertAndPlaceToScene(
                cityModel, meshExtractOptions, gmlPath, gmlTrans
            );
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
