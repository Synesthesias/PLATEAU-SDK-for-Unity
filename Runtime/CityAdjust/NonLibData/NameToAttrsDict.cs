using PLATEAU.CityInfo;
using PLATEAU.Util;
using System.Linq;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// ゲームオブジェクト名と属性情報の辞書です。
    /// <see cref="NonLibDataHolder"/>が利用します。
    /// </summary>
    internal class NameToAttrsDict : INonLibData
    {
        private NonLibDictionary<PLATEAUCityObjectGroup> data = new();

        /// <summary>
        /// ゲームオブジェクトとその子から属性情報の辞書を構築します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                data.Add(trans, src.Get.ToArray(), cog); // nullでも追加します。
                return NextSearchFlow.Continue;
            });
        }

        /// <summary>
        /// <paramref name="target"/>とその子に対して、
        /// ゲームオブジェクト名を元に覚えておいた属性情報を復元します。
        /// </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                var srcAttr = data.GetNonRestoredAndMarkRestored(trans, target.Get.ToArray());
                if (srcAttr == null) return NextSearchFlow.Continue;
                var dstAttr = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (dstAttr == null)
                {
                    dstAttr = trans.gameObject.AddComponent<PLATEAUCityObjectGroup>();
                }

                dstAttr.Init(srcAttr.CityObjects, srcAttr.InfoForToolkits, srcAttr.Granularity, srcAttr.Lod);
                
                return NextSearchFlow.Continue;
            });
            
        }
    }
}