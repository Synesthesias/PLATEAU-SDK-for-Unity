using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlasticGui.Configuration.CloudEdition;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// GML IDとその属性情報の辞書です。
    /// </summary>
    public class GmlIdToAttributes
    {
        private Dictionary<string, CityObjectList.Attributes> data;
        
        /// <summary>
        /// 引数に含まれるGmlIDと属性情報をすべて取得して記憶したインスタンスを返します。
        /// </summary>
        public static GmlIdToAttributes ComposeFrom(IEnumerable<PLATEAUCityObjectGroup> cityObjGroups)
        {
            var ret = new GmlIdToAttributes();
            foreach(var cityObjs in cityObjGroups)
            {
                foreach (var cityObj in cityObjs.GetAllCityObjects())
                {
                    ret.Add(cityObj.GmlID, cityObj.AttributesMap);
                }
            }

            return ret;
        }

        private void Add(string gmlId, CityObjectList.Attributes attr)
        {
            data.Add(gmlId, attr);
        }
    }
}