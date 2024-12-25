namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// RoadNetworkの信号制御機のプロパティを保持するクラス
    /// </summary>
    public class GeoJsonFeaturePropertiesSignalController : GeoJsonFeatureProperties
    {
        public string ID { get; set; }
        public string ALLOCNODE { get; set; }
        public string SIGLIGHT { get; set; }
        public string OFFSETBASESIGID { get; set; }
        public int NUMOFPATTERN { get; set; }
        public string PATTERNID { get; set; }
        public int INITCYCLE { get; set; }
        public int PHASENUM { get; set; }
        public int OFFSETTYPE { get; set; }
        public int OFFSET { get; set; }
        public string STARTTIME { get; set; }
    }
}