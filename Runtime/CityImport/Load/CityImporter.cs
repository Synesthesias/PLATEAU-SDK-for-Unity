using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityAdjust;
using PLATEAU.CityImport.Load.CityImportProcedure;
using PLATEAU.CityImport.Load.FileCopy;
using PLATEAU.CityImport.Setting;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Dataset;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityImport.Load
{
    /// <summary>
    /// GMLファイルに記載された都市モデルを Unity にインポートします。
    /// </summary>
    internal static class CityImporter
    {
        /// <summary>
        /// <see cref="CityImporter"/> クラスのメインメソッドです。
        /// GMLファイルから都市モデルを読み、そのメッシュをUnity向けに変換してシーンに配置します。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        public static async Task ImportAsync(CityLoadConfig config, IProgressDisplay progressDisplay)
        {
            var datasetSourceConfig = config.DatasetSourceConfig;
            string destPath = PathUtil.PLATEAUSrcFetchDir;
            string destFolderName = datasetSourceConfig.RootDirName;

            if ((!datasetSourceConfig.IsServer) && (!Directory.Exists(datasetSourceConfig.DatasetIdOrSourcePath)))
            {
                Debug.LogError($"インポート元パスが存在しません。 sourcePath = {datasetSourceConfig.DatasetIdOrSourcePath}");
                return;
            }
            
            progressDisplay.SetProgress("GMLファイル検索", 10f, "");
            using var datasetSource = DatasetSource.Create(datasetSourceConfig);
            using var datasetAccessor = datasetSource.Accessor;
            var targetGmls = await Task.Run(() => TargetGmlFinder.FindTargetGmls(
                datasetAccessor, config
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
            var referencePoint = CalcCenterPoint(targetGmls, config.CoordinateZoneID);
            
            // ルートのGameObjectにコンポーネントを付けます。 
            var cityModelComponent = rootTrans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
            cityModelComponent.GeoReference =
                new GeoReference(referencePoint, GmlImporter.UnitScale, GmlImporter.MeshAxes, config.CoordinateZoneID);
            
            // GMLファイルを同時に処理する最大数です。
            // 並列数が 4 くらいだと、1つずつ処理するよりも、全部同時に処理するよりも速いという経験則です。
            var sem = new SemaphoreSlim(4);
            
            await Task.WhenAll(targetGmls.Select(async gmlInfo =>
            {
                await sem.WaitAsync(); 
                try
                {
                    // ここはメインスレッドで呼ぶ必要があります。
                    await GmlImporter.Import(gmlInfo, destPath, config, rootTrans, progressDisplay, referencePoint);
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
            
            // インポート完了後の処理
            CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(cityModelComponent);
        }

        private static PlateauVector3d CalcCenterPoint(IEnumerable<GmlFile> targetGmls, int coordinateZoneID)
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            var geoCoordSum = new GeoCoordinate(0, 0, 0);
            int count = 0;
            foreach (var gml in targetGmls)
            {
                geoCoordSum += gml.MeshCode.Extent.Center;
                count++;
            }

            if (count == 0) throw new ArgumentException("Target gmls count is zero.");
            var centerGeo = geoCoordSum / count;
            return geoReference.Project(centerGeo);
        }

        
    }
}
