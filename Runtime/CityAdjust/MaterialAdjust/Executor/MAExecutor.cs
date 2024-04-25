using System.Threading.Tasks;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System.Collections.Generic;
using UnityEngine;
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
        /// <summary> 属性情報によってマテリアル分けするインスタンスを返します </summary>
        public static MAExecutor CreateAttrExecutor(MAExecutorConf confBase)
        {
            var conf = (MAExecutorConfByAttr)confBase;
            return new MAExecutor(
                conf,
                new MADecomposer(),
                new MAMaterialChanger(conf.MaterialAdjustConf,
                    new MAMaterialSelectorByAttr(conf.AttrKey)),
                new MAComposer(conf),
                conf.Condition
            );
        }
        
        /// <summary> 地物型によってマテリアル分けするインスタンスを返します </summary>
        public static MAExecutor CreateTypeExecutor(MAExecutorConf conf)
        {
            return new MAExecutor(
                conf,
                new MADecomposer(),
                new MAMaterialChanger(conf.MaterialAdjustConf,
                    new MAMaterialSelectorByType()),
                new MAComposer(conf),
                conf.Condition
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
        private readonly MAMaterialChanger maMaterialChanger;
        private readonly MAComposer maComposer;
        private readonly IMACondition maCondition;


        /// <summary>
        /// 設計方針:
        /// マテリアル分けは次の3手順から成ります。
        /// ・最小地物に分解 <see cref="maDecomposer"/>
        /// ・マテリアル変更 <see cref="maMaterialChanger"/>
        /// ・結合 <see cref="maComposer"/>
        ///
        /// これらの各処理をコンストラクタで注入するために<see cref="MAExecutorFactory"/>を利用してください。
        /// </summary>
        public MAExecutor(MAExecutorConf conf, MADecomposer maDecomposer, MAMaterialChanger maMaterialChanger,
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
            
            
            // 変更対象となったゲームオブジェクトの一覧を格納する
            var converted = new UniqueParentTransformList();
            var srcDstDict = new Dictionary<Transform, Transform>(); // src -> dst の対応の一部を記録する

            await conf.TargetTransforms.BfsExecAsync(async srcTrans =>
            {
                var dstGranularity = GranularityFor(srcTrans);
                
                // メッシュがない場合、属性情報だけコピーして次へ
                if (srcTrans.GetComponent<MeshFilter>() == null)
                {
                    CopyGameObj(srcTrans, converted, srcDstDict);
                    return NextSearchFlow.Continue;
                }

                
                // 分解が必要な場合
                if (maCondition.ShouldDeconstruct(srcTrans, MAGranularity.PerAtomicFeatureObject))
                {
                    // 分解
                    var decomposeResult = await maDecomposer.ExecAsync(conf, srcTrans);
                    if (!decomposeResult.IsSucceed) return NextSearchFlow.Continue;
                    var decomposed = decomposeResult.Get.GeneratedRootTransforms;
                    
                    // 分解したものについて、再帰的にマテリアル変更
                    maMaterialChanger.ExecRecursive(decomposed);

                    // 分解したものについて結合します。
                    if (dstGranularity != MAGranularity.CombineAll)
                    {
                        // 全結合でなければ、再帰的に結合します。
                        await decomposed.DfsExecAsync(async innerTransSrc =>
                        {
                            if (!maCondition.ShouldConstruct(innerTransSrc, dstGranularity))
                            {
                                CopyGameObj(innerTransSrc, converted, srcDstDict);
                                return NextSearchFlow.Continue;
                            }
                    
                            var result = await maComposer.ExecAsync(new UniqueParentTransformList(innerTransSrc), dstGranularity);
                            converted.AddRange(result.Get.GeneratedRootTransforms.Get);
                            return NextSearchFlow.SkipChildren;
                        });
                    }
                    else
                    {
                        // 全結合の場合
                        var result = await maComposer.ExecAsync(decomposed, dstGranularity);
                        converted.AddRange(result.Get.GeneratedRootTransforms.Get);
                        return NextSearchFlow.SkipChildren;
                    }
                    

                    // transとその子は完了
                    return NextSearchFlow.SkipChildren;
                }
                // 結合が必要な場合
                if (maCondition.ShouldConstruct(srcTrans, dstGranularity))
                {
                    maMaterialChanger.ExecRecursive(new UniqueParentTransformList(srcTrans));
                    var result = await maComposer.ExecAsync(new UniqueParentTransformList(srcTrans), dstGranularity);
                    converted.AddRange(result.Get.GeneratedRootTransforms.Get);
                    return NextSearchFlow.SkipChildren;
                }
                
                // 分解も結合も不要な場合（srcもdstも最小地物）
                var copy = CopyGameObj(srcTrans, converted, srcDstDict);
                maMaterialChanger.Exec(copy.transform);
                return NextSearchFlow.Continue;
            }); // END Transformごとの変換処理
            
            // 生成されたものを指定の名前のゲームオブジェクトの子に移動
            var dstParent = new GameObject(conf.DstObjName).transform;
            foreach (var trans in converted.Get)
            {
                trans.parent = dstParent;
            }

            #if UNITY_EDITOR
            Selection.activeTransform = dstParent;
            #endif

        }

        /// <summary>
        /// マテリアル分けの対象として、ゲームオブジェクト(子を除く)をコピーし、
        /// マテリアル分けの処理で使う中間データ構造に追加します。
        /// </summary>
        private GameObject CopyGameObj(Transform srcTrans, UniqueParentTransformList converted, Dictionary<Transform, Transform> srcDstDict)
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
            if ( srcParent != null && srcDstDict.TryGetValue(srcParent, out var dstParent))
            {
                copied.transform.parent = dstParent;
            }
            converted.Add(copied.transform);
            srcDstDict.Add(srcTrans, copied.transform);
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
                var cog = srcTrans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog != null)
                {
                    dstGranularity = cog.Granularity.ToMAGranularity();
                }
            }

            return dstGranularity;
        }

    }
}