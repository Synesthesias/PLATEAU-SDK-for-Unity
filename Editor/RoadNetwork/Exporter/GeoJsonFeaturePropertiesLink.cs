namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路（リンク）に関連するGeoJSONフィーチャのプロパティを定義するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesLink : GeoJsonFeatureProperties
    {
        /// <summary>
        /// リンクの一意な識別子
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 上流ノードの識別子
        /// </summary>
        public string UPNODE { get; set; }

        /// <summary>
        /// 下流ノードの識別子
        /// </summary>
        public string DOWNNODE { get; set; }

        /// <summary>
        /// リンクの長さ（m）
        /// </summary>
        public double LENGTH { get; set; }

        /// <summary>
        /// 車線数
        /// </summary>
        public int LANENUM { get; set; }

        /// <summary>
        /// 右折（付加）レーンの数
        /// </summary>
        public int RLANENUM { get; set; }

        /// <summary>
        /// 右折（付加）レーンの長さ（m）
        /// </summary>
        public int RLANELENGTH { get; set; }

        /// <summary>
        /// 左折（付加）レーンの数
        /// </summary>
        public int LLANENUM { get; set; }

        /// <summary>
        /// 左折（付加）レーンの長さ（m）
        /// </summary>
        public int LLANELENGTH { get; set; }

        // TODO: 以下のプロパティはAVENUEへの互換のため必要です

        /// <summary>
        /// 通行禁止情報
        /// </summary>
        public string PROHIBIT { get; set; } = GeoJsonFeatureProperties.ProhiBit.UTurn;

        /// <summary>
        /// 進行可能な方向の設定
        /// </summary>
        public int TURNCONFIG { get; set; } = (int)(GeoJsonFeatureProperties.TurnConfig.Left | GeoJsonFeatureProperties.TurnConfig.Straight | GeoJsonFeatureProperties.TurnConfig.Right);

        /// <summary>
        /// 進行可能な車両の種類
        /// </summary>
        public int TYPECONFIG { get; set; } = (int)(GeoJsonFeatureProperties.TypeConfig.Small | GeoJsonFeatureProperties.TypeConfig.Large | GeoJsonFeatureProperties.TypeConfig.Bus);
    }
}