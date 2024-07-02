using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using System.Collections.Generic;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// <see cref="PLATEAUInstancedCityModel"/>と、ゲームオブジェクト名の辞書です。
    /// 用途：モデル修正において、変換前の都市情報を覚えておいて変換後に適用するために利用します。
    /// 参照： <see cref="NonLibDataHolder"/>
    /// </summary>
    public class InstancedCityModelDict : INonLibData
    {
        /// <summary>
        /// ゲームオブジェクト名とPLATEAUInstancedCityModelを紐つけます。
        /// </summary>
        private Dictionary<string, PLATEAUInstancedCityModel> data = new();

        /// <summary>
        /// <paramref name="srcGameObjs"/> とその子に含まれる<see cref="PLATEAUInstancedCityModel"/>を
        /// 記憶したインスタンスを返します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList srcTransforms)
        {
            srcTransforms.BfsExec(
                trans =>
                {
                    var cityModel = trans.GetComponent<PLATEAUInstancedCityModel>();
                    if (cityModel == null) return NextSearchFlow.Continue;
                    data.Add(trans.name, cityModel);
                    return NextSearchFlow.Continue;
                });
        }
        
        /// <summary>
        /// 記憶した<see cref="PLATEAUInstancedCityModel"/>を復元します。
        /// 復元先は、<paramref name="rootTransforms"/>とその子を探し、名前が一致した箇所で復元します。
        /// </summary>
        public void RestoreTo(UniqueParentTransformList rootTransforms)
        {
            var remaining = new Dictionary<string, PLATEAUInstancedCityModel>(data);
            rootTransforms.BfsExec(
                trans =>
                {
                    string name = trans.name;
                    if (!remaining.ContainsKey(name)) return NextSearchFlow.Continue;
                    var newModel = trans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
                    newModel.CopyFrom(remaining[name]);
                    remaining.Remove(name);
                    if (remaining.Count == 0) return NextSearchFlow.Abort;
                    return NextSearchFlow.Continue;
                });
            
        }
    }
}