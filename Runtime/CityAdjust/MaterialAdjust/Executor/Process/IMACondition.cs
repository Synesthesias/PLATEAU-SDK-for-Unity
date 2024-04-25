using PLATEAU.CityInfo;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// <see cref="MAExecutor"/>が、どのゲームオブジェクトを対象とするかを決めます。
    /// </summary>
    public interface IMACondition
    {
        public bool ShouldDeconstruct(Transform trans, MAGranularity dstGranularity);
        public bool ShouldConstruct(Transform tarns, MAGranularity dstGranularity);
    }

    /// <summary>
    /// シンプルな条件で、目標粒度が現在よりも細かければ分解、粗ければ結合する条件です。
    /// </summary>
    public class MAConditionSimple : IMACondition
    {

        public MAConditionSimple()
        {
        }
        
        public bool ShouldDeconstruct(Transform trans, MAGranularity dstGranularity)
        {
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            if (trans.GetComponent<MeshFilter>() == null) return false;
            var srcGranularity = cityObjGroup.Granularity;
            return dstGranularity.ToNativeGranularity() < srcGranularity;
        }

        public bool ShouldConstruct(Transform trans, MAGranularity dstGranularity)
        {
            // 自身、または非再帰的な子に目標粒度よりも細かい粒度があるかチェックします
            bool hasLowerGranularityInChildren = false;
            for (int i = 0; i < trans.childCount; i++)
            {
                if (hasLowerGranularityInChildren) break;
                var child = trans.GetChild(i);
                var childCog = child.GetComponent<PLATEAUCityObjectGroup>();
                if (childCog == null) continue;
                hasLowerGranularityInChildren |= childCog.Granularity < dstGranularity.ToNativeGranularity();
            }
            
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            hasLowerGranularityInChildren |= cityObjGroup.Granularity < dstGranularity.ToNativeGranularity();
            
            return hasLowerGranularityInChildren;
        }
    }

}