using System;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// データセット設定のローカルとサーバーの違いを吸収するインターフェイスです。
    /// </summary>
    public interface IDatasetSourceConfig
    {
        
    }

    /// <summary>
    /// ローカルインポートで利用する<see cref="DatasetSource"/>の設定です。
    /// </summary>
    public class DatasetSourceConfigLocal : IDatasetSourceConfig
    {
        public string LocalSourcePath { get; set; }

        /// <param name="localSourcePath">インポート元のパスです。</param>
        public DatasetSourceConfigLocal(string localSourcePath)
        {
            LocalSourcePath = localSourcePath;
        }

    }

    /// <summary>
    /// サーバーインポートで利用する<see cref="DatasetSource"/>の設定です。
    /// </summary>
    public class DatasetSourceConfigRemote : IDatasetSourceConfig
    {
        public string ServerDatasetID { get; set; }
        public string ServerUrl { get; set; }
        public string ServerToken { get; set; }

        /// <param name="serverDatasetID">
        /// データセットのIDを渡します。
        /// そのIDとは、APIサーバーにデータセットの一覧を問い合わせたときに得られるID文字列です。
        /// </param>
        /// <param name="serverUrl">サーバーのURLです。</param>
        /// <param name="serverToken">サーバー認証のトークンです。</param>
        public DatasetSourceConfigRemote(string serverDatasetID, string serverUrl, string serverToken)
        {
            ServerDatasetID = serverDatasetID;
            ServerUrl = serverUrl;
            ServerToken = serverToken;
        }
    }

}
