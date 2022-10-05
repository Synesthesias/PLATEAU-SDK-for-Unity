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
        /// <summary>
        /// 選択されたGMLとその関連ファイルを StreamingAssetsフォルダにコピーし、都市モデルをシーンに配置します。
        /// メインスレッドから呼ぶ必要があります。
        /// </summary>
        [Obsolete]
        public static async void ImportAsync(PLATEAUCityModelLoader loader, IProgressDisplay progressDisplay)
        {
            // コピー
            string fetchDestPath = PathUtil.plateauSrcFetchDir;
            string destPath = await Task.Run(()=>CityFilesCopy.ToStreamingAssets(loader.SourcePathBeforeImport, loader.CityLoadConfig, progressDisplay, fetchDestPath));
            loader.SourcePathAfterImport = destPath;
            // シーン配置
            var gmlPathsDict = loader.CityLoadConfig.SearchMatchingGMLList(destPath, out var collection);
            if (gmlPathsDict.Count == 0)
            {
                Debug.LogError("該当するGMLファイルの数が0です。");
                return;
            }

            var rootDirName = Path.GetFileName(destPath);
            var task = LoadAndPlaceGmlsAsync(gmlPathsDict, loader.CityLoadConfig, rootDirName, progressDisplay, CalcCenterPoint(collection, loader.CoordinateZoneID));
            task.ContinueWithErrorCatch();
        }

        public static void ImportV2(PLATEAUCityModelLoader loader, IProgressDisplay progressDisplay)
        {
            string destPath = PathUtil.plateauSrcFetchDir;
            var targetGmls = CityFilesCopy.FindTargetGmls(
                loader.SourcePathBeforeImport, loader.CityLoadConfig, out var collection
            );
            if (targetGmls.Count <= 0)
            {
                Debug.LogError("該当するGMLファイルがありません。");
                return;
            }

            try
            {
                Parallel.ForEach(targetGmls, gmlInfo =>
                {
                    string gmlName = Path.GetFileName(gmlInfo.Path);
                    progressDisplay.SetProgress(gmlName, 1f / 3f, "インポート処理中");
                    collection.Fetch(destPath, gmlInfo);

                });
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
        
        /// <summary>
        /// 選択された GMLファイル群を非同期でロードし、モデルをシーンに配置します。
        /// </summary>
        /// <param name="gmlPathsDict">対象となるGMLファイルのパスです。辞書であり、キーはパッケージ種、値はそのパッケージに該当するGMLファイルパスリストです。</param>
        /// <param name="config">ロード設定です。</param>
        /// <param name="rootObjName">配置するゲームオブジェクトの中でヒエラルキーが最も上であるものの名称です。</param>
        /// <param name="progressDisplay">処理の進捗がこのディスプレイに表示されます。</param>
        /// <param name="referencePoint">変換座標の基準点です。</param>
        private static async Task LoadAndPlaceGmlsAsync(Dictionary<PredefinedCityModelPackage, List<string>> gmlPathsDict, CityLoadConfig config, string rootObjName, IProgressDisplay progressDisplay, PlateauVector3d referencePoint)
        {
            int gmlCount = gmlPathsDict.SelectMany(pair => pair.Value).Count();
            var rootTrans = new GameObject(rootObjName).transform;
            // パッケージ種ごとのループです。
            int loopCountGml = 0;
            foreach (var package in gmlPathsDict.Keys)
            {
                // パッケージ種ごとの設定を利用します。
                var packageConf = config.GetConfigForPackage(package);
                // GMLファイルごとのループです。
                foreach (string gmlPath in gmlPathsDict[package])
                {
                    progressDisplay.SetProgress("3Dモデルのロード", (float)(loopCountGml) * 100 / gmlCount, $"[{loopCountGml+1} / {gmlCount} : {Path.GetFileName(gmlPath)}]");
                    var gmlTrans = new GameObject(Path.GetFileName(gmlPath)).transform;
                    gmlTrans.parent = rootTrans;
                    using var cityModel = await Task.Run(() => ParseGML(gmlPath));
                    if (cityModel == null) continue;
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
                        Debug.LogError($"メッシュ抽出設定に不正な点があります。 理由 : {failureMessage}");
                        continue;
                    }

                    await PlateauToUnityModelConverter.ConvertAndPlaceToScene(
                        cityModel, meshExtractOptions, gmlPath, gmlTrans
                    );
                    loopCountGml++;
                } // gmlファイルごとのループ
            }// パッケージ種ごとのループ
            progressDisplay.SetProgress("3Dモデルのロード", 100f, "完了");
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
