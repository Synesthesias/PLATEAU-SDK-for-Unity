using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Native;

namespace PLATEAU.Tests.EditModeTests.TestDoubles
{
    internal class DummyAreaSelectResultReceiver : IAreaSelectResultReceiver
    {
        public AreaSelectResult AreaSelectResult { get; private set; }
        
        public void ReceiveResult(AreaSelectResult areaSelectResult)
        {
            AreaSelectResult = areaSelectResult;
        }
    }
}
