using System.Threading.Tasks;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;

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
                new MAComposer(conf)
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
                new MAComposer(conf)
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
        

        /// <summary>
        /// 設計方針:
        /// マテリアル分けは次の3手順から成ります。
        /// ・最小地物に分解 <see cref="maDecomposer"/>
        /// ・マテリアル変更 <see cref="maMaterialChanger"/>
        /// ・結合 <see cref="maComposer"/>
        ///
        /// これらの各処理をコンストラクタで注入するために<see cref="MAExecutorFactory"/>を利用してください。
        /// </summary>
        public MAExecutor(MAExecutorConf conf, MADecomposer maDecomposer, MAMaterialChanger maMaterialChanger, MAComposer maComposer)
        {
            this.conf = conf;
            this.maDecomposer = maDecomposer;
            this.maMaterialChanger = maMaterialChanger;
            this.maComposer = maComposer;
        }

        public async Task Exec()
        {
            // 分解します
            var decomposeResult = await maDecomposer.ExecAsync(conf);
            if (!decomposeResult.IsSucceed) return;
            var decomposeReturned = decomposeResult.Get;
            
            // マテリアルを変更します。
            maMaterialChanger.Exec(decomposeReturned);
            
            // 結合し直します。
            await maComposer.ExecAsync(decomposeReturned);
        }
    }
}