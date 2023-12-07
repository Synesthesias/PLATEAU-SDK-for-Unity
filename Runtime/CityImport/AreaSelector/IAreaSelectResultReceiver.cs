using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityImport.Config.PackageLoadConfigs;

namespace PLATEAU.CityImport.AreaSelector
{
    public interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(AreaSelectResult areaSelectResult);
    }
    
    public class AreaSelectResult
    {
        public string[] AreaMeshCodes { get; }
        public PackageToLodDict PackageToLodDict { get; }

        public AreaSelectResult(IEnumerable<string> areaMeshCodes, PackageToLodDict packageToLodDict)
        {
            AreaMeshCodes = areaMeshCodes.ToArray();
            PackageToLodDict = packageToLodDict;
        }
    }
}
