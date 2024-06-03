using PLATEAU.CityAdjust.NonLibData;
using System.Collections.Generic;
using PLATEAU.CityInfo;
using PLATEAU.Util;

namespace PLATEAU.CityAdjust.NonLibDataHolder
{
    /// <summary>
    /// GML IDと、シリアライズ化された地物情報の辞書です。
    /// 用途：分割結合機能において、変換前の属性情報を覚えておいて変換後に適用するために利用します。
    /// 参照： <see cref="NonLibDataHolder"/>
    /// </summary>
    public class GmlIdToSerializedCityObj : INonLibData
    {
        private Dictionary<string, CityObjectList.CityObject> data = new ();
        
        /// <summary>
        /// 引数に含まれるGmlIDと属性情報をすべて取得して記憶したインスタンスを返します。
        /// 子の属性情報も再帰的に取得します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList srcTransforms)
        {
            var cityObjGroups = new List<PLATEAUCityObjectGroup>();
            srcTransforms.BfsExec(
                trans =>
                {
                    var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
                    if (cityObjGroup == null) return NextSearchFlow.Continue;

                    cityObjGroups.Add(cityObjGroup);
                    return NextSearchFlow.Continue;
                });
            
            foreach(var cityObjs in cityObjGroups)
            {
                foreach (var cityObj in cityObjs.GetAllCityObjects())
                {
                    Add(cityObj.GmlID, cityObj);
                }
            }

        }

        private void Add(string gmlId, CityObjectList.CityObject serializedCityObj)
        {
            if (data.ContainsKey(gmlId))
            {
                var attributes = serializedCityObj.AttributesMap;
                data[gmlId].AttributesMap.AddAttributes(attributes);
            }
            else
            {
                data.Add(gmlId, serializedCityObj);
            }
        }

        public bool TryGet(string gmlID, out CityObjectList.CityObject outSerializedCityObj)
        {
            if (data.TryGetValue(gmlID, out outSerializedCityObj))
            {
                return true;
            }
            outSerializedCityObj = null;
            return false;
        }

        /// <summary>
        /// 何もしません。
        /// 属性情報の復元手順は他の<see cref="INonLibData"/>とは異なっており、
        /// <see cref="RestoreTo"/>の代わりに
        /// <see cref="AttributeDataHelper"/>のnew時に渡す
        /// <see cref="SerializedCityObjectGetterFromDict"/>の引数に本クラスを渡すことで復元します。
        /// </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
        }
    }
}