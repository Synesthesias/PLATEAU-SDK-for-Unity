using PLATEAU.CityImport.AreaSelector;
using PLATEAU.Dataset;
using PLATEAU.Native;

namespace PLATEAU.Tests.EditModeTests.TestDoubles
{
    internal class DummyAreaSelectResultReceiver : IAreaSelectResultReceiver
    {
        // TODO 結果が3つ渡されるけど、1つのクラスにまとめたほうが綺麗になりそう。
        public string[] ResultMeshCodes { get; set; }
        public Extent ResultExtent { get; set; }
        public PredefinedCityModelPackage ResultAvailablePackageFlags { get; set; }
        
        public void ReceiveResult(string[] areaMeshCodes, Extent extent, PredefinedCityModelPackage availablePackageFlags)
        {
            ResultMeshCodes = areaMeshCodes;
            ResultExtent = extent;
            ResultAvailablePackageFlags = availablePackageFlags;
        }
    }
}
