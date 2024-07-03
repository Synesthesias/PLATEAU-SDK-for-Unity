using PLATEAU.CityInfo;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// ゲームオブジェクト名と属性情報の辞書です。
    /// <see cref="NonLibDataHolder"/>が利用します。
    /// </summary>
    internal class NameToAttrsDict : INonLibData
    {
        private Dictionary<NonLibKeyName, PLATEAUCityObjectGroup> data = new();

        /// <summary>
        /// ゲームオブジェクトとその子から属性情報の辞書を構築します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            var attrs =
                src.Get.SelectMany(trans => trans.GetComponentsInChildren<PLATEAUCityObjectGroup>());
            foreach (var attr in attrs)
            {
                data.TryAdd(new NonLibKeyName(attr.transform), attr);
            }
        }

        /// <summary>
        /// <paramref name="target"/>とその子に対して、
        /// ゲームオブジェクト名を元に覚えておいた属性情報を復元します。
        /// </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                var key = new NonLibKeyName(trans);
                if (!data.ContainsKey(key)) return NextSearchFlow.Continue;
                var dstAttr = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (dstAttr == null)
                {
                    dstAttr = trans.gameObject.AddComponent<PLATEAUCityObjectGroup>();
                }

                var srcAttr = data[key];
                dstAttr.Init(srcAttr.CityObjects, srcAttr.InfoForToolkits, srcAttr.Granularity, srcAttr.Lod);
                
                return NextSearchFlow.Continue;
            });
            
        }
    }
}