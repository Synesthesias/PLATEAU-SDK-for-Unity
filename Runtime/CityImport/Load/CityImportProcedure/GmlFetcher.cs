using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.Dataset;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityImport.Load.CityImportProcedure
{
    /// <summary>
    /// PLATEAU インポート処理の1つとして、GMLと関連ファイルを
    /// StreamingAssets フォルダにコピー(サーバーの場合はダウンロード)します。
    /// </summary>
    internal static class GmlFetcher
    {

        /// <summary>
        /// GMLと関連ファイルを StreamingAssets フォルダにコピー(サーバーの場合はダウンロード)します。
        /// </summary>
        /// <returns>Fetch後のGMLファイルの情報を返します。</returns>
        public static async Task<GmlFile> FetchAsync(GmlFile gmlFileBeforeFetch, string destPath, string gmlName, IProgressDisplay progressDisplay, bool isServer)
        {
            var fetchedGmlFile = isServer switch
            {
                false => await FetchLocalAsync(gmlFileBeforeFetch, destPath, gmlName, progressDisplay),
                true => await FetchRemoteAsync(gmlFileBeforeFetch, destPath, gmlName, progressDisplay)
            };
            return fetchedGmlFile;
        } 
        
        private static async Task<GmlFile> FetchLocalAsync(GmlFile gmlFile, string destPath, string gmlName, IProgressDisplay progressDisplay)
        {
            progressDisplay.SetProgress(gmlName, 5f, "ファイルコピー中");

            // GMLと関連ファイルを StreamingAssets にコピーします。
            var fetchedGml = await Task.Run(() => gmlFile.Fetch(destPath));
            return fetchedGml;
        }

        private static async Task<GmlFile> FetchRemoteAsync(GmlFile remoteGmlFile, string destPath, string gmlName, IProgressDisplay progressDisplay)
        {
            progressDisplay.SetProgress(gmlName, 5f, $"ダウンロード中 :{gmlName}");
            // GMLファイルを StreamingAssets にダウンロードします。
            var downloadedGml = await Task.Run(() => remoteGmlFile.Fetch(destPath));
            Debug.Log($"downloaded {remoteGmlFile.Path}");
            // TODO あとで消す
            // // 関連ファイルを取得します。
            // var pathsToDownload = await Task.Run(() =>
            //     {
            //         var codelistPaths = downloadedGml.SearchAllCodelistPathsInGml();
            //         var imagePaths = downloadedGml.SearchAllImagePathsInGml();
            //         var pathsToDownload = codelistPaths.ToCSharpArray().Concat(imagePaths.ToCSharpArray());
            //         return pathsToDownload.ToArray();
            //     }
            // );
            // string localGmlDirPath = new DirectoryInfo(downloadedGml.Path).Parent?.FullName;
            // if (localGmlDirPath == null) throw new Exception("invalid path.");
            // // 関連ファイルをダウンロードします。
            // var client = Client.Create();
            // client.Url = NetworkConfig.DefaultApiServerUrl;
            // for(int i=0; i<pathsToDownload.Length; i++)
            // {
            //     string relativePath = pathsToDownload[i];
            //     await Task.Run(() =>
            //     {
            //         string gmlUrl = remoteGmlFile.Path;
            //         string remoteUrlParent = gmlUrl.Substring(0, gmlUrl.LastIndexOf("/", StringComparison.Ordinal));
            //         string remoteUrl = ApplyPeriodPath(Path.Combine(remoteUrlParent, relativePath).Replace('\\', '/'));
            //         string localDest = Path.GetFullPath(Path.Combine(localGmlDirPath, relativePath)).Replace('\\', '/');
            //         string localDestDir = new DirectoryInfo(localDest).Parent?.FullName;
            //         if (localDestDir == null) throw new Exception("invalid path.");
            //         Directory.CreateDirectory(localDestDir);
            //         try
            //         {
            //             progressDisplay.SetProgress(gmlName, 6f, $"ダウンロード中 : [{i+1} / {pathsToDownload.Length}] {Path.GetFileName(remoteUrl)}");
            //             client.Download(localDestDir, remoteUrl);
            //             Debug.Log($"Downloaded {remoteUrl}");
            //         }
            //         catch (FileLoadException)
            //         {
            //             Debug.LogError($"Failed to download file: {remoteUrl}");
            //         }
            //
            //     });
            //     progressDisplay.SetProgress(gmlName, 7f, $"ダウンロード完了");
            // }
            // client.Dispose();
            return downloadedGml;
        }

        /// <summary>
        /// 引数のURLに含まれる "../" を適用します。
        /// "./" を除去します。
        /// パス区切りは "/" であることが前提です。
        /// </summary>
        private static string ApplyPeriodPath(string url)
        {
            var tokens = url
                .Split('/')
                .Where(t => t!=".") // "./" を除去します。
                .ToArray();
            while (true)
            {
                // "../" を検索します。
                int doublePeriodIdx = -1;
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (tokens[i] == "..")
                    {
                        doublePeriodIdx = i;
                        break;
                    }
                }
                // なければループを抜けます。
                if (doublePeriodIdx <= 0) break;
                
                // tokens から "../" とその手前のトークンを除いてコピーし、新たな tokens とします。
                var appliedTokens = new List<string>();
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (i == doublePeriodIdx || i == doublePeriodIdx - 1) continue;
                    appliedTokens.Add(tokens[i]);
                }

                tokens = appliedTokens.ToArray();
            }
            return string.Join("/", tokens);
        }
    }
}
