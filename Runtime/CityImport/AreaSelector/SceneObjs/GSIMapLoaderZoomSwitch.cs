using System.Collections.Generic;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class GSIMapLoaderZoomSwitch
    {
        private Dictionary<int, GSIMapLoader> zoomLevelToMapLoader = new Dictionary<int, GSIMapLoader>();
        private GeoReference geoReference;
        

        public GSIMapLoaderZoomSwitch(GeoReference geoReference)
        {
            this.geoReference = geoReference;
        }
        
        
    }
}
