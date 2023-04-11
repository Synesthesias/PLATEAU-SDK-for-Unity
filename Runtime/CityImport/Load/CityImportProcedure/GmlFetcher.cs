using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public static async Task<GmlFile> FetchAsync(GmlFile gmlFileBeforeFetch, string destPath, string gmlName, IProgressDisplay progressDisplay, bool isServer, CancellationToken token)
        {
            var fetchedGmlFile = isServer switch
            {
                false => await FetchLocalAsync(gmlFileBeforeFetch, destPath, gmlName, progressDisplay, token),
                true => await FetchRemoteAsync(gmlFileBeforeFetch, destPath, gmlName, progressDisplay, token)
            };
            return fetchedGmlFile;
        } 
        
        private static async Task<GmlFile> FetchLocalAsync(GmlFile gmlFile, string destPath, string gmlName, IProgressDisplay progressDisplay, CancellationToken token)
        {
            progressDisplay.SetProgress(gmlName, 5f, "ファイルコピー中");

            // GMLと関連ファイルを StreamingAssets にコピーします。
            var fetchedGml = await Task.Run(() => gmlFile.Fetch(destPath));
            return fetchedGml;
        }

        private static async Task<GmlFile> FetchRemoteAsync(GmlFile remoteGmlFile, string destPath, string gmlName, IProgressDisplay progressDisplay, CancellationToken token)
        {
            progressDisplay.SetProgress(gmlName, 5f, $"ダウンロード中 :{gmlName}");
            // GMLファイルを StreamingAssets にダウンロードします。
            var downloadedGml = await Task.Run(() => remoteGmlFile.Fetch(destPath));
            Debug.Log($"downloaded {remoteGmlFile.Path}\nto {downloadedGml.Path}");
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
