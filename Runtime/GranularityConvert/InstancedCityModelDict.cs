using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// <see cref="PLATEAUInstancedCityModel"/>と、ゲームオブジェクト名の辞書です。
    /// 用途：分割結合機能において、変換前の都市情報を覚えておいて変換後に適用するために利用します。
    /// 参照： <see cref="CityGranularityConverter"/>
    /// </summary>
    public class InstancedCityModelDict
    {
        /// <summary>
        /// ゲームオブジェクト名とPLATEAUInstancedCityModelを紐つけます。
        /// </summary>
        private Dictionary<string, PLATEAUInstancedCityModel> data = new();

        /// <summary>
        /// <paramref name="srcGameObjs"/> とその子に含まれる<see cref="PLATEAUInstancedCityModel"/>を
        /// 記憶したインスタンスを返します。
        /// </summary>
        public static InstancedCityModelDict ComposeFrom(IEnumerable<GameObject> srcGameObjs)
        {
            var dict = new InstancedCityModelDict();
            TransformBFS.Exec(srcGameObjs.Select(obj => obj.transform),
                trans =>
                {
                    var cityModel = trans.GetComponent<PLATEAUInstancedCityModel>();
                    if (cityModel == null) return true;
                    dict.data.Add(trans.name, cityModel);
                    return true;
                });
            return dict;
        }
        
        /// <summary>
        /// 記憶した<see cref="PLATEAUInstancedCityModel"/>を復元します。
        /// 復元先は、<paramref name="rootGameObjs"/>とその子を探し、名前が一致した箇所で復元します。
        /// </summary>
        public void Restore(IEnumerable<GameObject> rootGameObjs)
        {
            var remaining = new Dictionary<string, PLATEAUInstancedCityModel>(data);
            TransformBFS.Exec(rootGameObjs.Select(obj => obj.transform),
                trans =>
                {
                    string name = trans.name;
                    if (!remaining.ContainsKey(name)) return true;
                    var newModel = trans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
                    newModel.CopyFrom(remaining[name]);
                    remaining.Remove(name);
                    if (remaining.Count == 0) return false;
                    return true;
                });
            
        }
    }
}