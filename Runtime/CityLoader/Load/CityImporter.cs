﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityLoader.Load.Convert;
using PLATEAU.CityLoader.Load.FileCopy;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Geom;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityLoader.Load
{
    internal static class CityImporter
    {
        /// <summary>
        /// 選択されたGMLとその関連ファイルを StreamingAssetsフォルダにコピーし、都市モデルをシーンに配置します。
        /// </summary>
        public static void Import(PLATEAUCityLoaderBehaviour loader, IProgressDisplay progressDisplay)
        {
            // コピー
            string destPath = CityFilesCopy.ToStreamingAssets(loader.SourcePathBeforeImport, loader.CityLoadConfig);
            loader.SourcePathAfterImport = destPath;
            // シーン配置
            var gmlPathsDict = loader.CityLoadConfig.SearchMatchingGMLList(destPath, out var collection);
            if (gmlPathsDict.Count == 0)
            {
                Debug.LogError("該当するGMLファイルの数が0です。");
                return;
            }

            var rootDirName = Path.GetFileName(destPath);
            var task = LoadAndPlaceGmlsAsync(gmlPathsDict, loader.CityLoadConfig, rootDirName, progressDisplay, CalcCenterPoint(collection));
            task.ContinueWithErrorCatch();
        }

        private static PlateauVector3d CalcCenterPoint(UdxFileCollection collection)
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference();
            return collection.CalcCenterPoint(geoReference);
        }
        
        /// <summary>
        /// 選択された GMLファイル群を非同期でロードし、モデルをシーンに配置します。
        /// </summary>
        /// <param name="gmlPathsDict">対象となるGMLファイルのパスです。辞書であり、キーはパッケージ種、値はそのパッケージに該当するGMLファイルパスリストです。</param>
        /// <param name="config">ロード設定です。</param>
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
                        new Extent(new GeoCoordinate(-90, -180, -9999), new GeoCoordinate(90, 180, 9999))
                    );

                    if (!meshExtractOptions.Validate())
                    {
                        Debug.LogError("メッシュ抽出設定に不正な点があります。");
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
