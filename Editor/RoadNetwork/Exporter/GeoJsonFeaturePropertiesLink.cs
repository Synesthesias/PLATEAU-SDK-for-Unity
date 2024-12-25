namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// RoadNetworkのリンクのプロパティを保持するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesLink : GeoJsonFeatureProperties
    {
        public string ID { get; set; }
        public string UPNODE { get; set; }
        public string DOWNNODE { get; set; }
        public double LENGTH { get; set; }
        public int LANENUM { get; set; }
        public int RLANENUM { get; set; }
        public int RLANELENGTH { get; set; }
        public int LLANENUM { get; set; }
        public int LLANELENGTH { get; set; }
        public string PROHIBIT { get; set; } = GeoJsonFeatureProperties.ProhiBit.UTurn;
        public int TURNCONFIG { get; set; } = (int)(GeoJsonFeatureProperties.TurnConfig.Left | GeoJsonFeatureProperties.TurnConfig.Straight | GeoJsonFeatureProperties.TurnConfig.Right);
        public int TYPECONFIG { get; set; } = (int)(GeoJsonFeatureProperties.TypeConfig.Small | GeoJsonFeatureProperties.TypeConfig.Large | GeoJsonFeatureProperties.TypeConfig.Bus);
    }
}