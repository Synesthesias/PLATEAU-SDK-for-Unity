using PLATEAU.CityInfo;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// ゲームオブジェクト名と属性情報の辞書です。
    /// <see cref="NonLibDataHolder"/>が利用します。
    /// </summary>
    internal class NameToAttrsDict : INonLibData
    {
        private Dictionary<string, PLATEAUCityObjectGroup> data = new();

        /// <summary>
        /// ゲームオブジェクトとその子から属性情報の辞書を構築します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            var attrs =
                src.Get.SelectMany(trans => trans.GetComponentsInChildren<PLATEAUCityObjectGroup>());
            foreach (var attr in attrs)
            {
                data.TryAdd(attr.gameObject.name, attr);
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
                var existingAttr = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (existingAttr == null)
                {
                    if (data.TryGetValue(trans.name, out var srcAttr))
                    {
                        var dstAttr = trans.gameObject.AddComponent<PLATEAUCityObjectGroup>();
                        dstAttr.Init(srcAttr.CityObjects, srcAttr.InfoForToolkits, srcAttr.Granularity);
                    }
                }
                else
                {
                    Debug.LogWarning("PLATEAUCityObjectGroup is already attached.");
                }
                return NextSearchFlow.Continue;
            });
            
        }
    }
}