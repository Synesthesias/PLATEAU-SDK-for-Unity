using PLATEAU.Geometries;
using PLATEAU.Native;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// 座標変換に関するユーティリティです。
    /// ・PlateauのネイティブDLLで扱う Vector と Unity の Vector を変換します。
    /// ・Unity向けの座標変換インスタンスを提供します。
    /// </summary>
    internal static class CoordinatesConvertUtil
    {
        public static Vector3 ToUnityVector(this PlateauVector3d plateau3d)
        {
            return new Vector3((float)plateau3d.X, (float)plateau3d.Y, (float)plateau3d.Z);
        }

        public static PlateauVector3d ToPlateauVector(this Vector3 unity3d)
        {
            return new PlateauVector3d(unity3d.x, unity3d.y, unity3d.z);
        }

        /// <summary>
        /// Unity向けの座標変換インスタンスを new して返します。
        /// </summary>
        public static GeoReference UnityStandardGeoReference(int coordinateZoneID)
        {
            return GeoReference.Create(new PlateauVector3d(0, 0, 0),
                1.0f, CoordinateSystem.EUN, coordinateZoneID);
        }
    }
}

