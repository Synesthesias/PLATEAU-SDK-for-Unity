using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;

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
}