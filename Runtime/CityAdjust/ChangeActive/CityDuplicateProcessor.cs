using PLATEAU.CityImport.Import;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityAdjust.ChangeActive
{
    /// <summary>
    /// 重複した地物があるか検索し、重複して表示されないようにします。
    /// </summary>
    public class CityDuplicateProcessor : IPostGmlImportProcessor
    {
        /// <summary>
        /// 重複した地物があるか検索します。GMLファイルに対応するゲームオブジェクトを対象とします。(例:53394558_bldg_6697_op.gml)
        /// 重複のうちLODが最大のものを有効化し、そうでないものを無効化します。
        /// ただしすでに無効化されているものは無視します。
        /// </summary>
        public static void EnableOnlyLargestLODInDuplicate(Transform gmlTrans)
        {
            var sortedLods = LODTransformsSorted(gmlTrans).ToArray();
            // LODの高い方から
            for (int higherLodIndex = sortedLods.Length - 1; higherLodIndex >= 0; higherLodIndex--)
            {
                var higherLodTrans = sortedLods[higherLodIndex];
                if (!higherLodTrans.gameObject.activeInHierarchy) continue;
                int objCount = higherLodTrans.childCount;
                for (int higherObjIndex = 0; higherObjIndex < objCount; higherObjIndex++)
                {
                    var higherObjTrans = higherLodTrans.GetChild(higherObjIndex);
                    if (!higherObjTrans.gameObject.activeInHierarchy) continue;
                    if (IsAllChildrenDisabled(higherObjTrans))
                        continue; // LOD2以上で、自身はActiveでも子はすべて非Activeになっているというケースに対応します。
                    var higherObjId = higherObjTrans.name;
                    var higherGmlIds = GetCityObjs(higherObjTrans);
                    // LODの低いほうへ見て、重複があれば低いほうを非表示にします。
                    for (int lowerLodIndex = higherLodIndex - 1; lowerLodIndex >= 0; lowerLodIndex--)
                    {
                        var lowerLodTrans = sortedLods[lowerLodIndex].transform;

                        // 重複判定1 : 同名のゲームオブジェクトがあるか。
                        // 例
                        // LOD1 -> bldg_A
                        // LOD2 -> bldg_A
                        // というケースでは、LOD1 の bldg_A を非表示にする。
                        var foundByName = lowerLodTrans.Find(higherObjId);
                        if (foundByName != null)
                        {
                            foundByName.gameObject.SetActive(false);
                            continue;
                        }

                        // 重複判定2 : LODの低いほうが単位が細かい状況で、LODの低いほうのゲームオブジェクトが高いほうのGML IDに含まれるケース
                        // 例
                        // LOD1 -> bldg_A, bldg_B
                        // LOD2 -> group1
                        // ただし、bldg_A が group1 の gmlID に含まれている
                        // というケースでは、bldg_A を非表示にする。
                        // インポート時にはこのケースはないが、分割結合機能を使うとこのケースはありうる。
                        if (higherGmlIds.Count > 0)
                        {
                            foreach (var higherGmlId in higherGmlIds)
                            {
                                var foundByGmlId = lowerLodTrans.Find(higherGmlId);
                                if (foundByGmlId != null)
                                {
                                    foundByGmlId.gameObject.SetActive(false);
                                }
                            }
                        }

                        // 重複判定3 : LODの低い方が単位が粗い状況で、LODの低いほうのGML IDを、高いほうがすべて含むケース
                        // 例
                        // LOD1 -> area1
                        // LOD2 -> bldg_A, bldg_B
                        // ただし、area1 の gmlID を列挙すると bldg_A, bldg_B となり、そのすべてが LOD2 に含まれている
                        // というケースでは、area1を非表示にする。
                        // インポート時にはこのケースはないが、分割結合機能を使うとこのケースはありうる。
                        foreach (Transform lowerObjTrans in lowerLodTrans)
                        {
                            var lowerGmls = GetCityObjs(lowerObjTrans);
                            if (lowerGmls.Count > 0)
                            {
                                bool upperLodContainsAllLowerLodGmlId = true;
                                foreach (var lowerGml in lowerGmls)
                                {
                                    if (higherLodTrans.Find(lowerGml) == null)
                                    {
                                        upperLodContainsAllLowerLodGmlId = false;
                                        break;
                                    }
                                }

                                if (upperLodContainsAllLowerLodGmlId)
                                {
                                    lowerObjTrans.gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 重複非表示化で<see cref="PLATEAUInstancedCityModel"/>を対象とする版です。
        /// </summary>
        public static void EnableOnlyLargestLODInDuplicate(PLATEAUInstancedCityModel city)
        {
            var gmlTransforms = city.GmlTransforms;
            // 各GMLについて
            foreach (var gmlTrans in gmlTransforms)
            {
                EnableOnlyLargestLODInDuplicate(gmlTrans);
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

        /// <summary>
        /// GML1つをインポートしたあとに呼ばれます。重複する子LODを非表示にします。
        /// </summary>
        public void OnGmlImported(GmlImportResult result)
        {
            var gml = new UniqueParentTransformList(result.GeneratedObjects.Select(obj => obj.transform)).CalcCommonParent();
            if (gml == null || !gml.name.EndsWith(".gml"))
            {
                Debug.LogError("Failed to find GML transform. Skipping duplicate processing.");
                return;
            }
            EnableOnlyLargestLODInDuplicate(gml);
        }
    }
}