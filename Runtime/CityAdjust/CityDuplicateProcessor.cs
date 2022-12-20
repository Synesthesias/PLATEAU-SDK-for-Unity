using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust
{
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

        // private static void SetActiveAll(Transform trans, bool isActive)
        // {
        //     trans.gameObject.SetActive(isActive);
        //     int childCount = trans.childCount;
        //     for (int i = 0; i < childCount; i++)
        //     {
        //         var child = trans.GetChild(i);
        //         SetActiveAll(child, isActive);
        //     }
        // }
        
        private static IEnumerable<Transform> LODTransformsSorted(Transform gmlTrans)
        {
            var lods = PLATEAUInstancedCityModel.GetLodTransforms(gmlTrans);
            return lods.OrderBy(lod => lod.name);
            // int childCount = gmlTrans.childCount;
            // var availableLODs = new List<(int lod, Transform trans)>();
            // for (int i = 0; i < childCount; i++)
            // {
            //     var child = gmlTrans.GetChild(i);
            //     var name = child.name;
            //     if (!name.StartsWith("LOD")) continue;
            //     if (!int.TryParse(name.Substring("LOD".Length), out int lod))
            //         continue;
            //     availableLODs.Add((lod, child));
            // }
            //
            // return availableLODs
            //     .OrderBy(tuple => tuple.lod)
            //     .Select(tuple => tuple.trans);
        }

        private static bool IsAllChildrenDisabled(Transform tran)
        {
            int childCount = tran.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (tran.GetChild(i).gameObject.activeInHierarchy) return false;
            }

            return true;
        }
    }
}
