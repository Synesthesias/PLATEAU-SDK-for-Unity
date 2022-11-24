using PLATEAU.Interop;
using PLATEAU.Dataset;

namespace PLATEAU.CityImport.AreaSelector
{
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(string[] areaMeshCodes, Extent extent,
            PredefinedCityModelPackage availablePackageFlags);
    }
}
