using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Native;

namespace PLATEAU.CityImport.AreaSelector
{
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(string[] areaMeshCodes, Extent extent,
            PackageToLodDict availablePackageLods);
    }
}
