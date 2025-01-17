namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 信号灯火機に関連するGeoJSONフィーチャのプロパティを定義するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesSignalLight : GeoJsonFeatureProperties
    {
        /// <summary>
        /// 信号灯火器の一意な識別子
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 属する信号制御器の識別子
        /// </summary>
        public string SIGNALID { get; set; }

        /// <summary>
        /// 設置対象のレーンの識別子
        /// </summary>
        public string LINKID { get; set; }

        /// <summary>
        /// 規制対象レーン種別
        /// lane:レーン上に設置される、track：トラック上に設置される
        /// </summary>
        public string LANETYPE { get; set; } = "lane"; // レーンに設置

        /// <summary>
        /// 規制対象レーン番号（-1:全レーン）
        /// </summary>
        public string LANEPOS { get; set; }

        /// <summary>
        /// 設置位置（停止線からの距離）
        /// </summary>
        public string DISTANCE { get; set; }
    }
}