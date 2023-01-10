using PLATEAU.Interop;
using PLATEAU.Dataset;
using PLATEAU.Native;

namespace PLATEAU.CityImport.AreaSelector
{
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(string[] areaMeshCodes, Extent extent,
            PredefinedCityModelPackage availablePackageFlags);
    }
}
