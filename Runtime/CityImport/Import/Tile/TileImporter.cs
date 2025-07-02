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
    internal class TileImporter : IDisposable
    {

        /// <summary>
        /// GMLファイルから読み込んだ<see cref="CityModel"/>を格納する辞書です。
        /// Key:(package, epsg), Value: CityModelのリスト
        /// </summary>
        private Dictionary<(PredefinedCityModelPackage, int), List<CityModel>> cityModels;

        /// <summary>
        /// GMLファイルとCityModelの対応を格納する辞書です。
        /// </summary>
        private Dictionary<CityModel, GmlFile> cityModelGml;

        private readonly object cityModelsLock = new object();

        public void Dispose()
        {
            lock (cityModelsLock)
            {
                foreach (var cityModel in cityModels.Values.SelectMany(models => models))
                {
                    cityModel.Dispose();
                }
                cityModels.Clear();
                cityModelGml.Clear();
            }
        }

        internal async Task Import(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            cityModels = new();
            cityModelGml = new();

            await ImportGmlParallel(fetchedGmlFiles, conf, rootTrans, progressDisplay, token);

            await ImportOneArea(conf, 11, rootTrans, progressDisplay, token);

            await ImportOneArea(conf, 10, rootTrans, progressDisplay, token);

            // TODO: メッシュコード4枚取り出す
            await ImportMulti(conf, 9, rootTrans, progressDisplay, token);

            Dispose();
        }

        internal async Task ImportGml(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            token?.ThrowIfCancellationRequested();

            foreach (var fetchedGmlFile in fetchedGmlFiles)
            {
                token?.ThrowIfCancellationRequested();
                await ImportGmlInner(fetchedGmlFile, conf, rootTrans, progressDisplay, token);
            }

            Debug.Log($"GMLファイルのロードが完了しました。{cityModels.Count} 個のパッケージが見つかりました。");
        }

        internal async Task ImportGmlParallel(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            token?.ThrowIfCancellationRequested();

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
                            await ImportGmlInner(fetchedGml, conf,rootTrans, progressDisplay,token);
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

            Debug.Log($"GMLファイルのロードが完了しました。{cityModels.Count} 個のパッケージが見つかりました。");
        }

        internal async Task ImportGmlInner(GmlFile fetchedGmlFile, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            token?.ThrowIfCancellationRequested();

            string gmlName = Path.GetFileName(fetchedGmlFile.Path);

            var cityModel = await GmlImporter.LoadGmlAsync(fetchedGmlFile, token, progressDisplay, gmlName);

            if (cityModel != null)
            {
                lock (cityModelsLock)
                {
                    if (cityModels.ContainsKey((fetchedGmlFile.Package, fetchedGmlFile.Epsg)))
                        cityModels[(fetchedGmlFile.Package, fetchedGmlFile.Epsg)].Add(cityModel);
                    else
                        cityModels[(fetchedGmlFile.Package, fetchedGmlFile.Epsg)] = new List<CityModel> { cityModel };

                    cityModelGml.Add(cityModel, fetchedGmlFile);
                }

                progressDisplay.SetProgress(gmlName, 100f, "GMLファイルのロードが完了しました。");
            }
        }

        internal async Task ImportOneArea(CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            token?.ThrowIfCancellationRequested();

            foreach (var cityModel in cityModels.Values.SelectMany(models => models))
            {
                var gml = cityModelGml[cityModel];
                var gmlName = Path.GetFileName(gml.Path);

                var gmlTrans = GmlImporter.CreateGmlGameObject(gml).transform;

                if (!TryCreateMeshExtractOptions(gmlTrans, rootTrans, conf, gml, progressDisplay, gmlName, zoomLevel,
                        out var meshExtractOptions))
                {
                    return;
                }

                var packageConf = conf.GetConfigForPackage(gml.Package);
                var infoForToolkits = new CityObjectGroupInfoForToolkits(packageConf.EnableTexturePacking, false);
                // ここはメインスレッドで呼ぶ必要があります。
                var placingResult = await PlateauToUnityModelConverter.CityModelToScene(
                    cityModel, meshExtractOptions, conf.AreaGridCodes, gmlTrans, progressDisplay, gmlName,
                    packageConf.DoSetMeshCollider, packageConf.DoSetAttrInfo, token, packageConf.FallbackMaterial,
                    infoForToolkits, packageConf.MeshGranularity
                );

                if (placingResult.IsSucceed)
                {
                    progressDisplay.SetProgress(gmlName, 100f, "完了");
                }
                else
                {
                    progressDisplay.SetProgress(gmlName, 0f, "失敗 : モデルの変換または配置に失敗しました。");
                }
            }
        }

        internal async Task ImportMulti(CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            foreach (var (package, epsg) in cityModels.Keys)
            {
                token?.ThrowIfCancellationRequested();

                Debug.Log($"パッケージ: {package}, EPSG: {epsg} のモデルを配置します。");

                var citymodels = cityModels[(package, epsg)];
                var firstGml = cityModelGml[citymodels.FirstOrDefault()]; // epsg判定、gml名取得用
                var firstGmlName = firstGml != null ? Path.GetFileName(firstGml.Path) : package.ToString();

                var gmlTrans = new GameObject(firstGmlName).transform;

                if (!TryCreateMeshExtractOptions(gmlTrans, rootTrans, conf, firstGml, progressDisplay, firstGmlName, zoomLevel,
                    out var meshExtractOptions))
                {
                    return;
                }

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
                }

                progressDisplay.SetProgress(package.ToString(), 100f, "完了");
            }
        }

        public async Task<GranularityConvertResult> CityModelToScene(
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

        private Model ExtractMeshes(
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

        private bool TryCreateMeshExtractOptions(Transform gmlTrans, Transform rootTrans, CityImportConfig conf, GmlFile fetchedGmlFile, IProgressDisplay progressDisplay, string gmlName, int zoomLevel, out MeshExtractOptions result)
        {
            MeshExtractOptions meshExtractOptions;
            bool success = false;
            try
            {
                gmlTrans.parent = rootTrans;
                meshExtractOptions = conf.CreateNativeConfigFor(fetchedGmlFile.Package, fetchedGmlFile);
                success = true;
            }
            catch (Exception e)
            {
                progressDisplay.SetProgress(gmlName, 0f, $"失敗 : メッシュインポートの設定に失敗しました。\n{e.Message}");
                meshExtractOptions = new MeshExtractOptions();
                result = meshExtractOptions;
            }

            //Tileインポート時はZoomLevelに応じて設定を変更します。
            if (zoomLevel == 11)
            {
                meshExtractOptions.MeshGranularity = MeshGranularity.PerCityModelArea;
                meshExtractOptions.GridCountOfSide = 2; // 11の時は2x2グリッドに分割
            }
            else if (zoomLevel == 10)
            {
                meshExtractOptions.MeshGranularity = MeshGranularity.PerCityModelArea;
                meshExtractOptions.GridCountOfSide = 1; // 分割しない
            }
            else if (zoomLevel == 9)
            {
                meshExtractOptions.MeshGranularity = MeshGranularity.PerCityModelArea;
                meshExtractOptions.GridCountOfSide = 1;
            }

            result = meshExtractOptions;
            return success;
        }
    }
}
