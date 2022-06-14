using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.CityModelImportWindow;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMapMetaData
{
    public class CityMapMetaData : ScriptableObject
    {
        // TODO 未実装
        public IdToGmlTable idToGmlTable = new IdToGmlTable();
        public CityModelImportConfig cityModelImportConfig = new CityModelImportConfig();
        public string[] meshPaths;
        
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

        public void ClearData()
        {
            this.idToGmlTable?.Clear();
            cityModelImportConfig.referencePoint = Vector3.zero;
            // MaxLod = 0;
            cityModelImportConfig.meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
            this.meshPaths = new string[]{};
        }
    }
}