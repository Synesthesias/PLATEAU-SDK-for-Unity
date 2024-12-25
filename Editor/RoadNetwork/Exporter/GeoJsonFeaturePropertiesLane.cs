namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 車線（レーン）に関連するGeoJSONフィーチャのプロパティを定義するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesLane : GeoJsonFeatureProperties
    {
        /// <summary>
        /// レーンの一意な識別子
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// レーンが属するリンクの識別子
        /// </summary>
        public string LINKID { get; set; }

        /// <summary>
        /// レーンの位置を示す番号（左からの順番）
        /// </summary>
        public int LANEPOS { get; set; }

        /// <summary>
        /// レーンの長さ（m）
        /// </summary>
        public double LENGTH { get; set; }

        /// <summary>
        /// レーンの幅（m）
        /// </summary>
        public double WIDTH { get; set; }
    }
}