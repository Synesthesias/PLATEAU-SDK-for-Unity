using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlasticGui.Configuration.CloudEdition;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// GML IDと、シリアライズ化された地物情報の辞書です。
    /// </summary>
    public class GmlIdToSerializedCityObj
    {
        private Dictionary<string, CityInfo.CityObjectList.CityObject> data;
        
        /// <summary>
        /// 引数に含まれるGmlIDと属性情報をすべて取得して記憶したインスタンスを返します。
        /// </summary>
        public static GmlIdToSerializedCityObj ComposeFrom(IEnumerable<PLATEAUCityObjectGroup> cityObjGroups)
        {
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
            data.Add(gmlId, serializedCityObj);
        }
    }
}