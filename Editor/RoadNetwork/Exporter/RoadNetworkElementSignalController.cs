using System.Collections.Generic;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 信号制御器を表すクラス
    /// </summary>
    public class RoadNetworkElementSignalController : RoadNetworkElement
    {
        /// <summary>
        /// 信号制御器識別子のプレフィックス
        /// </summary>
        public static readonly string IDPrefix = "SignalController";

        /// <summary>
        /// 設置ノード
        /// </summary>
        public RoadNetworkElementNode Node;

        /// <summary>
        /// 制御対象の信号灯器
        /// </summary>
        public List<RoadNetworkElementSignalLight> SignalLights = new List<RoadNetworkElementSignalLight>();

        /// <summary>
        /// オフセット基準信号制御器
        /// </summary>
        public RoadNetworkElementSignalController OffsetController;

        /// <summary>
        /// オフセットタイプ
        /// </summary>
        public int OffsetType;

        /// <summary>
        /// オフセット値
        /// </summary>
        public int OffsetValue;

        /// <summary>
        /// 信号パターンのリスト
        /// </summary>
        public List<(string StartTime, List<RoadNetworkElementSignalStep> SignalSteps)> SignalPatterns = new List<(string, List<RoadNetworkElementSignalStep>)>();

        /// <summary>
        /// パターンインデックス
        /// </summary>
        public int PatternIndex;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">道路ネットワークのコンテキスト</param>
        /// <param name="id">ID</param>
        /// <param name="index">インデックス</param>
        public RoadNetworkElementSignalController(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
        }

        /// <summary>
        /// 信号制御器の識別子を生成します
        /// </summary>
        /// <param name="id">元のID</param>
        /// <returns>新しいID</returns>
        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

        /// <summary>
        /// 設置ノードを取得します
        /// </summary>
        /// <returns>設置ノード</returns>
        public string GetNode()
        {
            return Node.ID;
        }

        /// <summary>
        /// 制御対象の制御信号灯器を取得します
        /// </summary>
        /// <returns>制御対象の制御信号灯器の文字列</returns>
        public string GetSignalLights()
        {
            if (SignalLights.Count == 0)
            {
                return "";
            }

            var ret = SignalLights[0].ID;

            for (int i = 1; i < SignalLights.Count; i++)
            {
                ret += ":" + SignalLights[i].ID;
            }

            return ret;
        }

        /// <summary>
        /// 制御パターン数を取得します
        /// </summary>
        /// <returns>パターンの数</returns>
        public int GetPatternNum()
        {
            return SignalPatterns.Count;
        }

        /// <summary>
        /// 制御パターンを取得します
        /// </summary>
        /// <returns>パターンIDの文字列</returns>
        public string GetPatternID()
        {
            if (SignalPatterns.Count == 0 || SignalPatterns[PatternIndex].SignalSteps.Count == 0)
            {
                return "";
            }

            var ret = SignalPatterns[PatternIndex].SignalSteps[0].PatternID;

            for (int i = 1; i < SignalPatterns.Count; i++)
            {
                ret += ":" + SignalPatterns[PatternIndex].SignalSteps[i].PatternID;
            }

            return ret;
        }

        /// <summary>
        /// サイクル長を取得します
        /// </summary>
        /// <returns>サイクルの長さ</returns>
        public int GetCycleLen()
        {
            int cycle = 0;

            SignalPatterns[PatternIndex].SignalSteps.ForEach(x => cycle += x.Duration);

            return cycle;
        }

        /// <summary>
        /// 現示数を取得します
        /// </summary>
        /// <returns>フェーズの数</returns>
        public int GetPhaseNum()
        {
            return SignalPatterns[PatternIndex].SignalSteps.Count;
        }

        /// <summary>
        /// 制御パターン開始時間を取得します
        /// </summary>
        /// <returns>開始時間の文字列</returns>
        public string GetStartTime()
        {
            if (SignalPatterns.Count == 0)
            {
                return "";
            }

            var ret = SignalPatterns[0].StartTime;

            for (int i = 1; i < SignalPatterns.Count; i++)
            {
                ret += ":" + SignalPatterns[i].StartTime;
            }

            return ret;
        }

        /// <summary>
        /// ジオメトリ情報を取得します
        /// </summary>
        /// <returns>ジオメトリの位置</returns>
        public GeoJSON.Net.Geometry.Position GetGeometory()
        {
            return Node.GetGeometory();
        }
    }
}