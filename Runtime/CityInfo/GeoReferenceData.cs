using System;
using PLATEAU.Geometries;

namespace PLATEAU.CityInfo
{
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
