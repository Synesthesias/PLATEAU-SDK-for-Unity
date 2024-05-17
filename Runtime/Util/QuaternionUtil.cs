using UnityEngine;

namespace PLATEAU.Util
{
    public static class QuaternionUtil
    {
        /// <summary> Quaternionの各要素がNaNもしくはInfinityかどうかチェック </summary>
        public static bool IsNaNOrInfinity(this Quaternion q)
        {
            bool ret =
                float.IsNaN(q.x) || float.IsNaN(q.y) ||
                float.IsNaN(q.z) || float.IsNaN(q.w) ||
                float.IsInfinity(q.x) || float.IsInfinity(q.y) ||
                float.IsInfinity(q.z) || float.IsInfinity(q.w);

            return ret;
        }
    }

}