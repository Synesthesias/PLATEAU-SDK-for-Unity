using System;
using System.IO;

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
        public string DatasetIdOrSourcePath { get; set; }
        
        /// <summary>
        /// <see cref="DatasetSource"/> の初期化のための情報を渡すコンストラクタです。
        /// </summary>
        /// <param name="isServer">データの場所は true ならサーバー、falseならローカルです。</param>
        /// <param name="datasetIdOrSourcePath">
        /// サーバーのとき、データセットのIDを渡します。
        /// そのIDとは、APIサーバーにデータセットの一覧を問い合わせたときに得られるID文字列です。例: 東京23区のデータセットのIDは "23ku"
        /// ローカルのとき、そのパスを渡します。
        /// </param>
        public DatasetSourceConfig(bool isServer, string datasetIdOrSourcePath)
        {
            IsServer = isServer;
            DatasetIdOrSourcePath = datasetIdOrSourcePath;
        }

        public string RootDirName
        {
            get
            {
                switch (IsServer)
                {
                    case false:
                        return Path.GetFileName(DatasetIdOrSourcePath);
                    case true:
                        return DatasetIdOrSourcePath;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
