using System;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using UnityEngine;

namespace PLATEAU.CityInfo
{
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


        /// <summary>
        /// Unityでシリアライズ化して保存できるようにした <see cref="GeoReference"/> のデータです。
        /// </summary>
        [Serializable]
        public struct GeoReferenceData
        {
            public double referencePointX;
            public double referencePointY;
            public double referencePointZ;
            public float unitScale;
            public CoordinateSystem coordinateSystem;
            public int zoneID;

            public GeoReferenceData(double referencePointX, double referencePointY, double referencePointZ, float unitScale, CoordinateSystem coordinateSystem, int zoneID)
            {
                this.referencePointX = referencePointX;
                this.referencePointY = referencePointY;
                this.referencePointZ = referencePointZ;
                this.unitScale = unitScale;
                this.coordinateSystem = coordinateSystem;
                this.zoneID = zoneID;
            }
        }
    }
}
