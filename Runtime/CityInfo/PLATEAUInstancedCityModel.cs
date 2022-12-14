using System;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using UnityEngine;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// GMLファイルからUnityにインポートした都市のルートGameObjectに付与されます。
    /// </summary>
    public class PLATEAUInstancedCityModel : MonoBehaviour
    {
        [SerializeField] private GeoReferenceData geoReferenceData;

        public GeoReference GeoReference
        {
            get
            {
                var grd = this.geoReferenceData;
                var gr =
                    new GeoReference(
                        new PlateauVector3d(
                            grd.referencePointX,
                            grd.referencePointY,
                            grd.referencePointZ
                        ),
                        grd.unitScale, grd.coordinateSystem, grd.zoneID
                    );
                return gr;
            }
            set
            {
                var point = value.ReferencePoint;
                var data =
                    new GeoReferenceData(
                        point.X, point.Y, point.Z, value.UnitScale,
                        value.CoordinateSystem, value.ZoneID
                    );
                this.geoReferenceData = data;
            }
        }
    }
}
