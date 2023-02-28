using PLATEAU.CityImport.Setting;
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
        public Extent Extent { get; }
        public PackageToLodDict PackageToLodDict { get; }

        public AreaSelectResult(string[] areaMeshCodes, Extent extent, PackageToLodDict packageToLodDict)
        {
            AreaMeshCodes = areaMeshCodes;
            Extent = extent;
            PackageToLodDict = packageToLodDict;
        }
    }
}
