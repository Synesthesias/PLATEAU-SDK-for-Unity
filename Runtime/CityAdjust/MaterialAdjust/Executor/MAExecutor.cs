using System.Threading.Tasks;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

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

            await conf.TargetTransforms.BfsExecAsync(async trans =>
            {
                // 分解が必要な場合
                if (maCondition.ShouldDeconstruct(trans, MAGranularity.PerAtomicFeatureObject))
                {
                    // 分解
                    var decomposeResult = await maDecomposer.ExecAsync(conf, trans);
                    if (!decomposeResult.IsSucceed) return NextSearchFlow.Continue;
                    var decomposed = decomposeResult.Get.GeneratedRootTransforms;
                    
                    // 分解したものについて、再帰的にマテリアル変更
                    maMaterialChanger.ExecRecursive(decomposed);

                    // 分解したものについて、再帰的に結合
                    await decomposed.DfsExecAsync(async innerTrans =>
                    {
                        if (!maCondition.ShouldConstruct(innerTrans, conf.MeshGranularity))
                        {
                            return NextSearchFlow.Continue;
                        }

                        await maComposer.ExecAsync(innerTrans);
                        return NextSearchFlow.SkipChildren;
                    });

                    // transとその子は完了
                    return NextSearchFlow.SkipChildren;
                }
                // 結合が必要な場合
                if (maCondition.ShouldConstruct(trans, conf.MeshGranularity))
                {
                    maMaterialChanger.ExecRecursive(new UniqueParentTransformList(trans));
                    await maComposer.ExecAsync(trans);
                    return NextSearchFlow.SkipChildren;
                }
                // 分解も結合も不要な場合
                maMaterialChanger.Exec(trans);
                return NextSearchFlow.Continue;
            });
        }

    }
}