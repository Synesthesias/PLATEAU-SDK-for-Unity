using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// <see cref="MAExecutor"/>が、どのゲームオブジェクトを対象とするかを決めます。
    /// </summary>
    public interface IMACondition
    {
        public bool ShouldDeconstruct(Transform trans);
        public bool ShouldConstruct(Transform tarns);
    }

    /// <summary>
    /// シンプルな条件で、目標粒度が現在よりも細かければ分解、粗ければ結合する条件です。
    /// </summary>
    public class MAConditionSimple : IMACondition
    {
        private readonly MAGranularity dstGranularity;

        public MAConditionSimple(MAGranularity dstGranularity)
        {
            this.dstGranularity = dstGranularity;
        }
        
        public bool ShouldDeconstruct(Transform trans)
        {
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            var srcGranularity = cityObjGroup.Granularity;
            if (dstGranularity.ToNativeGranularity() >= srcGranularity) return false;
            return true;
        }

        public bool ShouldConstruct(Transform trans)
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