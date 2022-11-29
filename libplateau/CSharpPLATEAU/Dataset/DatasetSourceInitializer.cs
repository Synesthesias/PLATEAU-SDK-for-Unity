using System;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// <see cref="DatasetSource"/> の初期化方法は、データの場所がローカルかサーバーかで異なりますが、
    /// その違いを吸収するためのクラスです。
    /// </summary>
    [Serializable]
    public class DatasetSourceInitializer
    {
        public bool IsServer { get; }
        public string DatasetIdOrSourcePath { get; }
        
        /// <summary>
        /// <see cref="DatasetSource"/> の初期化のための情報を渡すコンストラクタです。
        /// </summary>
        /// <param name="isServer">データの場所は true ならサーバー、falseならローカルです。</param>
        /// <param name="datasetIdOrSourcePath">
        /// サーバーのとき、データセットのIDを渡します。
        /// そのIDとは、APIサーバーにデータセットの一覧を問い合わせたときに得られるID文字列です。例: 東京23区のデータセットのIDは "23ku"
        /// ローカルのとき、そのパスを渡します。
        /// </param>
        public DatasetSourceInitializer(bool isServer, string datasetIdOrSourcePath)
        {
            IsServer = isServer;
            DatasetIdOrSourcePath = datasetIdOrSourcePath;
        }
    }
}
