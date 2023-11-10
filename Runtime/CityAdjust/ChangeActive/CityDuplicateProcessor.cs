using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust
{
    /// <summary>
    /// 重複した地物があるか検索し、重複して表示されないようにします。
    /// </summary>
    public static class CityDuplicateProcessor
    {
        // TODO ヒエラルキー構成が変わったらここも直す必要がある。
        //      変更に強い設計に直したほうが良いのでは。
        /// <summary>
        /// 重複した地物があるか検索します。
        /// 重複のうちLODが最大のものを有効化し、そうでないものを無効化します。
        /// ただしすでに無効化されているものは無視します。
        /// </summary>
        public static void EnableOnlyLargestLODInDuplicate(PLATEAUInstancedCityModel city)
        {
            // var cityTrans = city.transform;
            // SetActiveAll(cityTrans, true);
            var gmlTransforms = city.GmlTransforms;
            // 各GMLについて
            foreach (var gmlTrans in gmlTransforms)
            {
                var sortedLods = LODTransformsSorted(gmlTrans).ToArray();
                // LODの高い方から
                for (int l = sortedLods.Length - 1; l >= 0; l--)
                {
                    var lodTrans = sortedLods[l];
                    if (!lodTrans.gameObject.activeInHierarchy) continue;
                    int objCount = lodTrans.childCount;
                    for (int o = 0; o < objCount; o++)
                    {
                        var objTrans = lodTrans.GetChild(o);
                        if (!objTrans.gameObject.activeInHierarchy) continue;
                        if (IsAllChildrenDisabled(objTrans)) continue; // LOD2以上で、自身はActiveでも子はすべて非Activeになっているというケースに対応します。
                        var objId = objTrans.name;
                        // LODの低いほうへ見て、重複があれば低いほうを非表示にします。
                        for (int searchLod = l-1; searchLod >= 0; searchLod--)
                        {
                            var found = sortedLods[searchLod].transform.Find(objId);
                            if (found != null)
                            {
                                found.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<Transform> LODTransformsSorted(Transform gmlTrans)
        {
            var lods = PLATEAUInstancedCityModel.GetLodTransforms(gmlTrans);
            return lods.OrderBy(lod => lod.name);
        }

        /// <summary>
        /// 子がおり、それらがすべて非Activeのとき、trueを返します。
        /// 子がない、または1つでも子がActiveであるとき false を返します。
        /// </summary>
        private static bool IsAllChildrenDisabled(Transform tran)
        {
            int childCount = tran.childCount;
            if (childCount == 0) return false;
            for (int i = 0; i < childCount; i++)
            {
                if (tran.GetChild(i).gameObject.activeInHierarchy) return false;
            }

            return true;
        }
    }
}
