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

        private readonly object cityModelLock = new object();

        /// <summary>
        /// TileImporterの初期化を行います。
        /// </summary>
        public void Initialize()
        {
            cityModels = new();
            cityModelGml = new();
        }

        /// <summary>
        /// CityModelを全て破棄します。
        /// </summary>
        public void Dispose()
        {
            lock (cityModelLock)
            {
                foreach (var cityModel in cityModelGml.Keys)
                {
                    cityModel.Dispose();
                }
                cityModels.Clear();
                cityModelGml.Clear();
            }
        }

        /// <summary>
        /// 都市モデルをインポートしてタイル用のメッシュ、GameObjectを生成します。
        /// </summary>
        /// <param name="fetchedGmlFiles"></param>
        /// <param name="conf"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task Import(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay,
            CancellationToken? token)
        {
            Initialize();

            try
            {
                token?.ThrowIfCancellationRequested();

                await ImportGmlParallel(fetchedGmlFiles, conf, rootTrans, progressDisplay, 10f, 40f, token); // GML読込

                await ImportTiles(conf, 11, rootTrans, progressDisplay, 40f, 60f, token);

                await ImportTiles(conf, 10, rootTrans, progressDisplay, 60f, 80f, token);

                await ImportTiles(conf, 9, rootTrans, progressDisplay, 80f, 100f, token);

            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Debug.LogError($"都市モデルのインポート中にエラーが発生しました: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// GMLファイルをインポートします。
        /// </summary>
        /// <param name="fetchedGmlFiles"></param>
        /// <param name="conf"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task ImportGml(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay, float startProgess, float endProgress,
            CancellationToken? token)
        {
            foreach (var fetchedGmlFile in fetchedGmlFiles)
            {
                token?.ThrowIfCancellationRequested();
                await ImportGmlInner(fetchedGmlFile, conf, rootTrans, progressDisplay, startProgess, endProgress, token);
            }

            Debug.Log($"GMLファイルのロードが完了しました。{cityModels.Count} 個のパッケージが見つかりました。");
        }

        /// <summary>
        /// GMLファイルを並列処理でインポートします。
        /// </summary>
        /// <param name="fetchedGmlFiles"></param>
        /// <param name="conf"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task ImportGmlParallel(List<GmlFile> fetchedGmlFiles, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay, float startProgess, float endProgress,
            CancellationToken? token)
        {
            // GMLファイルを同時に処理する最大数です。
            // 並列数が 4 くらいだと、1つずつ処理するよりも、全部同時に処理するよりも速いという経験則です。
            // ただしメモリ使用量が増えます。
            var semGmlProcess = new SemaphoreSlim(4);
            async Task ProcessGmlAsync(GmlFile fetchedGml)
            {
                if (fetchedGml == null || string.IsNullOrEmpty(fetchedGml.Path))
                    return;

                await semGmlProcess.WaitAsync();
                try
                {
                    await ImportGmlInner(fetchedGml, conf, rootTrans, progressDisplay, startProgess, endProgress, token);
                }
                catch (OperationCanceledException)
                {
                    progressDisplay.SetProgress(Path.GetFileName(fetchedGml.Path), 0f, "キャンセルされました");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    semGmlProcess.Release();
                }
            }
            await Task.WhenAll(fetchedGmlFiles.Select(ProcessGmlAsync));

            Debug.Log($"GMLファイルのロードが完了しました。{cityModels.Count} 個のパッケージが見つかりました。");
        }

        /// <summary>
        /// GMLファイルを1つインポートします。
        /// </summary>
        /// <param name="fetchedGmlFile"></param>
        /// <param name="conf"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task ImportGmlInner(GmlFile fetchedGmlFile, CityImportConfig conf,
            Transform rootTrans, IProgressDisplay progressDisplay, float startProgess, float endProgress,
            CancellationToken? token)
        {
            token?.ThrowIfCancellationRequested();

            string gmlName = Path.GetFileName(fetchedGmlFile.Path);

            var cityModel = await GmlImporter.LoadGmlAsync(fetchedGmlFile, token, progressDisplay, gmlName, startProgess);

            if (cityModel != null)
            {
                lock (cityModelLock)
                {
                    if (cityModels.ContainsKey((fetchedGmlFile.Package, fetchedGmlFile.Epsg)))
                        cityModels[(fetchedGmlFile.Package, fetchedGmlFile.Epsg)].Add(cityModel);
                    else
                        cityModels[(fetchedGmlFile.Package, fetchedGmlFile.Epsg)] = new List<CityModel> { cityModel };

                    cityModelGml.Add(cityModel, fetchedGmlFile);
                }

                progressDisplay.SetProgress(gmlName, endProgress, "GMLファイルのロードが完了しました。");
            }
        }

        /// <summary>
        /// タイルのインポート処理です。
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task ImportTiles(CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay, float startProgress, float endProgress,
            CancellationToken? token)
        {
            if (zoomLevel <= 9)
            {
                await ImportCombinedTiles(conf, zoomLevel, rootTrans, progressDisplay, startProgress, endProgress, token);
            }
            else
            {
                await ImportEachTiles(conf, zoomLevel, rootTrans, progressDisplay, startProgress, endProgress, token);
            }
        }

        /// <summary>
        /// メッシュコード単位のタイル、又はタイル分割用のインポート処理です。
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task ImportEachTiles(CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay, float startProgress, float endProgress,
            CancellationToken? token)
        {
            foreach (var cityModel in cityModels.Values.SelectMany(models => models))
            {
                token?.ThrowIfCancellationRequested();

                var gml = cityModelGml[cityModel];
                var gmlName = Path.GetFileName(gml.Path);

                // GameObject名：tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
                var gameObjectName = $"tile_zoom_{zoomLevel}_grid_{gmlName.TrimEnd('.','g','m','l')}" ;
                //var gmlTrans = GmlImporter.CreateGmlGameObject(gml).transform;
                var gmlTrans = new GameObject(gameObjectName).transform;

                if (!TryCreateMeshExtractOptions(gmlTrans, rootTrans, conf, gml, progressDisplay, gmlName, zoomLevel,
                        out var meshExtractOptions))
                {
                    return;
                }

                var packageConf = conf.GetConfigForPackage(gml.Package);
                var infoForToolkits = new CityObjectGroupInfoForToolkits(packageConf.EnableTexturePacking, false);
                // ここはメインスレッドで呼ぶ必要があります。
                var placingResult = await CityModelToScene(
                    new List<CityModel>() { cityModel }, meshExtractOptions, conf.AreaGridCodes, gmlTrans, progressDisplay, gmlName,
                    packageConf.DoSetMeshCollider, packageConf.DoSetAttrInfo, token, packageConf.FallbackMaterial,
                    infoForToolkits, packageConf.MeshGranularity, zoomLevel, startProgress, endProgress
                );

                if (placingResult.IsSucceed)
                {
                    progressDisplay.SetProgress(gmlName, endProgress, $"ズームレベル:{zoomLevel} 完了");
                }
                else
                {
                    progressDisplay.SetProgress(gmlName, 0f, "失敗 : モデルの変換または配置に失敗しました。");
                }

                if(zoomLevel > 10)
                {
                    // Grid GameObject名変更
                    // GameObject名：tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
                    for ( int i = 0; i <  gmlTrans.childCount; i++)
                    {
                        var child = gmlTrans.GetChild(i);
                        if(child?.name.StartsWith("GRID") ?? false)
                        {
                            var gridNum = child.name.TrimStart('G', 'R', 'I', 'D');
                            var splittedGml = gmlName.Split('_').ToList();
                            splittedGml[0] = $"{splittedGml.First()}{gridNum}"; // GML名にグリッド番号を追加
                            child.name = $"tile_zoom_{zoomLevel}_grid_{splittedGml.ToArray().Join2String("_").TrimEnd('.','g','m','l')}" ;
                        }
                    }
                }

                // TODO : Prefab生成して Addressablesに登録する処理を追加する
                // gmlTrans をそのままPrefab化する？
            }
        }

        /// <summary>
        /// タイル結合用Import
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="rootTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task ImportCombinedTiles(CityImportConfig conf, int zoomLevel,
            Transform rootTrans, IProgressDisplay progressDisplay, float startProgress, float endProgress,
            CancellationToken? token)
        {
            foreach (var kv in cityModels)
            {
                token?.ThrowIfCancellationRequested();
                
                var (package, epsg) = kv.Key;
                var cityModels = kv.Value;

                if (cityModels.Count == 0)
                {
                    Debug.LogWarning($"パッケージ: {package}, EPSG: {epsg} のモデルが見つかりません。");
                    continue;
                }

                Debug.Log($"パッケージ: {package}, EPSG: {epsg} のモデルを配置します。");

                var packageConf = conf.GetConfigForPackage(package);
                var infoForToolkits = new CityObjectGroupInfoForToolkits(packageConf.EnableTexturePacking, false);

                var tileGroups = GetCityModelsForEachTile(cityModels, zoomLevel); // ズームレベルに応じた結合タイル数ごとにまとめる

                foreach (var cityModelsInTile in tileGroups)
                {
                    token?.ThrowIfCancellationRequested();

                    var firstGml = cityModelGml[cityModelsInTile.FirstOrDefault()]; // epsg判定、gml名取得用
                    var firstGmlName = firstGml != null ? Path.GetFileName(firstGml.Path) : package.ToString();

                    // GameObject名：tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
                    var gameObjectName = $"tile_zoom_{zoomLevel}_grid_{firstGmlName.TrimEnd('.', 'g', 'm', 'l')}";
                    var gmlTrans = new GameObject(gameObjectName).transform;

                    if (!TryCreateMeshExtractOptions(gmlTrans, rootTrans, conf, firstGml, progressDisplay, firstGmlName, zoomLevel,
                        out var meshExtractOptions))
                    {
                        return;
                    }

                    // ここはメインスレッドで呼ぶ必要があります。
                    var placingResult = await CityModelToScene(
                        cityModelsInTile, meshExtractOptions, conf.AreaGridCodes, gmlTrans, progressDisplay, firstGmlName,
                        packageConf.DoSetMeshCollider, packageConf.DoSetAttrInfo, token, packageConf.FallbackMaterial,
                        infoForToolkits, packageConf.MeshGranularity, zoomLevel, startProgress, endProgress
                    );

                    foreach (var cityModel in cityModels)
                    {
                        var gml = cityModelGml[cityModel];
                        string gmlName = Path.GetFileName(gml.Path);
                        if (placingResult.IsSucceed)
                        {
                            progressDisplay.SetProgress(gmlName, endProgress, endProgress >= 100f ? "完了" : $"ズームレベル:{zoomLevel} 完了");
                        }
                        else
                        {
                            progressDisplay.SetProgress(gmlName, 0f, "失敗 : モデルの変換または配置に失敗しました。");
                        }
                    }

                    // TODO : Prefab生成して Addressablesに登録する処理を追加する
                    // gmlTrans をそのままPrefab化する？

                }
            }
        }

        /// <summary>
        /// ズームレベルを考慮して、タイルごとにCityModelのリストを取得します。
        /// zoomLevel:9なら隣接する4枚のCityModelをまとめて1つのリストにします。
        /// </summary>
        /// <param name="cityModels"></param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        private List<List<CityModel>> GetCityModelsForEachTile(List<CityModel> cityModels, int zoomLevel)
        {
            List<List<CityModel>> tileGroup = new List<List<CityModel>>();

            Dictionary<string, List<(int, CityModel)>> CodeDict = new(); //key:３次メッシュコード value: List (グループIndex , CityModel)

            // 全て結合する場合は zoomLevel 0
            if (zoomLevel == 0) 
            {
                tileGroup.Add(cityModels);
                return tileGroup;
            }

            var groups = new List<List<(int x, int y)>>();
            if (zoomLevel == 9) // 隣接する 4Gridcode分結合
            {
                // ３次メッシュ内の分割地域メッシュコードを４グリッドずつ結合したグループを作成
                for (int y = 0; y <= 8; y += 2) // 1刻みで2段分 → 2刻み
                {
                    for (int x = 0; x <= 8; x += 2) // 2列分をまとめて
                    {
                        var group = new List<(int x, int y)>
                        {
                            (x, y),
                            (x + 1, y),
                            (x, y + 1),
                            (x + 1, y + 1)
                        };
                        groups.Add(group);
                    }
                }
            }

            foreach (var cityModel in cityModels)
            {
                if (cityModel == null)
                {
                    Debug.LogWarning("CityModelがnullです。");
                    continue;
                }

                var gml = cityModelGml[cityModel];
                var gridCode = gml.GridCode.StringCode;
                var tertiaryMesh = gridCode.Substring(0, gridCode.Length - 2); // ３次メッシュ
                var subdividedGridSquareCode = gridCode.Substring(gridCode.Length - 2, 2); // 分割地域メッシュコード
                var x = subdividedGridSquareCode.Substring(0, 1); // 分割地域メッシュコード 1桁目
                var y = subdividedGridSquareCode.Substring(1, 1); // 分割地域メッシュコード 2桁目
                var groupIndex = groups.FindIndex(g => g.Any(c => c.x.ToString() == x && c.y.ToString() == y)); // ↑で作成したグループの該当するIndexを取得

                Debug.Log($"CityModelのGridCode: {gridCode} {x} {y} {groupIndex}");

                if (CodeDict.ContainsKey(tertiaryMesh))
                {
                    CodeDict[tertiaryMesh].Add((groupIndex, cityModel));
                }
                else
                {
                    CodeDict[tertiaryMesh] = new List<(int, CityModel)> { (groupIndex, cityModel) };
                }
            }

            foreach (var code in CodeDict)
            {
                Dictionary<int, List<CityModel>> tileGroupDict = new(); // グループIndexごとにCityModelをまとめる

                code.Value.ForEach(x =>
                {
                    if (!tileGroupDict.ContainsKey(x.Item1))
                    {
                        tileGroupDict[x.Item1] = new List<CityModel>();
                    }
                    tileGroupDict[x.Item1].Add(x.Item2);
                });

                foreach (var tileGroupEntry in tileGroupDict)
                {
                    //グループIndexごとにCityModelのリストを作成
                    if (tileGroupEntry.Value.Count > 0)
                    {
                        tileGroup.Add(tileGroupEntry.Value);
                    }
                }
            }

            return tileGroup;
        }

        /// <summary>
        /// タイル結合時のシーン配置
        /// </summary>
        /// <param name="cityModels"></param>
        /// <param name="meshExtractOptions"></param>
        /// <param name="selectedGridCodes"></param>
        /// <param name="parentTrans"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="progressName"></param>
        /// <param name="doSetMeshCollider"></param>
        /// <param name="doSetAttrInfo"></param>
        /// <param name="token"></param>
        /// <param name="fallbackMaterial"></param>
        /// <param name="infoForToolkits"></param>
        /// <param name="granularity"></param>
        /// <returns></returns>
        public async Task<GranularityConvertResult> CityModelToScene(
            List<CityModel> cityModels, MeshExtractOptions meshExtractOptions, GridCodeList selectedGridCodes,
            Transform parentTrans, IProgressDisplay progressDisplay, string progressName,
            bool doSetMeshCollider, bool doSetAttrInfo, CancellationToken? token, UnityEngine.Material fallbackMaterial,
            CityObjectGroupInfoForToolkits infoForToolkits, MeshGranularity granularity, 
            int zoomLevel, float startProgress, float endProgress
        )
        {
            Debug.Log($"load started");

            token?.ThrowIfCancellationRequested();

            AttributeDataHelper attributeDataHelper =
                new AttributeDataHelper(new SerializedCityObjectGetterFromCityModels(cityModels), doSetAttrInfo);

            Model plateauModel;
            try
            {
                plateauModel = await Task.Run(() => ExtractMeshes(cityModels, meshExtractOptions, selectedGridCodes, zoomLevel));
            }
            catch (Exception e)
            {
                Debug.LogError("メッシュデータの抽出に失敗しました。\n" + e);
                return GranularityConvertResult.Fail();
            }

            var materialConverter = new DllSubMeshToUnityMaterialByTextureMaterial();

            // TODO : Prefab生成して Addressablesに登録する処理を追加する ? 
            //　ここで追加するか不明？

            var placeToSceneConf = new PlaceToSceneConfig(materialConverter, doSetMeshCollider, token, fallbackMaterial,
                infoForToolkits, granularity);
            return await PlateauToUnityModelConverter.PlateauModelToScene(
                parentTrans, progressDisplay, progressName, placeToSceneConf,
                plateauModel, attributeDataHelper, true, startProgress, endProgress);
        }

        /// <summary>
        /// タイル結合時のメッシュ抽出処理です。
        /// </summary>
        /// <param name="cityModels"></param>
        /// <param name="meshExtractOptions"></param>
        /// <param name="selectedGridCodes"></param>
        /// <returns></returns>
        private Model ExtractMeshes(
            List<CityModel> cityModels, MeshExtractOptions meshExtractOptions, GridCodeList selectedGridCodes, int zoomLevel)
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

            if(zoomLevel <= 9)
                TileExtractor.ExtractWithCombine(ref model, cityModels, meshExtractOptions, extents); //結合
            else
                TileExtractor.ExtractWithGrid(ref model, cityModels.FirstOrDefault(), meshExtractOptions, extents);　//分割、又はエリア単位

                Debug.Log("model extracted.");
            return model;
        }

        /// <summary>
        /// ズームレベルを考慮したタイル用のMeshExtractOptionsを取得
        /// </summary>
        /// <param name="gmlTrans"></param>
        /// <param name="rootTrans"></param>
        /// <param name="conf"></param>
        /// <param name="fetchedGmlFile"></param>
        /// <param name="progressDisplay"></param>
        /// <param name="gmlName"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryCreateMeshExtractOptions(Transform gmlTrans, Transform rootTrans, CityImportConfig conf, GmlFile fetchedGmlFile, IProgressDisplay progressDisplay, string gmlName, int zoomLevel, out MeshExtractOptions result)
        {
            MeshExtractOptions meshExtractOptions;
            bool success = false;
            try
            {
                // TODO : ここの設定はPrefab生成時は不要？
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
                meshExtractOptions.highestLodOnly = true; // 高精細メッシュのみを抽出
            }
            else if (zoomLevel == 10)
            {
                meshExtractOptions.MeshGranularity = MeshGranularity.PerCityModelArea;
                meshExtractOptions.GridCountOfSide = 1; // 分割しない
                meshExtractOptions.highestLodOnly = true; // 高精細メッシュのみを抽出
            }
            else if (zoomLevel == 9)
            {
                meshExtractOptions.MeshGranularity = MeshGranularity.PerCityModelArea;
                meshExtractOptions.GridCountOfSide = 1;
                meshExtractOptions.highestLodOnly = true; // 高精細メッシュのみを抽出
            }

            result = meshExtractOptions;
            return success;
        }
    }
}
