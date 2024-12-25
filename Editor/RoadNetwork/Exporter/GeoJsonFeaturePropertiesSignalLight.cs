namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// RoadNetworkの信号灯器のプロパティを保持するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesSignalLight : GeoJsonFeatureProperties
    {
        public string ID { get; set; }

        public string SIGNALID { get; set; }

        public string LINKID { get; set; }

        public string LANETYPE { get; set; }

        public string LANEPOS { get; set; }

        public string DISTANCE { get; set; }
    }
}