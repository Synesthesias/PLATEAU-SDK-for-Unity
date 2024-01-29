using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.ChangeActive
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
                        var gmlIdsHigher = GetCityObjs(objTrans);
                        // LODの低いほうへ見て、重複があれば低いほうを非表示にします。
                        for (int searchLod = l-1; searchLod >= 0; searchLod--)
                        {
                            var searchLodTrans = sortedLods[searchLod].transform;
                            
                            // 重複判定1 : 同名のゲームオブジェクトがあるか。
                            var foundByName = searchLodTrans.Find(objId);
                            if (foundByName != null)
                            {
                                foundByName.gameObject.SetActive(false);
                                continue;
                            }
                            
                            // 重複判定2 : LODの低いほうが単位が細かい状況で、LODの低いほうのゲームオブジェクトが高いほうのGML IDに含まれるケース
                            if (gmlIdsHigher.Count > 0)
                            {
                                foreach (var gmlId in gmlIdsHigher)
                                {
                                    var foundByGmlId = searchLodTrans.Find(gmlId);
                                    if (foundByGmlId != null)
                                    {
                                        foundByGmlId.gameObject.SetActive(false);
                                    }
                                }
                                
                            }
                            
                            // 重複判定3 : LODの低い方が単位が粗い状況で、LODの低いほうのGML IDを高いほうがすべて含むケース
                            foreach (Transform searchObjTrans in searchLodTrans)
                            {
                                var searchGmls = GetCityObjs(searchObjTrans);
                                if (searchGmls.Count > 0)
                                {
                                    bool upperLodContainsAllLowerLodGmlId = true;
                                    foreach (var searchGml in searchGmls)
                                    {
                                        if (lodTrans.Find(searchGml) == null)
                                        {
                                            upperLodContainsAllLowerLodGmlId = false;
                                            break;
                                        }
                                    }

                                    if (upperLodContainsAllLowerLodGmlId)
                                    {
                                        searchObjTrans.gameObject.SetActive(false);
                                    }
                                }
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

        private static List<string> GetCityObjs(Transform trans)
        {
            // 重複判定2,3向けのメソッドです。
            var components = trans.GetComponentsInChildren<PLATEAUCityObjectGroup>();
            var gmlIds = new List<string>();
            // コンポーネントが見つからない場合は空配列とします。
            if (components.Length == 0) return gmlIds;
            foreach (var c in components)
            {
                // 下の式において、rootCityObjects の children までは見なくて良いです。
                // なぜなら、それは最小地物であり、最小単位で disable したい状況は重複判定1で拾えており、重複判定2,3ではないからです。
                gmlIds.AddRange(c.CityObjects.rootCityObjects.Select(cityObj => cityObj.GmlID));
            }

            return gmlIds;
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
