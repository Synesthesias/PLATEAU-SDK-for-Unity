using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Udx;
using PLATEAU.Util.Async;

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
                    await GMLLoader.Load(
                        // TODO これは仮。ここに後半の設定を正しく渡せるようにする。
                        gmlPath,
                        packageConf.meshGranularity,
                        packageConf.minLOD, packageConf.maxLOD, packageConf.includeTexture, 5,
                        -90, -180, 90, 180
                    );
                }
            }
        }
    }
}
