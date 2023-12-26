using System;
using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using UnityEngine;
using CityObjectList = PLATEAU.CityInfo.CityObjectList;
using PLATEAUCityObjectList = PLATEAU.PolygonMesh.CityObjectList;

namespace PLATEAU.CityImport.Import.Convert
{
    /// <summary>
    /// PLATEAU の属性情報を Unity の GameObjectで扱えるようにするためのHelperクラスです
    /// </summary>
    internal class AttributeDataHelper : IDisposable
    {
        private readonly MeshGranularity meshGranularity; 
        private readonly List<CityObjectID> indexList = new();
        private readonly List<string> outsideChildrenList = new();
        private ISerializedCityObjectGetter serializedCityObjectGetter;
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

        public AttributeDataHelper(ISerializedCityObjectGetter serializedCityObjectGetter, MeshGranularity granularity, bool doSetAttrInfo)
        {
            meshGranularity = granularity;
            this.doSetAttrInfo = doSetAttrInfo;
            this.serializedCityObjectGetter = serializedCityObjectGetter;
        }

        public AttributeDataHelper Copy()
        {
            return new AttributeDataHelper(serializedCityObjectGetter, meshGranularity, doSetAttrInfo);
        }

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
            var allKeys = cityObjectList.GetAllKeys();
            foreach (var key in allKeys)
            {
                var atomicGmlID = cityObjectList.GetAtomicID(key);
                var primaryGmlID = cityObjectList.GetPrimaryID(key.PrimaryIndex);

                bool shouldAddIDWhenAreaGranularity = meshGranularity == MeshGranularity.PerCityModelArea;
                bool shouldAddIDWhenPrimaryGranularity =
                    meshGranularity == MeshGranularity.PerPrimaryFeatureObject &&
                    (primaryGmlID == id /*|| // 主要地物単位のインポート時
                      primaryGmlID == null*/); // 主要地物単位へ結合分解時
                bool shouldAddID = shouldAddIDWhenAreaGranularity || shouldAddIDWhenPrimaryGranularity;
                if (shouldAddID)
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
            switch (meshGranularity)
            {
                case MeshGranularity.PerCityModelArea:
                    return GetSerializableCityObjectForArea();
                case MeshGranularity.PerPrimaryFeatureObject:
                case MeshGranularity.PerAtomicFeatureObject:
                    return GetSerializableCityObjectForAtomicOrPrimary();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 最小地物単位または主要地物単位のモデルの場合の、シリアライズ可能なデータへの変換です。
        /// </summary>
        private CityObjectList GetSerializableCityObjectForAtomicOrPrimary()
        {
            var cityObjSer = serializedCityObjectGetter.GetByID(this.id, index);
            if (cityObjSer == null) return null;
            cityObjSer.CityObjectIndex = new int[]{index.PrimaryIndex, index.AtomicIndex}; // 分割結合時に必要
            CityObjectList cityObjList = new CityObjectList();

            if (!string.IsNullOrEmpty(this.parent))
                cityObjList.outsideParent = this.parent;
            
            foreach (var id in indexList)
            {
                if (id.PrimaryID == id.AtomicID) continue;
                var childCityObj = serializedCityObjectGetter.GetByID(id.AtomicID, id.Index);
                if (childCityObj == null) continue;
                childCityObj.CityObjectIndex = new int[]{id.Index.PrimaryIndex, id.Index.AtomicIndex}; // 分割結合時に必要
                cityObjSer.Children.Add(childCityObj);
            }
            cityObjList.rootCityObjects.Add(cityObjSer);
            cityObjList.outsideChildren = outsideChildrenList;
            return cityObjList;
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
                var cityObj = serializedCityObjectGetter.GetByID(id.AtomicID, id.Index);
                if (cityObj == null) continue;
                
                // TODO 下の処理は GetByIDメソッド内にまとめられそう？
                cityObj.CityObjectIndex = new int[]{id.Index.PrimaryIndex, id.Index.AtomicIndex}; // 分割結合時に必要
                
                // var ser = CityObjectSerializableConvert.FromCityGMLCityObject(cityObj, id.Index);
                if (!chidrenMap.ContainsKey(id.AtomicID)) continue;
                var childrenId = chidrenMap[id.AtomicID];
                foreach (var c in childrenId)
                {
                    if (c.PrimaryID == c.AtomicID) continue;
                    var childCityObj = serializedCityObjectGetter.GetByID(c.AtomicID, c.Index);
                    if (childCityObj == null) continue;
                    childCityObj.CityObjectIndex = new int[]{c.Index.PrimaryIndex, c.Index.AtomicIndex}; // 分割結合時に必要
                    cityObj.Children.Add(childCityObj);
                }
                cityObjSer.rootCityObjects.Add(cityObj);
            }   
            return cityObjSer;
        }

        public void Dispose()
        {
            serializedCityObjectGetter.Dispose();
        }
    }

    internal class SerializedCityObjectGetterFromCityModel : ISerializedCityObjectGetter
    {
        private CityModel cityModel;

        public SerializedCityObjectGetterFromCityModel(CityModel cityModel)
        {
            this.cityModel = cityModel;
        }
        
        public CityObjectList.CityObject GetByID(string gmlID, CityObjectIndex? index)
        {
            var cityObj = GetByIDInner(gmlID);
            if (cityObj == null)
                return null;

            var ser = CityObjectSerializableConvert.FromCityGMLCityObject(cityObj, index);
            return ser;
        }

        public void Dispose()
        {
            cityModel = null;
        }
        
        private CityGML.CityObject GetByIDInner(string id)
        {
            try
            {
                return cityModel.GetCityObjectById(id);
            }
            catch (KeyNotFoundException ex)
            {
                Debug.LogWarning($"{ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }
    }

    internal interface ISerializedCityObjectGetter
    {
        CityObjectList.CityObject GetByID(string gmlID, CityObjectIndex? index);
        void Dispose();
    }
}
