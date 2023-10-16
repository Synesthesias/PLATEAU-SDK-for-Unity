using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using PlasticGui.Configuration.CloudEdition;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// GML IDと、シリアライズ化された地物情報の辞書です。
    /// </summary>
    public class GmlIdToSerializedCityObj
    {
        private Dictionary<string, CityInfo.CityObjectList.CityObject> data = new Dictionary<string, CityObjectList.CityObject>();
        
        /// <summary>
        /// 引数に含まれるGmlIDと属性情報をすべて取得して記憶したインスタンスを返します。
        /// 子の属性情報も再帰的に取得します。
        /// </summary>
        public static GmlIdToSerializedCityObj ComposeFrom(IEnumerable<GameObject> srcGameObjs)
        {
            var cityObjGroups = new List<PLATEAUCityObjectGroup>();
            var queue = new Queue<Transform>(srcGameObjs.Select(obj => obj.transform));
            while (queue.Count > 0)
            {
                var trans = queue.Dequeue();
                // if (!trans.gameObject.activeInHierarchy) continue; // 非アクティブはスキップします
                var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cityObjGroup != null)
                {
                    cityObjGroups.Add(cityObjGroup);
                }

                for (int i = 0; i < trans.childCount; i++)
                {
                    queue.Enqueue(trans.GetChild(i));
                }
            }
            
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

        private void Add(string gmlId, CityInfo.CityObjectList.CityObject serializedCityObj)
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