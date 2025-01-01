namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路ネットワークの要素を表すベースクラス
    /// </summary>
    public class RoadNetworkElement
    {
        /// <summary>
        /// 要素の識別子
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 道路ネットワークのコンテキスト
        /// </summary>
        protected RoadNetworkContext roadNetworkContext;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">道路ネットワークのコンテキスト</param>
        /// <param name="id">要素のID</param>
        public RoadNetworkElement(RoadNetworkContext context, string id)
        {
            ID = id;

            roadNetworkContext = context;
        }
    }
}