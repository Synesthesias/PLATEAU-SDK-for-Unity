using PLATEAU.CityInfo;
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
            var srcGranularity = cityObjGroup.Granularity;
            if (dstGranularity.ToNativeGranularity() >= srcGranularity) return false;
            return true;
        }

        public bool ShouldConstruct(Transform trans, MAGranularity dstGranularity)
        {
            // 上のメソッドとほぼ同じ
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            var srcGranularity = cityObjGroup.Granularity;
            if (dstGranularity.ToNativeGranularity() <= srcGranularity) return false;
            return true;
        }
    }

}