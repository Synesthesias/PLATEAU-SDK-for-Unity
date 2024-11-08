using System;
using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using System.Linq;
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

        private ConvertGranularity granularity;
        /// <summary>
        /// 注目オブジェクトの粒度設定です。
        /// 例： 最小地物でインポートした場合、最小地物の親は主要地物になります。
        /// ここで主要地物に着目したとき、granularityは最小地物で、currentGranularityは主要地物です。
        /// </summary>
        public ConvertGranularity CurrentGranularity { get; private set; }
        
        private readonly List<CityObjectID> indexList = new();
        private readonly List<string> outsideChildrenList = new();
        private ISerializedCityObjectGetter serializedCityObjectGetter;
        private Node currentNode;
        private CityObjectIndex index;
        private string parentGmlID;
        private readonly bool doSetAttrInfo;

        class CityObjectID
        {
            public CityObjectIndex Index;
            public string AtomicID;
            public string PrimaryID;
        }

        public AttributeDataHelper(ISerializedCityObjectGetter serializedCityObjectGetter, bool doSetAttrInfo)
        {
            this.doSetAttrInfo = doSetAttrInfo;
            this.serializedCityObjectGetter = serializedCityObjectGetter;
        }

        public AttributeDataHelper Copy()
        {
            return new AttributeDataHelper(serializedCityObjectGetter, doSetAttrInfo);
        }

        public void SetCurrentNode(Node node)
        {
            currentNode = node;
        }
        
        private string NodeName => currentNode?.Name;

        /// <summary>
        /// UV情報から地物IDを取得し保持します
        /// </summary>
        public void SetTargetCityObjList(PLATEAUCityObjectList cityObjectList)
        {
            if (!doSetAttrInfo) return;
            granularity = GuessGranularity(cityObjectList);
            indexList.Clear();
            var allKeys = cityObjectList.GetAllKeys();
            foreach (var key in allKeys)
            {
                var atomicGmlID = cityObjectList.GetAtomicID(key);
                var primaryGmlID = cityObjectList.GetPrimaryID(key.PrimaryIndex);

                bool shouldAddIDWhenAreaGranularity = granularity == ConvertGranularity.PerCityModelArea;
                bool shouldAddIDWhenPrimaryGranularity =
                    granularity == ConvertGranularity.PerPrimaryFeatureObject &&
                    (primaryGmlID == NodeName /*|| // 主要地物単位のインポート時
                      primaryGmlID == null*/); // 主要地物単位へ結合分解時
                bool shouldAddID = shouldAddIDWhenAreaGranularity || shouldAddIDWhenPrimaryGranularity;
                if (shouldAddID)
                        indexList.Add(new CityObjectID { Index = key, AtomicID = atomicGmlID, PrimaryID = primaryGmlID});
                        
                if ((granularity == ConvertGranularity.PerAtomicFeatureObject || granularity == ConvertGranularity.MaterialInPrimary) && primaryGmlID != NodeName)
                    this.parentGmlID = primaryGmlID;
                
                // 最小地物でインポートしているけど、最小地物の親は主要地物としたいケースに対応します。
                CurrentGranularity =
                    granularity != ConvertGranularity.PerAtomicFeatureObject ? granularity :
                    (primaryGmlID == NodeName) ? ConvertGranularity.PerPrimaryFeatureObject :
                    ConvertGranularity.PerAtomicFeatureObject;

            }
            index =  cityObjectList.GetCityObjectIndex(NodeName);         
        }

        /// <summary>
        /// <see cref="PLATEAUCityObjectList"/>から粒度を推測します。
        /// </summary>
        private ConvertGranularity GuessGranularity(PLATEAUCityObjectList cityObjectList)
        {
            var keys = cityObjectList.GetAllKeys();
            HashSet<int> primaryIDs = new HashSet<int>();
            int atomicCount = 0;
            foreach (var key in keys)
            {
                if (key.AtomicIndex == -1)
                {
                    primaryIDs.Add(key.PrimaryIndex);
                    if (primaryIDs.Count >= 2) return ConvertGranularity.PerCityModelArea;
                }
                else
                {
                    atomicCount++;
                }
            }
            if(primaryIDs.Count == 0) return ConvertGranularity.PerAtomicFeatureObject; // あまり見ないケースですが一応
            
            // atomicなmeshにもprimaryのIDが入っているため以下でさらに判別します
            var primaryGmlId = cityObjectList.GetPrimaryID(primaryIDs.First());
            if (primaryGmlId == NodeName)
            {
                return ConvertGranularity.PerPrimaryFeatureObject;
            }
            else
            {
                return atomicCount <= 1 ? ConvertGranularity.PerAtomicFeatureObject : ConvertGranularity.MaterialInPrimary;
            }
        }

        /// <summary>
        /// OutsideChildrenを取得し保持します
        /// </summary>
        public void AddOutsideChildren(string childId)
        {
            if (!doSetAttrInfo) return;
            if (granularity == ConvertGranularity.PerAtomicFeatureObject &&
                !string.IsNullOrEmpty(childId) &&
                !outsideChildrenList.Contains(childId))
            {
                outsideChildrenList.Add(childId);
                // 最小地物でインポートしている場合でも、最小地物の親は主要地物とします。
                // 最小地物でインポートしていて、OutsideChildrenが存在するということは主要地物のはずです。
                CurrentGranularity = ConvertGranularity.PerPrimaryFeatureObject;
            }
            
        }
        

        /// <summary>
        /// 各CityObjectの属性情報を取得してシリアライズ可能なデータに変換します
        /// CityObjectが存在しない場合はnullを返します
        /// </summary>
        public CityObjectList GetSerializableCityObject()
        {
            if (!doSetAttrInfo) return null;
            switch (granularity)
            {
                case ConvertGranularity.PerCityModelArea:
                    return GetSerializableCityObjectForArea();
                case ConvertGranularity.PerPrimaryFeatureObject:
                case ConvertGranularity.PerAtomicFeatureObject:
                case ConvertGranularity.MaterialInPrimary:
                    return GetSerializableCityObjectForNonArea();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 最小地物単位または主要地物単位のモデルの場合の、シリアライズ可能なデータへの変換です。
        /// </summary>
        private CityObjectList GetSerializableCityObjectForNonArea()
        {
            var cityObjSerArr = serializedCityObjectGetter.GetDstCityObjectsByNode(currentNode, index, parentGmlID);
            if (cityObjSerArr == null || cityObjSerArr.Length == 0)
            {
                return null;
            }

            CityObjectList cityObjList = new CityObjectList();
            foreach (var cityObjSer in cityObjSerArr)
            {
                cityObjSer.CityObjectIndex = GetCurrentCityObjectIndex(index, currentNode, cityObjSer.GmlID).ToArray(); // 分割結合時に必要

                if (!string.IsNullOrEmpty(this.parentGmlID))
                    cityObjList.outsideParent = this.parentGmlID;

                for (int i = 0; i < indexList.Count; i++)
                {
                    var id = indexList[i];
                    if (id.PrimaryID == id.AtomicID) continue;
                    var childCityObj = serializedCityObjectGetter.GetDstCityObjectByGmlID(id.AtomicID, id.Index);
                    if (childCityObj == null) continue;
                    childCityObj.CityObjectIndex = GetCurrentCityObjectIndex(id.Index, currentNode, id.AtomicID).ToArray(); // 分割結合時に必要
                    cityObjSer.Children.Add(childCityObj);
                }
                cityObjList.rootCityObjects.Add(cityObjSer);
            }
            
            cityObjList.outsideChildren = outsideChildrenList;
            return cityObjList;
        }

        private CityObjectIndex GetCurrentCityObjectIndex(CityObjectIndex id, Node node, string gmlID)
        {

            var currentID = new CityObjectIndex(id.PrimaryIndex, id.AtomicIndex);
            return currentID;
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
            Dictionary<string, List<CityObjectID>> chidrenMap = new Dictionary<string, List<CityObjectID>>();

            foreach (var id in indexList)
            {
                if (chidrenMap.ContainsKey(id.PrimaryID))
                    chidrenMap[id.PrimaryID].Add(id);
                else
                    chidrenMap.Add(id.PrimaryID, new List<CityObjectID> { id });
            }

            foreach (var id in indexList)
            {
                var cityObj = serializedCityObjectGetter.GetDstCityObjectByGmlID(id.AtomicID, id.Index);
                if (cityObj == null) continue;
                if (id.PrimaryID != id.AtomicID) continue; // まずは主要地物から見る
                
                // TODO 下の処理は GetByIDメソッド内にまとめられそう？
                cityObj.CityObjectIndex = GetCurrentCityObjectIndex(id.Index, currentNode, id.PrimaryID).ToArray(); // 分割結合時に必要
                
                // var ser = CityObjectSerializableConvert.FromCityGMLCityObject(cityObj, id.Index);
                if (!chidrenMap.ContainsKey(id.PrimaryID)) continue;
                var childrenId = chidrenMap[id.PrimaryID];
                foreach (var c in childrenId)
                {
                    // ここで最小地物を見ます（それ以外をスキップします）
                    if (c.PrimaryID == c.AtomicID) continue; 
                    if (c.PrimaryID != id.PrimaryID) continue;
                    
                    var childCityObj = serializedCityObjectGetter.GetDstCityObjectByGmlID(c.AtomicID, c.Index);
                    if (childCityObj == null) continue;
                    childCityObj.CityObjectIndex = new int[]{id.Index.PrimaryIndex, c.Index.AtomicIndex}; // 分割結合時に必要
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

    /// <summary>
    /// <see cref="CityModel"/>から<see cref="CityObject"/>を取得します。
    /// インポート時に利用します。
    /// </summary>
    internal class SerializedCityObjectGetterFromCityModel : ISerializedCityObjectGetter
    {
        private CityModel cityModel;

        public SerializedCityObjectGetterFromCityModel(CityModel cityModel)
        {
            this.cityModel = cityModel;
        }
        
        public CityObjectList.CityObject[] GetDstCityObjectsByNode(Node node, CityObjectIndex? index, string _)
        {
            var co = GetDstCityObjectByGmlID(node.Name, index);
            if (co == null) return null;
            return new[] { co };
        }
        
        public CityObjectList.CityObject GetDstCityObjectByGmlID(string gmlID, CityObjectIndex? index)
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
            catch (KeyNotFoundException )
            {
                Debug.Log($"skipping because id {id} is not existing.");
            }
            return null;
        }
    }

    /// <summary>
    /// <see cref="AttributeDataHelper"/>が、属性情報を設定するためにどうやってNode名から<see cref="CityObject"/>を得るかの違いを吸収します。
    /// </summary>
    internal interface ISerializedCityObjectGetter
    {
        CityObjectList.CityObject[] GetDstCityObjectsByNode(Node node, CityObjectIndex? index, string parentGmlID);
        CityObjectList.CityObject GetDstCityObjectByGmlID(string gmlID, CityObjectIndex? index);
        void Dispose();
    }
}
