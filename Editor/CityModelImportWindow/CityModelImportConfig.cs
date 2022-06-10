using LibPLATEAU.NET.CityGML;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    public class CityModelImportConfig
    {
        public bool OptimizeFlg { get; set; } = true;
        public MeshGranularity MeshGranularity { get; set; } = MeshGranularity.PerPrimaryFeatureObject;
        public DllLogLevel LogLevel { get; set; } = DllLogLevel.Error;

        public CityModelImportConfig()
        {
        }
    }
}