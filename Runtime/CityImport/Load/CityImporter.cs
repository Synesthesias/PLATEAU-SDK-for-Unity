using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityAdjust;
using PLATEAU.CityImport.Load.CityImportProcedure;
using PLATEAU.CityImport.Setting;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
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
        static string  lastFetchedGmlRootPath = "";
        /// <summary>
        /// <see cref="CityImporter"/> クラスのメインメソッドです。
        /// GMLファイルから都市モデルを読み、そのメッシュをUnity向けに変換してシーンに配置します。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        public static async Task ImportAsync(CityLoadConfig config, IProgressDisplay progressDisplay)
        {
            var datasetSourceConfig = config.DatasetSourceConfig;
            string destPath = PathUtil.PLATEAUSrcFetchDir;
            // string destFolderName = datasetSourceConfig.RootDirName;

            if ((!datasetSourceConfig.IsServer) && (!Directory.Exists(datasetSourceConfig.LocalSourcePath)))
            {
                Debug.LogError($"インポート元パスが存在しません。 sourcePath = {datasetSourceConfig.LocalSourcePath}");
                return;
            }
            
            progressDisplay.SetProgress("GMLファイル検索", 10f, "");
            List<GmlFile> targetGmls = null;
            try
            {
                targetGmls = await Task.Run(config.SearchMatchingGMLList);
            }
            catch (Exception)
            {
                progressDisplay.SetProgress("GMLファイル検索", 0f, "失敗 : GMLファイルを検索できませんでした。");
                throw;
            }

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
            
            // 都市ゲームオブジェクト階層のルートを生成します。
            // ここで指定するゲームオブジェクト名は仮であり、あとからインポートしたGMLファイルパスに応じてふさわしいものに変更します。
            var rootTrans = new GameObject("インポート中です...").transform;

            // 基準点を設定します。基準点はどのGMLファイルでも共通です。（そうでないと複数のGMLファイル間で位置が合わないため。）
            var referencePoint = config.ReferencePoint;
            
            // ルートのGameObjectにコンポーネントを付けます。 
            var cityModelComponent = rootTrans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
            cityModelComponent.GeoReference =
                GeoReference.Create(referencePoint, GmlImporter.UnitScale, GmlImporter.MeshAxes, config.CoordinateZoneID);
            
            // GMLファイルを同時に処理する最大数です。
            // 並列数が 4 くらいだと、1つずつ処理するよりも、全部同時に処理するよりも速いという経験則です。
            // ただしメモリ使用量が増えます。
            var semGmlProcess = new SemaphoreSlim(4);
            // 上記に関わらず、fetch処理に関しては同時に動くのは 1 つのみとします。
            // なぜなら、ファイルコピー が並列で動くのはトラブルの元(特に同じ codelist を同時にコピーしようとしがち) だからです。
            var semGmlFetch = new SemaphoreSlim(1);
            await Task.WhenAll(targetGmls.Select(async gmlInfo =>
            {
                await semGmlProcess.WaitAsync(); 
                try
                {
                    GmlFile fetchedGml;
                    // GMLを1つ fetch します。同時に走らないようにします。
                    await semGmlFetch.WaitAsync();
                    try
                    {
                        fetchedGml = await GmlImporter.Fetch(gmlInfo, destPath, config, progressDisplay);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        progressDisplay.SetProgress(Path.GetFileName(gmlInfo.Path), 0f, "GMLファイルの取得に失敗しました。");
                        fetchedGml = null;
                    }
                    finally
                    {
                        semGmlFetch.Release();
                    }
                    
                    
                    
                    if (fetchedGml != null && !string.IsNullOrEmpty(fetchedGml.Path))
                    {
                        // GMLを1つインポートします。
                        // ここはメインスレッドで呼ぶ必要があります。
                        await GmlImporter.Import(fetchedGml, config, rootTrans, progressDisplay, referencePoint);
                        lock (lastFetchedGmlRootPath)
                        {
                            lastFetchedGmlRootPath = fetchedGml.CityRootPath();
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
            
            // インポート完了後の処理
            CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(cityModelComponent);
            rootTrans.name = Path.GetFileName(lastFetchedGmlRootPath);
        }
    }
}
