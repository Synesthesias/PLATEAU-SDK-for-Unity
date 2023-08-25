using System;
using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using UnityEngine;
using CityObjectList = PLATEAU.CityInfo.CityObjectList;
using PLATEAUCityObjectList = PLATEAU.PolygonMesh.CityObjectList;

namespace PLATEAU.CityImport.Load.Convert
{
    /// <summary>
    /// PLATEAU の属性情報を Unity の GameObjectで扱えるようにするためのHelperクラスです
    /// </summary>
    internal class AttributeDataHelper : IDisposable
    {
        private readonly MeshGranularity meshGranularity; 
        private readonly List<CityObjectID> indexList = new();
        private readonly List<string> outsideChildrenList = new();
        private CityModel cityModel;
        private string id;
        private CityObjectIndex index;
        private string parent;
        private readonly bool doSetAttrInfo;

        class CityObjectID
        {
            public CityObjectIndex Index;
            public string AtomicID;
            public string PrimaryID;
        }

        public AttributeDataHelper(CityModel cityModel, MeshGranularity granularity, bool doSetAttrInfo)
        {
            this.cityModel = cityModel;
            meshGranularity = granularity;
            this.doSetAttrInfo = doSetAttrInfo;
        }

        public AttributeDataHelper(AttributeDataHelper attributeDataHelper) : this(attributeDataHelper.cityModel, attributeDataHelper.meshGranularity, attributeDataHelper.doSetAttrInfo) { }

        public void SetId(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// UV情報から地物IDを取得し保持します
        /// </summary>
        public void SetCityObjectList(PLATEAUCityObjectList cityObjectList)
        {
            if (!doSetAttrInfo) return;
            indexList.Clear();
            foreach (var key in cityObjectList.GetAllKeys())
            {
                var atomicGmlID = cityObjectList.GetAtomicID(key);
                var primaryGmlID = cityObjectList.GetPrimaryID(key.PrimaryIndex);

                if (meshGranularity == MeshGranularity.PerCityModelArea ||
                    (meshGranularity == MeshGranularity.PerPrimaryFeatureObject && primaryGmlID == id))
                        indexList.Add(new CityObjectID { Index = key, AtomicID = atomicGmlID, PrimaryID = primaryGmlID});
                        
                if (meshGranularity == MeshGranularity.PerAtomicFeatureObject && atomicGmlID == id)
                    this.parent = primaryGmlID;
            }
            index =  cityObjectList.GetCityObjectIndex(id);         
        }

        /// <summary>
        /// OutsideChildrenを取得し保持します
        /// </summary>
        public void AddOutsideChildren(string childId)
        {
            if (!doSetAttrInfo) return;
            if (meshGranularity == MeshGranularity.PerAtomicFeatureObject && 
                !string.IsNullOrEmpty(childId) && 
                !outsideChildrenList.Contains(childId))
                outsideChildrenList.Add(childId);
        }

        /// <summary>
        /// 各CityObjectの属性情報を取得してシリアライズ可能なデータに変換します
        /// CityObjectが存在しない場合はnullを返します
        /// </summary>
        public CityObjectList GetSerializableCityObject()
        {
            if (!doSetAttrInfo) return null;
            if (meshGranularity == MeshGranularity.PerCityModelArea)
                return GetSerializableCityObjectForArea();

            var cityObj = GetCityObjectById(this.id);
            if (cityObj == null)
                return null;

            CityObjectList cityObjSer = new CityObjectList();

            if (!string.IsNullOrEmpty(this.parent))
                cityObjSer.outsideParent = this.parent;

            var ser = CityObjectSerializableConvert.FromCityGMLCityObject(cityObj, index);
            foreach (var id in indexList)
            {
                if (id.PrimaryID == id.AtomicID) continue;
                var childCityObj = GetCityObjectById(id.AtomicID);
                if (childCityObj == null) continue;
                ser.Children.Add(CityObjectSerializableConvert.FromCityGMLCityObject(childCityObj, id.Index));
            }
            cityObjSer.rootCityObjects.Add(ser);
            cityObjSer.outsideChildren = outsideChildrenList;
            return cityObjSer;
        }

        /// <summary>
        /// 地域単位結合モデルの場合のシリアライズ可能なデータへの変換です
        /// rootCityObjectsが空の場合はnullを返します
        /// </summary>
        /// <returns></returns>
        private CityObjectList GetSerializableCityObjectForArea()
        {
            if (indexList.Count <= 0) 
                return null;

            CityObjectList cityObjSer = new CityObjectList();
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
                cityObjSer.rootCityObjects.Add(ser);
            }   
            return cityObjSer;
        }

        private CityGML.CityObject GetCityObjectById(string id)
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

        public void Dispose()
        {
            cityModel = null;
        }
    }
}
