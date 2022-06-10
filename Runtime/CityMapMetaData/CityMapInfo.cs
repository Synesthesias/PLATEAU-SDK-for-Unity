using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMapMetaData
{
    public class CityMapInfo : ScriptableObject
    {
        // TODO 未実装
        public IdToGmlTable idToGmlTable = new IdToGmlTable(); 
        public Vector3 ReferencePoint;
        public int MaxLod;
        public MeshGranularity MeshGranularity;
        public string[] MeshPaths;
        
        public bool DoGmlTableContainsKey(string cityObjId)
        {
            return this.idToGmlTable.ContainsKey(cityObjId);
        }

        public void AddToGmlTable(string cityObjId, string gmlName)
        {
            this.idToGmlTable.Add(cityObjId, gmlName);
        }

        public bool TryGetValueFromGmlTable(string cityObjId, out string gmlFileName)
        {
            return this.idToGmlTable.TryGetValue(cityObjId, out gmlFileName);
        }
    }
}