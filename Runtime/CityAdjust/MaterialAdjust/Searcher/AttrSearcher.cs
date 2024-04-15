using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// 属性情報を基準にマテリアル分けのキーを検索します。
    /// </summary>
    internal class AttrSearcher : ISearcher<string>
    {

        private SearchArgByArr searchArg;
        public AttrSearcher(SearchArgByArr searchArg)
        {
            this.searchArg = searchArg;
        }
        
        /// <summary>
        /// <paramref name="targets"/>とその子について、
        /// 属性情報のキー<paramref name="attrKey"/>に対応する属性値として何が存在するかを検索します。
        /// 入れ子になっている属性情報を検索したい場合は、キーをスラッシュ"/"で区切ってください。
        /// なお、<paramref name="attrKey"/>に対応する値が入れ子である（文字列として表現不可能な型である）場合は検索対象外です。
        /// </summary>
        public string[] Search()
        {
            HashSet<string> found = new();
            foreach (var target in searchArg.TargetObjs.Select(obj => obj.transform))
            {
                var cityObjGroups = target.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var cityObjGroup in cityObjGroups)
                {
                    var mesh = cityObjGroup.GetComponent<MeshFilter>();
                    if (mesh == null) continue;
                    var cityObjs = cityObjGroup.GetAllCityObjects();
                    foreach (var cityObj in cityObjs)
                    {
                        var attr = cityObj.AttributesMap;
                        if (!attr.TryGetValueWithSlash(searchArg.AttrKey, out var val)) continue;
                        if (val.Type == AttributeType.AttributeSet) continue;
                        var valStr = val.StringValue;
                        if (string.IsNullOrEmpty(valStr)) continue;
                        found.Add(valStr);
                    }
                }
            }
            return found.ToArray();
        }
    }
}