using PLATEAU.Interop;
using PLATEAU.Udx;

namespace PLATEAU.CityImport.AreaSelector
{
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(string[] areaMeshCodes, Extent extent,
            PredefinedCityModelPackage availablePackageFlags);
    }
}
