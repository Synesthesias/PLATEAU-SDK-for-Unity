using PLATEAU.CityInfo;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// 用途：モデル修正において、変換前の<see cref="PLATEAUInstancedCityModel"/>覚えておいて変換後にコピーするために利用します。
    /// <see cref="PLATEAUInstancedCityModel"/>と、ゲームオブジェクト名の辞書を覚えておきます。
    /// 参照： <see cref="NonLibDataHolder"/>
    /// </summary>
    public class InstancedCityModelDict : INonLibData
    {
        /// <summary>
        /// ゲームオブジェクト名とPLATEAUInstancedCityModelを紐つけます。
        /// </summary>
        private Dictionary<NonLibKeyName, PLATEAUInstancedCityModel> data = new();

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
                    data.Add(new NonLibKeyName(trans, srcTransforms.Get.ToArray()), cityModel);
                    return NextSearchFlow.Continue;
                });
        }
        
        /// <summary>
        /// 記憶した<see cref="PLATEAUInstancedCityModel"/>を復元します。
        /// 復元先は、<paramref name="rootTransforms"/>とその子を探し、名前が一致した箇所で復元します。
        /// </summary>
        public void RestoreTo(UniqueParentTransformList rootTransforms)
        {
            var remaining = new Dictionary<NonLibKeyName, PLATEAUInstancedCityModel>(data);
            rootTransforms.BfsExec(
                trans =>
                {
                    var key = new NonLibKeyName(trans, rootTransforms.Get.ToArray());
                    if (!remaining.ContainsKey(key)) return NextSearchFlow.Continue;
                    var newModel = trans.gameObject.AddComponent<PLATEAUInstancedCityModel>();
                    newModel.CopyFrom(remaining[key]);
                    remaining.Remove(key);
                    if (remaining.Count == 0) return NextSearchFlow.Abort;
                    return NextSearchFlow.Continue;
                });
            
        }
    }
}