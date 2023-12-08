using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageLoadConfigs;

namespace PLATEAU.CityImport.AreaSelector
{
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(AreaSelectResult areaSelectResult);
    }
    
    public class AreaSelectResult
    {
        public MeshCodeList AreaMeshCodes { get; }
        public PackageToLodDict PackageToLodDict { get; }

        public AreaSelectResult(MeshCodeList areaMeshCodes, PackageToLodDict packageToLodDict)
        {
            AreaMeshCodes = areaMeshCodes;
            PackageToLodDict = packageToLodDict;
        }
    }
}
