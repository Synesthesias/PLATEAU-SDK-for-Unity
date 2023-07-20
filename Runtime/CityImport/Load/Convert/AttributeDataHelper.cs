using System;
using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using UnityEngine;
using CityObject = PLATEAU.CityInfo.CityObject;

namespace PLATEAU.CityImport.Load.Convert
{
    /// <summary>
    /// PLATEAU の属性情報を Unity の GameObjectで扱えるようにするためのHelperクラスです
    /// </summary>
    internal class AttributeDataHelper
    {
        private readonly CityModel cityModel;
        private readonly MeshGranularity meshGranularity; 
        private readonly List<CityObjectID> indexList = new();
        private string id;
        private CityObjectIndex index;
        private string parant;

        class CityObjectID
        {
            public CityObjectIndex Index;
            public string AtomicID;
            public string PrimaryID;
        }

        public AttributeDataHelper(CityModel cityModel, MeshGranularity granularity)
        {
            this.cityModel = cityModel;
            meshGranularity = granularity;
        }

        public AttributeDataHelper(AttributeDataHelper attributeDataHelper) : this(attributeDataHelper.cityModel, attributeDataHelper.meshGranularity) { }

        public void SetId(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// UV情報から地物IDを取得し保持します
        /// </summary>
        public void SetCityObjectList(CityObjectList cityObjectList)
        {
            indexList.Clear();
            foreach (var key in cityObjectList.GetAllKeys())
            {
                var atomicGmlID = cityObjectList.GetAtomicID(key);
                var primaryGmlID = cityObjectList.GetPrimaryID(key.PrimaryIndex);

                if (meshGranularity == MeshGranularity.PerCityModelArea ||
                    (meshGranularity == MeshGranularity.PerPrimaryFeatureObject && primaryGmlID == id))
                        indexList.Add(new CityObjectID { Index = key, AtomicID = atomicGmlID, PrimaryID = primaryGmlID});
                        
                if (meshGranularity == MeshGranularity.PerAtomicFeatureObject && atomicGmlID == id)
                    parant = primaryGmlID;
            }
            index =  cityObjectList.GetCityObjectIndex(id);         
        }

        /// <summary>
        /// 各CityObjectの属性情報を取得してシリアライズ可能なデータに変換します
        /// </summary>
        public CityObject GetSerializableCityObject()
        {
            if (meshGranularity == MeshGranularity.PerCityModelArea)
                return GetSerializableCityObjectForArea();

            CityObject cityObjSer = new CityObject();

            if (!string.IsNullOrEmpty(parant))
                cityObjSer.parent = parant;

            var cityObj = GetCityObjectById(this.id);
            if (cityObj != null)
            {
                var ser = CityObjectSerializableConvert.FromCityGMLCityObject(cityObj, index);
                foreach (var id in indexList)
                {
                    if (id.PrimaryID == id.AtomicID) continue;
                    var childCityObj = GetCityObjectById(id.AtomicID);
                    if (childCityObj == null) continue;
                    ser.Children.Add(CityObjectSerializableConvert.FromCityGMLCityObject(childCityObj, id.Index));
                }
                cityObjSer.cityObjects.Add(ser);
            }
            return cityObjSer;
        }

        //地域単位結合モデルの場合のシリアライズ可能なデータへの変換です
        public CityObject GetSerializableCityObjectForArea()
        {
            CityObject cityObjSer = new CityObject();
            List<string> cityObjList = new List<string>();
            Dictionary<string, List<CityObjectID>> chidrenMap = new Dictionary<string, List<CityObjectID>>();

            foreach (var id in indexList)
            {
                if (string.IsNullOrEmpty(id.PrimaryID))
                    cityObjList.Add(id.AtomicID);
                else
                {
                    if (chidrenMap.ContainsKey(id.PrimaryID))
                        chidrenMap[id.PrimaryID].Add(id);
                    else
                        chidrenMap.Add(id.PrimaryID, new List<CityObjectID> {id});
                }
            }

            foreach (var id in indexList)
            {
                var cityObj = GetCityObjectById(id.AtomicID);
                if (cityObj == null) continue;
                var ser = CityObjectSerializableConvert.FromCityGMLCityObject(cityObj, id.Index);
                if (!chidrenMap.ContainsKey(id.AtomicID)) continue;
                var childrenId = chidrenMap[id.AtomicID];
                foreach (var c in childrenId)
                {
                    if (c.PrimaryID == c.AtomicID) continue;
                    var childCityObj = GetCityObjectById(c.AtomicID);
                    if (childCityObj == null) continue;
                    ser.Children.Add(CityObjectSerializableConvert.FromCityGMLCityObject(childCityObj, c.Index));
                }
                cityObjSer.cityObjects.Add(ser);
            }
            return cityObjSer;
        }

        public CityGML.CityObject GetCityObjectById(string id)
        {
            try
            {
                return cityModel.GetCityObjectById(id);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
            return null;
        }
    }
}
