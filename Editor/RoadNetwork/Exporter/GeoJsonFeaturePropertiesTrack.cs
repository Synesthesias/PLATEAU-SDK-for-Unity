namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 交差点軌跡（トラック）に関連するGeoJSONフィーチャのプロパティを定義するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesTrack : GeoJsonFeatureProperties
    {
        /// <summary>
        /// トラックの一意な識別子
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// トラックの順序を示す値
        /// </summary>
        public int ORDER { get; set; }

        /// <summary>
        /// 上流リンクの識別子
        /// </summary>
        public string UPLINKID { get; set; }

        /// <summary>
        /// 上流レーンの位置を示す番号（左からの順番）
        /// </summary>
        public int UPLANEPOS { get; set; }

        /// <summary>
        /// 上流リンクからの距離（m）
        /// </summary>
        public double UPDISTANCE { get; set; }

        /// <summary>
        /// 下流リンクの識別子
        /// </summary>
        public string DOWNLINKID { get; set; }

        /// <summary>
        /// 下流レーンの位置を示す番号（左からの順番）
        /// </summary>
        public int DOWNLANEPOS { get; set; }

        /// <summary>
        /// 下流リンクからの距離（m）
        /// </summary>
        public double DOWNDISTANCE { get; set; }

        /// <summary>
        /// トラックの長さ（m）
        /// </summary>
        public double LENGTH { get; set; }

        // TODO: 以下のプロパティはAVENUEへの互換のため必要です

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