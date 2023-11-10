using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// ゲームオブジェクトに含まれるCityObjectTypeを探し列挙します。
    /// </summary>
    internal class CityTypeSearcher
    {
        /// <summary>
        /// 引数とその子に含まれるCityObjectTypeを列挙します。
        /// </summary>
        public CityObjectType[] Search(IReadOnlyCollection<Transform> targets)
        {
            HashSet<CityObjectType> found = new();
            foreach(var target in targets)
            {
                var cityObjGroups = target.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var cityObjGroup in cityObjGroups)
                {
                    var cityObjs = cityObjGroup.GetAllCityObjects();
                    foreach (var cityObj in cityObjs)
                    {
                        found.Add(cityObj.CityObjectType);
                    }
                }
                
            }

            return found.ToArray();
        }
    }
}