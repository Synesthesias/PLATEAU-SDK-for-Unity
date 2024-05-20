using System.Threading.Tasks;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor
{
    /// <summary>
    /// マテリアル分けを実行します。
    /// このインスタンスを作成するには<see cref="MAExecutorFactory"/>を利用してください。
    /// </summary>
    internal class MAExecutor
    {
        private readonly MAExecutorConf conf;
        private readonly MADecomposer maDecomposer;
        private readonly IMAMaterialChanger maMaterialChanger;
        private readonly MAComposer maComposer;
        private readonly IMACondition maCondition;

        // デバッグ時は下をtrueにしてログを出し、デバッグが終わったらfalseにしてログを隠してください。
        private readonly ConditionalLogger logger = new (() => true);


        /// <summary>
        /// 設計方針:
        /// マテリアル分けは次の3手順から成ります。
        /// ・最小地物に分解 <see cref="maDecomposer"/>
        /// ・マテリアル変更 <see cref="maMaterialChanger"/>
        /// ・結合 <see cref="maComposer"/>
        ///
        /// これらの各処理をコンストラクタで注入するために<see cref="MAExecutorFactory"/>を利用してください。
        /// </summary>
        public MAExecutor(MAExecutorConf conf, MADecomposer maDecomposer, IMAMaterialChanger maMaterialChanger,
            MAComposer maComposer, IMACondition maCondition)
        {
            this.conf = conf;
            this.maDecomposer = maDecomposer;
            this.maMaterialChanger = maMaterialChanger;
            this.maComposer = maComposer;
            this.maCondition = maCondition;
        }

        public async Task<UniqueParentTransformList> Exec()
        {
            if (!conf.Validate())
            {
                Debug.LogError("設定値が不正です。");
                return null;
            }

            maCondition.Init(conf.TargetTransforms);

            // 変更対象となったゲームオブジェクトの一覧を格納する
            var srcMeshes = new UniqueParentTransformList(); // 変換の対象となったsrcを後で消すために記憶する
            var converted = new UniqueParentTransformList();

            await conf.TargetTransforms.BfsExecAsync(async srcTrans =>
            {
                var dstGranularity = GranularityFor(srcTrans);

                // メッシュがなく、分解結合の必要もない場合、属性情報だけコピーして次へ
                if (srcTrans.GetComponent<MeshFilter>() == null &&
                    !(ShouldSkipDueToMaterial(srcTrans)) &&
                    !(maCondition.ShouldConstruct(srcTrans, dstGranularity)) &&
                    !(maCondition.ShouldDeconstruct(srcTrans, dstGranularity)))
                {
                    logger.Log("通常コピー  " + srcTrans.name);
                    var copied = CopyGameObjAsResult(srcTrans, converted, true);
                    copied.transform.parent = srcTrans.parent;
                    return NextSearchFlow.Continue;
                }


                // 分解結合が必要な場合
                bool shouldDeconstruct = maCondition.ShouldDeconstruct(srcTrans, MAGranularity.PerAtomicFeatureObject);
                bool shouldConstruct = maCondition.ShouldConstruct(srcTrans, dstGranularity);
                if (shouldDeconstruct || shouldConstruct)
                {
                    // 分解
                    srcMeshes.Add(srcTrans);
                    UniqueParentTransformList decomposed;
                    if (shouldDeconstruct)
                    {
                        logger.Log("分解フェイズ：分解 " + srcTrans.name);
                        var decomposeResult = await maDecomposer.ExecAsync(conf, srcTrans, maCondition);
                        if (!decomposeResult.IsSucceed)
                        {
                            Debug.LogError($"分解失敗: {srcTrans.name}");
                            return NextSearchFlow.Continue;
                        }

                        decomposed = decomposeResult.Get.GeneratedRootTransforms;
                    }
                    else
                    {
                        // 分解不要なら代わりにコピー
                        logger.Log("分解フェイズ：コピー " + srcTrans.name);
                        var copied = CopyGameObjWithChildren(srcTrans);
                        decomposed = new UniqueParentTransformList(copied.transform);
                    }


                    // 分解したものについて、再帰的にマテリアル変更
                    maMaterialChanger.ExecRecursive(decomposed);

                    // 条件を結合向けに再設定
                    IMACondition composeCondition;
                    switch (maCondition)
                    {
                        case MAConditionMatChange:
                            composeCondition = new MAConditionMatChange(maMaterialChanger);
                            composeCondition.Init(new UniqueParentTransformList(decomposed.Get));
                            break;
                        default:
                            composeCondition = maCondition;
                            break;
                    }

                    // 分解したものについて結合します。
                    if (dstGranularity != MAGranularity.CombineAll)
                    {
                        // 全結合でなければ、再帰的に結合します。
                        await decomposed.DfsExecAsync(async innerTransSrc =>
                        {
                            if (!composeCondition.ShouldConstruct(innerTransSrc, dstGranularity))
                            {
                                logger.Log("結合フェイズ：再帰的結合：コピー" + innerTransSrc.name);
                                CopyGameObjAsResult(innerTransSrc, converted, false);
                                Object.DestroyImmediate(innerTransSrc.gameObject);
                                return NextSearchFlow.SkipChildren;
                            }

                            logger.Log("結合フェイズ：再帰的結合：結合 " + innerTransSrc.name);
                            var innerSrcParent = innerTransSrc.parent;
                            var parentOfComposed = innerSrcParent == null ? null : innerTransSrc.parent;
                            var result = await maComposer.ExecAsync(new UniqueParentTransformList(innerTransSrc),
                                dstGranularity, maCondition);
                            var resultTransforms = result.Get.GeneratedRootTransforms;
                            if (dstGranularity != MAGranularity.PerMaterialInPrimary) // PerMaterialInPrimaryのときはCityGranularityConverter内ですでに親変更済み
                            {
                                foreach (var t in resultTransforms.Get)
                                {
                                    t.parent = parentOfComposed;
                                }
                            }
                            

                            converted.AddRange(resultTransforms.Get);
                            return NextSearchFlow.SkipChildren;
                        });
                    }
                    else
                    {
                        var result = await maComposer.ExecAsync(decomposed, dstGranularity, composeCondition);
                        converted.AddRange(result.Get.GeneratedRootTransforms.Get);
                        return NextSearchFlow.SkipChildren;
                    }


                    // transとその子は完了
                    return NextSearchFlow.SkipChildren;
                }

                // マテリアル変更が不要のためスキップする場合
                if (ShouldSkipDueToMaterial(srcTrans))
                {
                    return NextSearchFlow.Continue;
                }

                // 分解も結合も不要だがコピーを作りたい場合
                if (!ShouldSkipDueToMaterial(srcTrans))
                {
                    logger.Log("コピー：　分解結合不要： " + srcTrans.name);
                    srcMeshes.Add(srcTrans);
                    var copy = CopyGameObjAsResult(srcTrans, converted, false);
                    maMaterialChanger.ExecRecursive(new UniqueParentTransformList(copy.transform));
                    return NextSearchFlow.SkipChildren;
                }

                Debug.LogError("マテリアル分け： どのケースにも当てはまらない未知のケース： " + srcTrans.name);
                return NextSearchFlow.Continue;
            }); // END Transformごとの変換処理


            // 変換結果のうち、子にメッシュを1つも持たないものを削除
            var toDestroy = converted.Get
                .Where(c => c != null && c.GetComponentInChildren<MeshRenderer>() == null)
                .ToArray();
            foreach (var d in toDestroy)
            {
                Object.DestroyImmediate(d.gameObject);
            }

            var result = new UniqueParentTransformList(converted.Get.Where(t => t != null));

            // 変換元を削除する設定なら削除
            if (conf.DoDestroySrcObjs)
            {
                var srcToDestroy = new UniqueParentTransformList();
                srcMeshes.BfsExec(t =>
                {
                    if (t == null) return NextSearchFlow.Continue;

                    // dstが含まれるものを削除対象から除外する
                    bool shouldDestroy = true;
                    foreach (var r in result.Get)
                    {
                        if (r.IsChildOf(t) || r == t)
                        {
                            shouldDestroy = false;
                            break;
                        }
                    }

                    if (shouldDestroy)
                    {
                        srcToDestroy.Add(t);
                        return NextSearchFlow.SkipChildren;
                    }

                    return NextSearchFlow.Continue;
                });


                foreach (var d in srcToDestroy.Get)
                {
                    if (d == null) continue;
                    Object.DestroyImmediate(d.gameObject);
                }
            }

#if UNITY_EDITOR
            Selection.objects = result.Get.Select(trans => (Object)trans.gameObject).ToArray();
#endif

            return result;
        }

        /// <summary>
        /// ゲームオブジェクト(子を除く)をコピーし、
        /// それを成果物として中間データ構造に追加します。
        /// </summary>
        private GameObject CopyGameObjAsResult(Transform srcTrans, UniqueParentTransformList converted,
            bool withoutChild)
        {
            var copied = Object.Instantiate(srcTrans.gameObject);
            if (withoutChild)
            {
                // 子まではコピーしない
                var objsToDestroy = new List<GameObject>();
                foreach (Transform child in copied.transform)
                {
                    objsToDestroy.Add(child.gameObject);
                }

                foreach (var obj in objsToDestroy) Object.DestroyImmediate(obj);
            }

            var srcParent = srcTrans.parent;
            if (srcParent != null)
            {
                // dstの中ではrootの場合、親はsrcに合わせる
                var copiedTran = copied.transform;
                copiedTran.parent = srcTrans.parent;
                copiedTran.SetSiblingIndex(srcTrans.GetSiblingIndex());
            }

            converted.Add(copied.transform);
            copied.name = copied.name.Replace("(Clone)", "");
            return copied;
        }

        private GameObject CopyGameObjWithChildren(Transform srcTrans)
        {
            var srcParent = srcTrans.parent;
            var copied = Object.Instantiate(srcTrans.gameObject, srcParent, true);
            copied.name = copied.name.Replace("(Clone)", "");
            return copied;
        }

        /// <summary>
        /// マテリアル分け設定が「粒度を変えない」だった場合、どの粒度にすれば良いのかを返します。
        /// 粒度に指定がある場合、それをそのまま返します。
        /// </summary>
        private MAGranularity GranularityFor(Transform srcTrans)
        {
            var dstGranularity = conf.MeshGranularity;
            if (conf.MeshGranularity == MAGranularity.DoNotChange)
            {
                // もとの粒度が何であるかを探します。
                var cog = srcTrans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog != null)
                {
                    return cog.Granularity.ToMAGranularity();
                }

                // 自身になければ子から探します。
                for (int i = 0; i < srcTrans.childCount; i++)
                {
                    var childCog = srcTrans.GetChild(i).GetComponentInChildren<PLATEAUCityObjectGroup>();
                    if (childCog != null)
                    {
                        return childCog.Granularity.ToMAGranularity();
                    }
                }

                throw new Exception("Could not determine src granularity.");
            }

            return dstGranularity;
        }

        private bool ShouldSkipDueToMaterial(Transform srcTrans)
        {
            if (maCondition is not MAConditionMatChange cond) return false;
            return !cond.IsTargetRecursive(srcTrans);
        }
    }
}