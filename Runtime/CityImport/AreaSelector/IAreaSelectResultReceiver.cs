using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Native;

namespace PLATEAU.CityImport.AreaSelector
{
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(AreaSelectResult areaSelectResult);
    }
    
    public class AreaSelectResult
    {
        public string[] AreaMeshCodes { get; }
        public PackageToLodDict PackageToLodDict { get; }

        public AreaSelectResult(string[] areaMeshCodes, PackageToLodDict packageToLodDict)
        {
            AreaMeshCodes = areaMeshCodes;
            PackageToLodDict = packageToLodDict;
        }
    }
}
