using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using PLATEAU.CityImport.Import.CityImportProcedure;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;

namespace PLATEAU.CityImport.Import.Tile
{
    internal static class TileImporter
    {
        internal static async Task Import(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {

            await ImportOneArea(fetchedGmlFiles, conf, 11, rootTrans, progressDisplay, token);

            await ImportOneArea(fetchedGmlFiles, conf, 10, rootTrans, progressDisplay, token);

            await ImportMulti(fetchedGmlFiles, conf, 9, rootTrans, progressDisplay, token);
        }

        internal static async Task ImportOneArea(List<GmlFile> fetchedGmlFiles, CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            conf.packageImportConfigConverter = new TilePackageImportConfigConverter(zoomLevel);

            // GMLファイルを同時に処理する最大数です。
            // 並列数が 4 くらいだと、1つずつ処理するよりも、全部同時に処理するよりも速いという経験則です。
            // ただしメモリ使用量が増えます。
            var semGmlProcess = new SemaphoreSlim(4);
            await Task.WhenAll(fetchedGmlFiles.Select(async fetchedGml =>
            {
                await semGmlProcess.WaitAsync();
                try
                {
                    if (fetchedGml != null && !string.IsNullOrEmpty(fetchedGml.Path))
                    {
                        try
                        {
                            // GMLを1つインポートします。
                            // ここはメインスレッドで呼ぶ必要があります。
                            await GmlImporter.Import(fetchedGml, conf, rootTrans, progressDisplay, token);
                        }
                        catch (OperationCanceledException)
                        {
                            progressDisplay.SetProgress(Path.GetFileName(fetchedGml.Path), 0f, "キャンセルされました");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    semGmlProcess.Release();
                }

            }));
        }

        /// <summary>
        /// 複数のGMLファイルをインポートして結合します。
        /// </summary>
        /// <param name="fetchedGmlFiles"></param>
        /// <param name="conf"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal static async Task ImportMulti(List<GmlFile> fetchedGmlFiles, CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            token?.ThrowIfCancellationRequested();

            conf.packageImportConfigConverter = new TilePackageImportConfigConverter(zoomLevel);

            //List<CityModel> cityModels = new List<CityModel>();
            var cityModels = new Dictionary<(PredefinedCityModelPackage, int), List<CityModel>>();
            var cityModelGml = new Dictionary<CityModel, GmlFile>();

            foreach (var fetchedGmlFile in fetchedGmlFiles)
            {
                token?.ThrowIfCancellationRequested();

                if (fetchedGmlFile.Path == null) continue;

                string gmlName = Path.GetFileName(fetchedGmlFile.Path);

                var cityModel = await GmlImporter.LoadGmlAsync(fetchedGmlFile, token, progressDisplay, gmlName);

                if (cityModel != null)
                {
                    //cityModels.Add(cityModel);

                    if (cityModels.ContainsKey((fetchedGmlFile.Package, fetchedGmlFile.Epsg)))
                        cityModels[(fetchedGmlFile.Package, fetchedGmlFile.Epsg)].Add(cityModel);
                    else
                        cityModels[(fetchedGmlFile.Package, fetchedGmlFile.Epsg)] = new List<CityModel> { cityModel };

                    cityModelGml.Add(cityModel, fetchedGmlFile);

                    progressDisplay.SetProgress(gmlName, 100f, "GMLファイルのロードが完了しました。");
                }
            }

            Debug.Log($"GMLファイルのロードが完了しました。{cityModels.Count} 個のパッケージが見つかりました。");

            foreach (var (package, epsg) in cityModels.Keys)
            {
                token?.ThrowIfCancellationRequested();

                Debug.Log($"パッケージ: {package}, EPSG: {epsg} のモデルを配置します。");

                var citymodels = cityModels[(package, epsg)];
                var firstGml = cityModelGml[citymodels.FirstOrDefault()]; // epsg判定、gml名取得用
                var firstGmlName = firstGml != null ? Path.GetFileName(firstGml.Path) : package.ToString();

                var gmlTrans = new GameObject(firstGmlName).transform;

                var meshExtractOptions = conf.CreateNativeConfigFor(package, firstGml);

                var packageConf = conf.GetConfigForPackage(package);
                var infoForToolkits = new CityObjectGroupInfoForToolkits(packageConf.EnableTexturePacking, false);

                // ここはメインスレッドで呼ぶ必要があります。
                var placingResult = await CityModelToScene(
                    citymodels, meshExtractOptions, conf.AreaGridCodes, gmlTrans, progressDisplay, package.ToString(),
                    packageConf.DoSetMeshCollider, packageConf.DoSetAttrInfo, token, packageConf.FallbackMaterial,
                    infoForToolkits, packageConf.MeshGranularity
                );

                foreach (var cityModel in citymodels)
                {
                    var gml = cityModelGml[cityModel];
                    string gmlName = Path.GetFileName(gml.Path);
                    if (placingResult.IsSucceed)
                    {
                        progressDisplay.SetProgress(gmlName, 100f, "完了");
                    }
                    else
                    {
                        progressDisplay.SetProgress(gmlName, 0f, "失敗 : モデルの変換または配置に失敗しました。");
                    }

                    cityModel.Dispose();
                }

                progressDisplay.SetProgress(package.ToString(), 100f, "完了");
            }
        }

        public static async Task<GranularityConvertResult> CityModelToScene(
            List<CityModel> cityModels, MeshExtractOptions meshExtractOptions, GridCodeList selectedGridCodes,
            Transform parentTrans, IProgressDisplay progressDisplay, string progressName,
            bool doSetMeshCollider, bool doSetAttrInfo, CancellationToken? token, UnityEngine.Material fallbackMaterial,
            CityObjectGroupInfoForToolkits infoForToolkits, MeshGranularity granularity
        )
        {
            Debug.Log($"load started");

            var cityModel = cityModels.First();

            // TODO: 分割された属性情報を結合する処理が必要

            token?.ThrowIfCancellationRequested();
            AttributeDataHelper attributeDataHelper =
                new AttributeDataHelper(new SerializedCityObjectGetterFromCityModel(cityModel), doSetAttrInfo);

            Model plateauModel;
            try
            {
                plateauModel = await Task.Run(() => ExtractMeshes(cityModels, meshExtractOptions, selectedGridCodes));
            }
            catch (Exception e)
            {
                Debug.LogError("メッシュデータの抽出に失敗しました。\n" + e);
                return GranularityConvertResult.Fail();
            }

            var materialConverter = new DllSubMeshToUnityMaterialByTextureMaterial();
            var placeToSceneConf = new PlaceToSceneConfig(materialConverter, doSetMeshCollider, token, fallbackMaterial,
                infoForToolkits, granularity);

            return await PlateauToUnityModelConverter.PlateauModelToScene(
                parentTrans, progressDisplay, progressName, placeToSceneConf,
                plateauModel, attributeDataHelper, true);
        }

        private static Model ExtractMeshes(
            List<CityModel> cityModels, MeshExtractOptions meshExtractOptions, GridCodeList selectedGridCodes)
        {
            var model = Model.Create();
            if (cityModels.Count == 0) return model;
            var extents = selectedGridCodes.GridCodes.Select(code => {
                var extent = code.Extent;
                extent.Min.Height = -999999.0;
                extent.Max.Height = 999999.0;
                code.Dispose(); // 廃棄を明示
                return extent;
            }).ToList();

            meshExtractOptions.MeshGranularity = MeshGranularity.PerCityModelArea; //
            meshExtractOptions.GridCountOfSide = 1;
            TileExtractor.ExtractInExtents(ref model, cityModels, meshExtractOptions, extents);

            Debug.Log("model extracted.");
            return model;
        }
    }

    /// <summary>
    /// ZoomLevelに応じてTileインポート時の設定を変更するためのコンバーターです。
    /// </summary>
    public class TilePackageImportConfigConverter: IPackageImportConfigConverter
    {
        public int TileZoomLevel { get; private set; } = 0;

        public TilePackageImportConfigConverter(int tileZoomLevel)
        {
            TileZoomLevel = tileZoomLevel;
        }

        public void Convert(ref MeshExtractOptions options)
        {
            //Tileインポート時はZoomLevelに応じて設定を変更します。
            if (TileZoomLevel == 11)
            {
                options.MeshGranularity = MeshGranularity.PerCityModelArea;
                options.GridCountOfSide = 2; // 11の時は2x2グリッドに分割
            }
            else if (TileZoomLevel == 10)
            {
                options.MeshGranularity = MeshGranularity.PerCityModelArea;
                options.GridCountOfSide = 1; // 分割しない
            }
            else if (TileZoomLevel == 9)
            {
                options.MeshGranularity = MeshGranularity.PerCityModelArea;
                options.GridCountOfSide = 1;
            }
        }
    }
}
