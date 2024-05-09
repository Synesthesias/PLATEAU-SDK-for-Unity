using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// <see cref="MAExecutor"/>が、どのゲームオブジェクトを対象とするかを決めます。
    /// </summary>
    public interface IMACondition
    {
        public void Init(UniqueParentTransformList targets);
        public bool ShouldDeconstruct(Transform trans, MAGranularity dstGranularity);
        public bool ShouldConstruct(Transform tarns, MAGranularity dstGranularity);
    }

    public static class MAConditionFactory
    {
        /// <summary>
        /// 設定によって応じて返す<see cref="MAConditionSimple"/>のファクトリです。
        /// </summary>
        public static MAConditionSimple Create(bool skipNotChangingMaterial, IMAMaterialChanger maMatChanger)
        {
            return skipNotChangingMaterial switch
            {
                true => new MAConditionMatChange(maMatChanger),
                false => new MAConditionSimple()
            };
        }
    }

    /// <summary>
    /// <see cref="MAExecutor"/>が、どのゲームオブジェクトを対象とするかの条件実装の1つです。
    /// シンプルな条件で、目標粒度が現在よりも細かければ分解、粗ければ結合する条件です。
    /// </summary>
    public class MAConditionSimple : IMACondition
    {

        public MAConditionSimple()
        {
        }

        public virtual void Init(UniqueParentTransformList targets)
        {
            
        }
        
        public virtual bool ShouldDeconstruct(Transform trans, MAGranularity dstGranularity)
        {
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            if (trans.GetComponent<MeshFilter>() == null) return false;
            var srcGranularity = cityObjGroup.Granularity;
            
            // srcが主要地物、地域単位の場合は、マテリアル分けのために最小地物まで分解する
            return srcGranularity > MeshGranularity.PerAtomicFeatureObject;
        }

        /// <summary>
        /// 自身、または非再帰的な子に目標粒度よりも細かい粒度があるかチェックします
        /// </summary>
        public virtual bool ShouldConstruct(Transform trans, MAGranularity dstGranularity)
        {
            // 自身のチェック
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup != null && cityObjGroup.Granularity < dstGranularity.ToNativeGranularity())
            {
                return true;
            }
            
            // 子のチェック
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                var childCog = child.GetComponent<PLATEAUCityObjectGroup>();
                if (childCog == null) continue;
                if (childCog.Granularity < dstGranularity.ToNativeGranularity()) return true;
            } 
            return false;
        }
    }

    /// <summary>
    /// <see cref="MAExecutor"/>が、どのゲームオブジェクトを対象とするかの条件実装の1つです。
    /// マテリアルに変更があるときのみ分割結合の対象とする条件です。
    /// </summary>
    public class MAConditionMatChange : MAConditionSimple
    {
        private IMAMaterialChanger matChanger;
        
        /// マテリアルの観点から、Transformキーを変換対象とするかどうかの辞書
        private Dictionary<Transform, bool> targetDict = new Dictionary<Transform, bool>();

        public MAConditionMatChange(IMAMaterialChanger matChanger)
        {
            this.matChanger = matChanger;
        }

        public override void Init(UniqueParentTransformList targets)
        {
            // 変換対象とすべきかどうかの辞書を構築します
            targets.BfsExec(trans =>
            {
                if (targetDict.TryGetValue(trans, out bool _))
                {
                    return NextSearchFlow.Continue;
                }
                bool changeMat = matChanger.ShouldChange(trans);
                
                // 建物の一部がスキップされて、一部のみ変換対象となってしまうと建物がバラバラになって不便なので、
                // 最小地物に変更がある場合、主要地物は必ず変換対象とします。
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog == null) return ContinueWithAdd(trans, changeMat);
                
                // 主要地物の場合、子を調べて1つでも対象とすべきなら対象。
                if (cog.Granularity == MeshGranularity.PerPrimaryFeatureObject)
                {
                    new UniqueParentTransformList(trans).BfsExec(childTrans =>
                    {
                        if (childTrans == trans) return NextSearchFlow.Continue;
                        bool changeChild = matChanger.ShouldChange(childTrans);
                        if (changeChild)
                        {
                            changeMat = true;
                            return NextSearchFlow.Abort;
                        }

                        return NextSearchFlow.Continue;
                    });
                }
                
                // 最小地物の場合、親が対象なら対象。
                else if (cog.Granularity == MeshGranularity.PerAtomicFeatureObject)
                {
                    var parent = trans.parent;
                    while (parent != null)
                    {
                        if (targetDict.TryGetValue(parent, out bool isTarget))
                        {
                            if (isTarget)
                            {
                                changeMat = true;
                                break;
                            }
                        }
                        parent = parent.parent;
                    }
                }
                
                // 上記の結果を格納
                return ContinueWithAdd(trans, changeMat);
            });
        }

        private NextSearchFlow ContinueWithAdd(Transform trans, bool shouldChangeMat)
        {
            targetDict.Add(trans, shouldChangeMat);
            return NextSearchFlow.Continue;
        }
        
        public override bool ShouldDeconstruct(Transform trans, MAGranularity dstGranularity)
        {
            bool baseRet = base.ShouldDeconstruct(trans, dstGranularity);
            bool isTargetRecursive = IsTargetRecursive(trans);
            return baseRet && isTargetRecursive;
        }

        public override bool ShouldConstruct(Transform srcTrans, MAGranularity dstGranularity)
        {
            bool baseRet = base.ShouldConstruct(srcTrans, dstGranularity);
            bool isTargetRecursive = IsTargetRecursive(srcTrans);
            return baseRet && isTargetRecursive;
        }

        public bool IsTarget(Transform srcTrans)
        {
            if (targetDict.TryGetValue(srcTrans, out var isTarget))
            {
                return isTarget;
            }

            return false;
        }

        public bool IsTargetRecursive(Transform srcTrans)
        {
            var transforms = new UniqueParentTransformList(srcTrans);
            bool ret = false;
            transforms.BfsExec(trans =>
            {
                if (IsTarget(trans))
                {
                    ret = true;
                    return NextSearchFlow.Abort;
                }
                return NextSearchFlow.Continue;
            });
            return ret;
        }
    }

}