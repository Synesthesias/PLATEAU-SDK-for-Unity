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
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Geometries;

namespace PLATEAU.CityImport.Import.Tile
{
    internal class TileImporter : IDisposable
    {

        /// <summary>
        /// <see cref="CityImporter"/> クラスのメインメソッドです。
        /// GMLファイルから都市モデルを読み、そのメッシュをUnity向けに変換してシーンに配置します。
        /// メインスレッドで呼ぶ必要があります。
        /// GMLを1つ読み込んだあとにしたい処理を<paramref name="postGmlProcessors"/>に渡します。
        /// </summary>
        public static async Task ImportAsync(CityImportConfig config, IProgressDisplay progressDisplay,
            CancellationToken? token, IEnumerable<IPostGmlImportProcessor> postGmlProcessors = null)
        {
            if (config == null)
            {
                Debug.LogError("CityImportConfig が null です。");
                return;
            }

            progressDisplay ??= new DummyProgressDisplay();
            var datasetSourceConfig = config.ConfBeforeAreaSelect.DatasetSourceConfig;


            if ((datasetSourceConfig is DatasetSourceConfigLocal localConf) && (!Directory.Exists(localConf.LocalSourcePath)))
            {
                Debug.LogError($"インポート元パスが存在しません。 sourcePath = {localConf.LocalSourcePath}");
                return;
            }

            progressDisplay.SetProgress("GMLファイル検索", 10f, "");
            List<GmlFile> targetGmls = null;
            try
            {
                targetGmls = await Task.Run(() => config.SearchMatchingGMLList(token));
            }
            catch (OperationCanceledException)
            {
                progressDisplay.SetProgress("GMLファイル検索", 0f, "キャンセルされました");
            }
            catch (Exception)
            {
                progressDisplay.SetProgress("GMLファイル検索", 0f, "失敗 : GMLファイルを検索できませんでした。");
                throw;
            }

            progressDisplay.SetProgress("GMLファイル検索", 100f, "完了");

            if (targetGmls == null || targetGmls.Count <= 0)
            {
                Debug.LogError("該当するGMLファイルがありません。");
                return;
            }

            foreach (var gml in targetGmls)
            {
                progressDisplay.SetProgress(Path.GetFileName(gml.Path), 0f, "未処理");
            }

            // 都市ゲームオブジェクト階層のルートを生成します。
            // ここで指定するゲームオブジェクト名は仮であり、あとからインポートしたGMLファイルパスに応じてふさわしいものに変更します。
            var rootTrans = new GameObject("インポート中です...").transform;

            // 基準点を設定します。基準点はどのGMLファイルでも共通です。（そうでないと複数のGMLファイル間で位置が合わないため。）
            var referencePoint = config.ReferencePoint;

            // ルートのGameObjectにコンポーネントを付けます。 
            var cityModelComponent = rootTrans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
            cityModelComponent.GeoReference =
                GeoReference.Create(referencePoint, PackageImportConfig.UnitScale, PackageImportConfig.MeshAxes, config.ConfBeforeAreaSelect.CoordinateZoneID);

            // ローカルインポートの場合は Fetch（ファイルコピー）を省略し、直接 GML ファイルパスを使用します。
            // リモートインポートの場合はFetch（ダウンロード） します。

            // リモートインポートの場合は一時フォルダを作成
            bool isRemoteImport = datasetSourceConfig is DatasetSourceConfigRemote;
            string remoteDownloadPath = datasetSourceConfig is DatasetSourceConfigRemote ? PathUtil.GetTempImportDir() : "";
            if (isRemoteImport && !Directory.Exists(remoteDownloadPath))
            {
                Directory.CreateDirectory(remoteDownloadPath);
            }

            bool isLocalImport = datasetSourceConfig is DatasetSourceConfigLocal;

            try
            {
                var fetchedGmls = await CityImporter.Fetch(targetGmls, isLocalImport, remoteDownloadPath, config, progressDisplay, token);

                // タイルインポート処理を行います。
                using var tileImporter = new TileImporter();
                await tileImporter.Import(fetchedGmls, config, rootTrans, progressDisplay, token, postGmlProcessors);

                // インポート完了後の処理
                string finalGmlRootPath = fetchedGmls.Last().CityRootPath();
                rootTrans.name = Path.GetFileName(finalGmlRootPath);
            }
            finally
            {
                // インポートの成否にかかわらず、リモートインポートでできた一時フォルダを削除します。
                if (isRemoteImport && !string.IsNullOrEmpty(remoteDownloadPath) && Directory.Exists(remoteDownloadPath))
                {
                    try
                    {
                        Directory.Delete(remoteDownloadPath, true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"一時フォルダの削除に失敗しました: {remoteDownloadPath}\n{e.Message}");
                    }
                }
            }
        }

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

        private IProgressDisplay progressDisplay;
        private Transform rootTransform;
        private CityImportConfig importConfig;
        private IEnumerable<IPostGmlImportProcessor> postGmlProcessors;

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
            CancellationToken? token, IEnumerable<IPostGmlImportProcessor> postGmlProcessors = null)
        {
            Initialize();

            this.progressDisplay = progressDisplay;
            this.rootTransform = rootTrans;
            this.importConfig = conf;
            this.postGmlProcessors = postGmlProcessors;

            try
            {
                token?.ThrowIfCancellationRequested();

                await ImportGmlParallel(fetchedGmlFiles, 10f, 30f, token); // GML読込

                await ImportTiles(11, 40f, 60f, token);
                await ImportTiles(10, 60f, 80f, token);
                await ImportTiles(9, 80f, 100f, token);

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
        internal async Task ImportGml(List<GmlFile> fetchedGmlFiles, float startProgess, float endProgress, CancellationToken? token)
        {
            foreach (var fetchedGmlFile in fetchedGmlFiles)
            {
                token?.ThrowIfCancellationRequested();
                await ImportGmlInner(fetchedGmlFile, startProgess, endProgress, token);
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
        internal async Task ImportGmlParallel(List<GmlFile> fetchedGmlFiles, float startProgess, float endProgress, CancellationToken? token)
        {
            // CityImporterと同様に並列数4で処理
            var semGmlProcess = new SemaphoreSlim(4);
            async Task ProcessGmlAsync(GmlFile fetchedGml)
            {
                if (fetchedGml == null || string.IsNullOrEmpty(fetchedGml.Path))
                    return;

                await semGmlProcess.WaitAsync();
                try
                {
                    await ImportGmlInner(fetchedGml, startProgess, endProgress, token);
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
        internal async Task ImportGmlInner(GmlFile fetchedGmlFile, float startProgess, float endProgress, CancellationToken? token)
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
        internal async Task ImportTiles(int zoomLevel, float startProgress, float endProgress, CancellationToken? token)
        {
            if (zoomLevel <= 9)
            {
                await ImportCombinedTiles(zoomLevel, startProgress, endProgress, token);
            }
            else
            {
                await ImportEachTiles(zoomLevel, startProgress, endProgress, token);
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
        internal async Task ImportEachTiles(int zoomLevel, float startProgress, float endProgress, CancellationToken? token)
        {
            foreach (var cityModel in cityModels.Values.SelectMany(models => models))
            {
                token?.ThrowIfCancellationRequested();

                var gml = cityModelGml[cityModel];
                var gmlName = Path.GetFileName(gml.Path);
                var gameObjectName = GetTileName(zoomLevel, gmlName);
                //var gmlTrans = GmlImporter.CreateGmlGameObject(gml).transform;
                var gmlTrans = new GameObject(gameObjectName).transform;

                if (!TryCreateMeshExtractOptions(gmlTrans, gml, gmlName, zoomLevel,
                        out var meshExtractOptions))
                {
                    return;
                }

                var packageConf = importConfig.GetConfigForPackage(gml.Package);
                var infoForToolkits = new CityObjectGroupInfoForToolkits(packageConf.EnableTexturePacking, false);
                // ここはメインスレッドで呼ぶ必要があります。
                var placingResult = await CityModelToGameObject(
                    new List<CityModel>() { cityModel }, meshExtractOptions, importConfig.AreaGridCodes, gmlTrans, gmlName,
                    packageConf.DoSetMeshCollider, packageConf.DoSetAttrInfo, token, packageConf.FallbackMaterial,
                    infoForToolkits, packageConf.MeshGranularity, zoomLevel, startProgress, startProgress + (int)((endProgress - startProgress) * 0.5f)
                );

                if (placingResult.IsSucceed)
                {
                    if (zoomLevel > 10)
                    {
                        // Grid GameObject名変更
                        for (int i = 0; i < gmlTrans.childCount; i++)
                        {
                            var child = gmlTrans.GetChild(i);
                            var gridName = child.name;
                            child.name = GetTileName(zoomLevel, gmlName, gridName);
                        }
                    }
                    progressDisplay.SetProgress(gmlName, endProgress, $"ズームレベル:{zoomLevel} 完了");
                    HandlePostProcessors(placingResult, gml, zoomLevel);
                }
                else
                {
                    progressDisplay.SetProgress(gmlName, 0f, "失敗 : モデルの変換または配置に失敗しました。");
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
        internal async Task ImportCombinedTiles(int zoomLevel, float startProgress, float endProgress, CancellationToken? token)
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

                var packageConf = importConfig.GetConfigForPackage(package);
                var infoForToolkits = new CityObjectGroupInfoForToolkits(packageConf.EnableTexturePacking, false);

                var tileGroups = GetCityModelsForEachTile(cityModels, zoomLevel); // ズームレベルに応じた結合タイル数ごとにまとめる

                foreach (var cityModelsInTile in tileGroups)
                {
                    token?.ThrowIfCancellationRequested();

                    var firstGml = cityModelGml[cityModelsInTile.FirstOrDefault()]; // epsg判定、gml名取得用
                    var firstGmlName = firstGml != null ? Path.GetFileName(firstGml.Path) : package.ToString(); // 結合する場合は、最初のGML名、又は パッケージ名を使用
                    var gameObjectName = GetTileName(zoomLevel, firstGmlName);
                    var gmlTrans = new GameObject(gameObjectName).transform;

                    if (!TryCreateMeshExtractOptions(gmlTrans, firstGml, firstGmlName, zoomLevel,
                        out var meshExtractOptions))
                    {
                        return;
                    }

                    // ここはメインスレッドで呼ぶ必要があります。
                    var placingResult = await CityModelToGameObject(
                        cityModelsInTile, meshExtractOptions, importConfig.AreaGridCodes, gmlTrans, firstGmlName,
                        packageConf.DoSetMeshCollider, packageConf.DoSetAttrInfo, token, packageConf.FallbackMaterial,
                        infoForToolkits, packageConf.MeshGranularity, zoomLevel, startProgress, startProgress + (int)((endProgress - startProgress) * 0.5f)
                    );

                    foreach (var cityModel in cityModelsInTile)
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

                    if (placingResult.IsSucceed)
                    {
                        HandlePostProcessors(placingResult, firstGml, zoomLevel);
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
        /// CityModelをUnityのGameObjectに変換
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
        public async Task<GranularityConvertResult> CityModelToGameObject(
            List<CityModel> cityModels, MeshExtractOptions meshExtractOptions, GridCodeList selectedGridCodes,
            Transform parentTrans, string progressName,
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
        /// メッシュ抽出処理です。
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
            var extents = selectedGridCodes.GridCodes.Select(code =>
            {
                var extent = code.Extent;
                extent.Min.Height = -999999.0;
                extent.Max.Height = 999999.0;
                code.Dispose(); // 廃棄
                return extent;
            }).ToList();

            if (zoomLevel <= 9)
                TileExtractor.ExtractWithCombine(ref model, cityModels, meshExtractOptions, extents); //結合
            else
                TileExtractor.ExtractWithGrid(ref model, cityModels.FirstOrDefault(), meshExtractOptions, extents); //分割、又はエリア単位

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
        private bool TryCreateMeshExtractOptions(Transform gmlTrans, GmlFile fetchedGmlFile, string gmlName, int zoomLevel, out MeshExtractOptions result)
        {
            MeshExtractOptions meshExtractOptions;
            bool success = false;
            try
            {
                // TODO : ここの設定はPrefab生成時は不要？
                gmlTrans.parent = rootTransform;
                meshExtractOptions = importConfig.CreateNativeConfigFor(fetchedGmlFile.Package, fetchedGmlFile);
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

        /// <summary>
        /// GMLインポート後に、PostGmlImportProcessorの処理を呼び出します。
        /// </summary>
        /// <param name="convertResult"></param>
        /// <param name="gml"></param>
        private void HandlePostProcessors(GranularityConvertResult convertResult, GmlFile gml, int zoomLevel)
        {
            if (postGmlProcessors != null)
            {
                var result = new TileImportResult(convertResult.GeneratedObjs, 0, gml.GridCode.StringCode, gml, convertResult.GeneratedRootTransforms.CalcCommonParent()?.gameObject, zoomLevel);
                foreach (var processor in postGmlProcessors)
                {
                    if(processor is IPostTileImportProcessor)
                        (processor as IPostTileImportProcessor).OnTileImported(result);
                    else
                        processor.OnGmlImported(result);
                }
            }
        }

        /// <summary>
        /// タイルのGameObject名を取得します。
        /// GameObject名：tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
        /// 例:tile_zoom_0_grid_meshcode_gameobjectname_0
        /// meshcodeは、gameobjectnameに含まれている
        /// 同名の場合の処理は別処理
        /// </summary>
        /// <param name="zoomLevel">9,10,11</param>
        /// <param name="gmlName">CityGML名</param>
        /// <param name="gridName">Grid分割されている場合のグリッド名(GRID1, GRID2...)</param>
        /// <returns></returns>
        public static string GetTileName(int zoomLevel, string gmlName, string gridName = null)
        {
            if (gridName?.StartsWith("GRID") ?? false)
            {
                // GRID名がある場合は、グリッド番号を抽出してGML名に追加
                var gridNum = gridName.TrimStart('G', 'R', 'I', 'D');
                var splittedGml = gmlName.Split('_').ToList();
                splittedGml[0] = $"{splittedGml.First()}{gridNum}"; // GML名にグリッド番号を追加
                gmlName = splittedGml.ToArray().Join2String("_");
            }
            return $"tile_zoom_{zoomLevel}_grid_{gmlName.TrimEnd('.', 'g', 'm', 'l')}";
        }
    }
}
