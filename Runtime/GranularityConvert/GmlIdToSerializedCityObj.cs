using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// GML IDと、シリアライズ化された地物情報の辞書です。
    /// 用途：分割結合機能において、変換前の属性情報を覚えておいて変換後に適用するために利用します。
    /// 参照： <see cref="CityGranularityConverter"/>
    /// </summary>
    public class GmlIdToSerializedCityObj
    {
        private Dictionary<string, CityObjectList.CityObject> data = new ();
        
        /// <summary>
        /// 引数に含まれるGmlIDと属性情報をすべて取得して記憶したインスタンスを返します。
        /// 子の属性情報も再帰的に取得します。
        /// </summary>
        public static GmlIdToSerializedCityObj ComposeFrom(IEnumerable<GameObject> srcGameObjs)
        {
            var cityObjGroups = new List<PLATEAUCityObjectGroup>();
            TransformBFS.Exec(srcGameObjs.Select(obj => obj.transform),
                trans =>
                {
                    var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
                    if (cityObjGroup == null) return true;

                    cityObjGroups.Add(cityObjGroup);
                    return true;
                });
            
            var ret = new GmlIdToSerializedCityObj();
            foreach(var cityObjs in cityObjGroups)
            {
                foreach (var cityObj in cityObjs.GetAllCityObjects())
                {
                    ret.Add(cityObj.GmlID, cityObj);
                }
            }

            return ret;
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
    }
}