using PLATEAU.Editor.PlateauWindow.Import.AreaSelect;

namespace PLATEAU.Editor.PlateauWindow.Import
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
