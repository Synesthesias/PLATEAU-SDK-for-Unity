using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Util;

namespace PLATEAU.CityImport.Load.FileCopy
{
    internal static class CityFilesCopy
    {
        /// <summary>
        /// 条件に合うGMLファイルを検索し、そのGMLと関連ファイルを StreamingAssets 以下の規定のフォルダにコピーします。
        /// </summary>
        /// <param name="sourcePath">検索元となるルートフォルダです。</param>
        /// <param name="config">検索対象となるGMLファイルの絞り込みの条件として利用します。</param>
        /// <param name="progressDisplay">進捗表示のインターフェイスです。</param>
        /// <param name="destPath">コピー先のパスです。</param>
        /// <returns>コピー先のルートフォルダのパスを返します。</returns>
        public static string ToStreamingAssets(string sourcePath, CityLoadConfig config, IProgressDisplay progressDisplay, string destPath)
        {
            // TODO 非同期にする
            // 条件に合うGMLファイルを検索して記憶します。
            // TODO サーバー対応
            var datasetAccessor = DatasetSource.Create(false, sourcePath).Accessor;
            var fetchTargetGmls = FindTargetGmls(datasetAccessor, config);

            // GMLと関連ファイルをコピーします。
            int targetGmlCount = fetchTargetGmls.Count;
            for (int i = 0; i < targetGmlCount; i++)
            {
                var gml = fetchTargetGmls[i];
                progressDisplay.SetProgress("インポート処理", 100f * i / targetGmlCount, $"[{i+1} / {targetGmlCount}] {Path.GetFileName(gml.Path)}");
                gml.Fetch(destPath);
            }

            string destFolderName = Path.GetFileName(sourcePath);
            string destRootFolderPath = Path.Combine(destPath, destFolderName);
            progressDisplay.SetProgress("インポート処理", 100f, "完了");
            return destRootFolderPath;
        }

        public static List<GmlFile> FindTargetGmls(DatasetAccessor datasetAccessor, CityLoadConfig config)
        {
            var fetchTargetGmls = new List<GmlFile>();
            var gmlFilesDict = config.SearchMatchingGMLList(datasetAccessor);
            foreach (var gmlFile in gmlFilesDict.SelectMany(pair => pair.Value))
            {
                fetchTargetGmls.Add(gmlFile);
            }

            return fetchTargetGmls;
        }
    }
}
