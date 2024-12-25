namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 交差点（ノード）に関連するGeoJSONフィーチャのプロパティを定義するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesNode : GeoJsonFeatureProperties
    {
        /// <summary>
        /// ノードの一意な識別子
        /// </summary>
        public string ID { get; set; }
    }
}