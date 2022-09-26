using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityLoader.Setting;
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
        public static void Import(PLATEAUCityLoaderBehaviour loader)
        {
            // コピー
            string destPath = CityFilesCopy.ToStreamingAssets(loader.SourcePathBeforeImport, loader.CityLoadConfig);
            loader.SourcePathAfterImport = destPath;
            // シーン配置
            var gmlPathsDict = loader.CityLoadConfig.SearchMatchingGMLList(destPath, out _);
            var task = LoadGmlsAsync(gmlPathsDict, loader.CityLoadConfig);
            task.ContinueWithErrorCatch();
        }
        
        /// <summary>
        /// 選択された GMLファイル群を非同期でロードします。
        /// </summary>
        /// <param name="gmlPathsDict">辞書であり、キーはパッケージ種、値はそのパッケージに該当するGMLファイルパスリストです。</param>
        /// <param name="config">ロード設定です。</param>
        private static async Task LoadGmlsAsync(Dictionary<PredefinedCityModelPackage, List<string>> gmlPathsDict, CityLoadConfig config)
        {
            // パッケージ種ごとのループです。
            foreach (var package in gmlPathsDict.Keys)
            {
                // パッケージ種ごとの設定を利用します。
                var packageConf = config.GetConfigForPackage(package);
                foreach (string gmlPath in gmlPathsDict[package])
                {
                    using var cityModel = await Task.Run(() => ParseGML(gmlPath));
                    if (cityModel == null) continue;
                    var meshExtractOptions = new MeshExtractOptions(
                        // TODO ReferencePoint, gridCountOfSide, Extent はユーザーが設定できるようにしたほうが良い
                        cityModel.CenterPoint,
                        CoordinateSystem.EUN,
                        packageConf.meshGranularity,
                        packageConf.maxLOD,
                        packageConf.minLOD,
                        packageConf.includeTexture,
                        5,
                        1.0f,
                        new Extent(new GeoCoordinate(-90, -180, -9999), new GeoCoordinate(90, 180, 9999))
                    );
                    
                    if (!meshExtractOptions.Validate()) continue;

                    await PlateauToUnityModelConverter.ConvertAndPlaceToScene(
                        cityModel,
                        meshExtractOptions,
                        gmlPath
                    );
                }
            }
        }
        
        /// <summary> gmlファイルをパースします。 </summary>
        /// <param name="gmlAbsolutePath"> gmlファイルのパスです。 </param>
        /// <returns><see cref="CityGML.CityModel"/> を返します。パスにファイルがなければ null を返します。</returns>
        private static CityModel ParseGML(string gmlAbsolutePath)
        {
            if (!File.Exists(gmlAbsolutePath))
            {
                return null;
            }
            var parserParams = new CitygmlParserParams(true, true, false);
            return CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
        }
    }
}
