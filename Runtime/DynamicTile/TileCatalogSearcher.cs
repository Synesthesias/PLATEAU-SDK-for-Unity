using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Util;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Addressables のカタログファイルを検索します。
    /// Unity 2022 では json、Unity 6 では bin 拡張子になるため両方に対応します。
    /// </summary>
    public static class TileCatalogSearcher
    {
        private static readonly string[] CatalogFilePatterns = new[]
        {
            "catalog_*.json",
            "catalog_*.bin"
        };

        /// <summary>
        /// 指定ディレクトリ配下からカタログファイル一覧を取得します。
        /// </summary>
        /// <param name="searchDirectory">検索開始ディレクトリ</param>
        /// <param name="includeSubdirectories">サブディレクトリも検索するか</param>
        /// <returns>更新日時の新しい順に並んだファイルパス配列（見つからなければ空配列）</returns>
        public static string[] FindCatalogFiles(string searchDirectory, bool includeSubdirectories)
        {
            if (string.IsNullOrEmpty(searchDirectory) || !Directory.Exists(searchDirectory))
            {
                return Array.Empty<string>();
            }

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            IEnumerable<string> EnumerateMatches()
            {
                foreach (var pattern in CatalogFilePatterns)
                {
                    string[] files = Array.Empty<string>();
                    try
                    {
                        files = Directory.GetFiles(searchDirectory, pattern, searchOption);
                    }
                    catch
                    {
                        // 読み取りエラーは無視し、他のパターン/ディレクトリを継続
                    }
                    foreach (var f in files)
                    {
                        // プロジェクト内ならAssetsから始める
                        if (PathUtil.IsSubDirectoryOfAssets(f))
                        {
                            yield return PathUtil.FullPathToAssetsPath(f);
                        }
                        else
                        {
                            yield return f;
                        }
                    }
                }
            }

            return EnumerateMatches()
                .Distinct()
                .OrderByDescending(p => File.GetLastWriteTimeUtc(Path.GetFullPath(p)))
                .ToArray();
        }

        /// <summary>
        /// 指定ディレクトリ配下の最新のカタログファイルを取得します。
        /// </summary>
        public static string FindLatestCatalogFile(string searchDirectory, bool includeSubdirectories)
        {
            return FindCatalogFiles(searchDirectory, includeSubdirectories).FirstOrDefault();
        }
    }
}


