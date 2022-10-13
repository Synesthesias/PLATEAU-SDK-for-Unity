using PLATEAU.Interop;
using PLATEAU.Udx;

namespace PLATEAU.CityImport.AreaSelector
{
    public interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(string[] areaMeshCodes, Extent extent,
            PredefinedCityModelPackage availablePackageFlags);
    }
}
