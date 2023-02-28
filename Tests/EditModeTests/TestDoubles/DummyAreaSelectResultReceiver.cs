using PLATEAU.CityImport.AreaSelector;

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
