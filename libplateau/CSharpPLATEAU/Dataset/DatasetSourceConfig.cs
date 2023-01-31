using System;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// <see cref="DatasetSource"/> の初期化方法は、データの場所がローカルかサーバーかで異なりますが、
    /// その違いを吸収するためのクラスです。
    /// </summary>
    [Serializable]
    public class DatasetSourceConfig
    {
        public bool IsServer { get; set; }
        public string LocalSourcePath { get; set; }
        public string ServerDatasetID { get; set; }
        public string ServerUrl { get; set; }
        public string ServerToken { get; set; }

        /// <summary>
        /// <see cref="DatasetSource"/> の初期化のための情報を渡すコンストラクタです。
        /// </summary>
        /// <param name="isServer">データの場所は true ならサーバー、falseならローカルです。</param>
        /// <param name="localSourcePath">ローカルモードでのみ利用します。インポート元のパスを渡します。</param>
        /// <param name="serverDatasetID">
        /// サーバーモードでのみ利用します。データセットのIDを渡します。
        /// そのIDとは、APIサーバーにデータセットの一覧を問い合わせたときに得られるID文字列です。例: 東京23区のデータセットのIDは "23ku"
        /// </param>
        /// <param name="serverUrl">サーバーモードでのみ利用します。サーバーのURLです。</param>
        /// <param name="serverToken">サーバーモードでのみ利用します。サーバー認証のトークンです。</param>
        public DatasetSourceConfig(bool isServer, string localSourcePath, string serverDatasetID, string serverUrl, string serverToken)
        {
            IsServer = isServer;
            LocalSourcePath = localSourcePath;
            ServerDatasetID = serverDatasetID;
            ServerUrl = serverUrl;
            ServerToken = serverToken;
        }
    }
}
