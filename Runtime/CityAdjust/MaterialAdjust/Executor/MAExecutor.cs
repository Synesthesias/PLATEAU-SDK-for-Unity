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
    /// <see cref="MAExecutor"/>を依存性注入で作って返すファクトリです。
    /// なおMAとはMaterialAdjustの略です。
    /// </summary>
    internal static class MAExecutorFactory
    {
        /// <summary> マテリアル分けせず、分割結合のみ行うインスタンスを返します。 </summary>
        public static MAExecutor CreateGranularityExecutor(MAExecutorConf conf)
        {
            var materialChanger = new MADummyMaterialChanger();
            return new MAExecutor(
                conf,
                new MADecomposer(),
                materialChanger,
                new MAComposer(conf),
                MAConditionFactory.Create(conf.SkipNotChangingMaterial, materialChanger)
            );
        }
        
        /// <summary> 属性情報によってマテリアル分けするインスタンスを返します </summary>
        public static MAExecutor CreateAttrExecutor(MAExecutorConf confBase)
        {
            var conf = (MAExecutorConfByAttr)confBase;
            var maMaterialChanger =
                new MAMaterialChanger(conf.MaterialAdjustConf,
                    new MAMaterialSelectorByAttr(conf.AttrKey));
            return new MAExecutor(
                conf,
                new MADecomposer(),
                maMaterialChanger,
                new MAComposer(conf),
                MAConditionFactory.Create(conf.SkipNotChangingMaterial, maMaterialChanger)
            );
        }
        
        /// <summary> 地物型によってマテリアル分けするインスタンスを返します </summary>
        public static MAExecutor CreateTypeExecutor(MAExecutorConf conf)
        {
            var maMaterialChanger =
                new MAMaterialChanger(conf.MaterialAdjustConf,
                    new MAMaterialSelectorByType());
            return new MAExecutor(
                conf,
                new MADecomposer(),
                maMaterialChanger,
                new MAComposer(conf),
                MAConditionFactory.Create(conf.SkipNotChangingMaterial, maMaterialChanger)
            );
        }
    }

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
        private readonly ConditionalLogger logger = new ConditionalLogger(()=>false);


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

        public async Task Exec()
        {
            if (!conf.Validate())
            {
                Debug.LogError("設定値が不正です。");
                return;
            }
            
            maCondition.Init(conf.TargetTransforms);
            
            // 変更対象となったゲームオブジェクトの一覧を格納する
            var converted = new UniqueParentTransformList();
            var srcDstDict = new Dictionary<Transform, Transform>(); // src -> dst の対応の一部を記録する

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
                var copied = CopyGameObjAsResult(srcTrans, converted, srcDstDict);
                copied.transform.parent = srcDstDict[srcTrans].parent;
                return NextSearchFlow.Continue;
            }
            
            
            // 分解結合が必要な場合
            bool shouldDeconstruct = maCondition.ShouldDeconstruct(srcTrans, MAGranularity.PerAtomicFeatureObject);
            bool shouldConstruct = maCondition.ShouldConstruct(srcTrans, dstGranularity);
            if (shouldDeconstruct || shouldConstruct)
            {
                // 分解
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
                    foreach (var dec in decomposed.Get)
                    {
                        srcDstDict.Add(srcTrans, dec);
                    }
                }
                else
                {
                    // 分解不要なら代わりにコピー
                    logger.Log("分解フェイズ：コピー " + srcTrans.name);
                    var copied = CopyGameObjWithChildren(srcTrans, srcDstDict);
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
                            logger.Log("結合フェイズ：再帰的結合：コピー" + srcTrans.name);
                            var copied = CopyGameObjAsResult(innerTransSrc, converted, srcDstDict);
                            Object.DestroyImmediate(innerTransSrc);
                            return NextSearchFlow.SkipChildren;
                        }
                        logger.Log("結合フェイズ：再帰的結合：結合 "  + srcTrans.name);
                        var innerSrcParent = innerTransSrc.parent;
                        var parentOfComposed = innerSrcParent == null ? null : innerTransSrc.parent;
                        var result = await maComposer.ExecAsync(new UniqueParentTransformList(innerTransSrc), dstGranularity, maCondition);
                        var resultTransforms = result.Get.GeneratedRootTransforms;
                        foreach (var t in resultTransforms.Get)
                        {
                            t.parent = parentOfComposed;
                        }
                        converted.AddRange(resultTransforms.Get);
                        return NextSearchFlow.SkipChildren;
                    });
                }
                else
                {
                    // 分解してから全結合の場合
                    logger.Log("結合フェイズ：一括結合" + srcTrans.name);
                    var parentOfComposed = srcDstDict[srcTrans.parent];
                    var result = await maComposer.ExecAsync(decomposed, dstGranularity, composeCondition);
                    foreach (var r in result.Get.GeneratedRootTransforms.Get)
                    {
                        r.parent = parentOfComposed;
                    }
                    converted.AddRange(result.Get.GeneratedRootTransforms.Get);
                    return NextSearchFlow.SkipChildren;
                }
                
            
                // transとその子は完了
                return NextSearchFlow.SkipChildren;
            }
            // 分解せずに結合が必要な場合（srcが最小地物でdstがそれより粗い粒度）
            // if (maCondition.ShouldConstruct(srcTrans, dstGranularity)) // 最小地物の親である主要地物の場合
            // {
            //     // 
            //     var copied = Object.Instantiate(srcTrans.gameObject); // 子も含めてコピー
            //     var copiedTrans = copied.transform;
            //     copiedTrans.parent = srcDstDict.GetValueOrDefault(srcTrans.parent);
            //     maMaterialChanger.ExecRecursive(new UniqueParentTransformList(copiedTrans));
            //     var result = await maComposer.ExecAsync(new UniqueParentTransformList(copiedTrans), dstGranularity, maCondition);
            //     converted.AddRange(result.Get.GeneratedRootTransforms.Get);
            //     return NextSearchFlow.SkipChildren;
            // }
            
            // マテリアル変更が不要のためスキップする場合
            if (ShouldSkipDueToMaterial(srcTrans))
            {
                return NextSearchFlow.Continue;
            }
            
            // 分解も結合も不要だがコピーを作りたい場合（srcもdstも最小地物）
            if (!ShouldSkipDueToMaterial(srcTrans))
            {
                logger.Log("コピー：　分解結合不要： " + srcTrans.name);
                var copy = CopyGameObjAsResult(srcTrans, converted, srcDstDict);
                maMaterialChanger.Exec(copy.transform);
                return NextSearchFlow.Continue;
            }
            
            Debug.LogError("マテリアル分け： どのケースにも当てはまらない未知のケース： " + srcTrans.name);
            return NextSearchFlow.Continue;
            }); // END Transformごとの変換処理
            
            
            #if UNITY_EDITOR
            Selection.objects = converted.Get.Select(trans => (Object)trans.gameObject).ToArray();
            #endif

        }

        /// <summary>
        /// ゲームオブジェクト(子を除く)をコピーし、
        /// それを成果物として中間データ構造に追加します。
        /// </summary>
        private GameObject CopyGameObjAsResult(Transform srcTrans, UniqueParentTransformList converted, Dictionary<Transform, Transform> srcDstDict)
        {
            var copied = Object.Instantiate(srcTrans.gameObject);
            // 子まではコピーしない
            var objsToDestroy = new List<GameObject>();
            foreach (Transform child in copied.transform)
            {
                objsToDestroy.Add(child.gameObject);
            }
            foreach(var obj in objsToDestroy) Object.DestroyImmediate(obj);

            var srcParent = srcTrans.parent;
            if ( srcParent != null)
            {
                if (srcDstDict.TryGetValue(srcParent, out var dstParent))
                {
                    // すでに配置済みのdstの子の場合、親はdst側に合わせる
                    copied.transform.parent = dstParent;
                }
                else
                {
                    // dstの中ではrootの場合、親はsrcに合わせる
                    copied.transform.parent = srcTrans.parent;
                }
                
            }
            converted.Add(copied.transform);
            srcDstDict.Add(srcTrans, copied.transform);
            copied.name = copied.name.Replace("(Clone)", "");
            return copied;
        }

        private GameObject CopyGameObjWithChildren(Transform srcTrans, Dictionary<Transform, Transform> srcDstDict)
        {
            var copied = Object.Instantiate(srcTrans.gameObject);
            var srcParent = srcTrans.parent;
            if ( srcParent != null && srcDstDict.TryGetValue(srcParent, out var dstParent))
            {
                copied.transform.parent = dstParent;
            }
            srcDstDict.Add(srcTrans, copied.transform);
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