using LibPLATEAU.NET.CityGML;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    public class CityModelImportConfig
    {
        public DllLogLevel LogLevel { get; set; } = DllLogLevel.Error;

        public CityModelImportConfig()
        {
            
        }

        public CityModelImportConfig(DllLogLevel logLevel)
        {
            LogLevel = logLevel;
        }
    }
}