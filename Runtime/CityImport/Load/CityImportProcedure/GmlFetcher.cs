using System.Threading.Tasks;
using PLATEAU.Dataset;
using PLATEAU.Util;

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
            // 下のメソッドは別スレッドで実行可能です。
            var fetchedGmlInfo = await Task.Run(() => gmlFile.Fetch(destPath));
            return fetchedGmlInfo;
        }

        private static async Task<GmlFile> FetchRemoteAsync(GmlFile gmlFile, string destPath, string gmlName, IProgressDisplay progressDisplay)
        {
            progressDisplay.SetProgress(gmlName, 5f, "ファイルダウンロード中");
            // GMLと関連ファイルを StreamingAssets にダウンロードします。
            // 下のメソッドは別スレッドで実行可能です。
            var fetchedGmlInfo = await Task.Run(() => gmlFile.Fetch(destPath));
            return fetchedGmlInfo;
        }
    }
}
