using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMapInfo
{
    public class CityMapInfo : ScriptableObject
    {
        // TODO 未実装
        public IdToGmlTable idToGmlTable; 
        public Vector3 ReferencePoint;
        public int MaxLod;
        public MeshGranularity MeshGranularity;
        public string[] MeshPaths;

    }
}