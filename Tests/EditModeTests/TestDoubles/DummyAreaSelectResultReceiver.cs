using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Native;

namespace PLATEAU.Tests.EditModeTests.TestDoubles
{
    internal class DummyAreaSelectResultReceiver : IAreaSelectResultReceiver
    {
        // TODO 結果が3つ渡されるけど、1つのクラスにまとめたほうが綺麗になりそう。
        public string[] ResultMeshCodes { get; set; }
        public Extent ResultExtent { get; set; }
        public PackageToLodDict AvailablePackageLods { get; set; }
        
        public void ReceiveResult(string[] areaMeshCodes, Extent extent, PackageToLodDict availablePackageLods)
        {
            ResultMeshCodes = areaMeshCodes;
            ResultExtent = extent;
            AvailablePackageLods = availablePackageLods;
        }
    }
}
