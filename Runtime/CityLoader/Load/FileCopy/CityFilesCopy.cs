using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.CityLoader.Load.FileCopy
{
    internal static class CityFilesCopy
    {
        /// <summary>
        /// 条件に合うGMLファイルを検索し、そのGMLと関連ファイルを StreamingAssets 以下の規定のフォルダにコピーします。
        /// </summary>
        /// <param name="sourcePath">検索元となるルートフォルダです。</param>
        /// <param name="config">検索対象となるGMLファイルの絞り込みの条件として利用します。</param>
        /// <returns>コピー先のルートフォルダのパスを返します。</returns>
        public static string ToStreamingAssets(string sourcePath, CityLoadConfig config)
        {
            // TODO 非同期にする
            // 条件に合うGMLファイルを検索して記憶します。
            string destPath = PathUtil.plateauSrcFetchDir;
            var fetchTargetGmls = new List<GmlFileInfo>();
            var gmlInfoToDestroy = new List<GmlFileInfo>();
            var gmlPathsDict = config.SearchMatchingGMLList(sourcePath, out var collection);
            foreach (var gmlPath in gmlPathsDict.SelectMany(pair => pair.Value))
            {
                var gmlInfo = GmlFileInfo.Create(gmlPath);
                gmlInfoToDestroy.Add(gmlInfo);
                fetchTargetGmls.Add(gmlInfo);
            }

            // GMLと関連ファイルをコピーします。
            int targetGmlCount = fetchTargetGmls.Count;
            for (int i = 0; i < targetGmlCount; i++)
            {
                var gml = fetchTargetGmls[i];
                EditorUtility.DisplayProgressBar("", $"インポート処理中 : [{i}/{targetGmlCount}] {Path.GetFileName(gml.Path)}",
                    (float)i / targetGmlCount);
                collection.Fetch(destPath, gml);
            }
            foreach(var gml in gmlInfoToDestroy) gml.Dispose();
            
            EditorUtility.ClearProgressBar();

            string destFolderName = Path.GetFileName(sourcePath);
            string destRootFolderPath = Path.Combine(destPath, destFolderName);
            return destRootFolderPath;
        }
    }
}
