using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Util
{
    public static class VectorConverter
    {
        public static Vector3 ToUnityVector(PlateauVector3d plateau3d)
        {
            return new Vector3((float)plateau3d.X, (float)plateau3d.Y, (float)plateau3d.Z);
        }

        public static PlateauVector3d ToPlateauVector(Vector3 unity3d)
        {
            return new PlateauVector3d(unity3d.x, unity3d.y, unity3d.z);
        }
    }
}

