using PLATEAU.Runtime.CityLoader.AreaSelector.Import.AreaSelect;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.Import
{
    public class Importer
    {
        public void Import(string sourcePathBeforeImport)
        {
            var areaSelector = new AreaSelectorStarter();
            areaSelector.Start();
        }
    }
}
