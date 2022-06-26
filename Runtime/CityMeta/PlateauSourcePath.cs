using System;

namespace PLATEAU.CityMeta
{
    [Serializable]
    public class PlateauSourcePath
    {
        public string udxFullPath;

        public PlateauSourcePath(string udxFullPath)
        {
            this.udxFullPath = udxFullPath;
        }
    }
}