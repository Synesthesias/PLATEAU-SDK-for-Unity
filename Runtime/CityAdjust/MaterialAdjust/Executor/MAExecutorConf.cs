using PLATEAU.GranularityConvert;
using PLATEAU.Util;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor
{
    /// <summary>
    /// <see cref="IMaterialAdjustExecutor"/>の実行に必要な設定項目です。
    /// 属性情報でのマテリアル分けの場合は、これの代わりにサブクラスである<see cref="MAExecutorConfByAttr"/>を使います。
    /// なおMAとはMaterialAdjustの略です。
    /// </summary>
    public class MAExecutorConf
    {
        public IMAConfig MaterialAdjustConf { get; }
        public UniqueParentTransformList TargetTransforms { get; set; }
        public bool DoDestroySrcObjs { get; set; }
        
        /// <summary> マテリアルを変更しない箇所は、分割結合をスキップするか </summary>
        public bool SkipNotChangingMaterial { get; }

        /// <summary>
        /// 地物型でのマテリアル分けの設定です。
        /// </summary>
        public MAExecutorConf(IMAConfig materialAdjustConf, UniqueParentTransformList targetTransforms,
            bool doDestroySrcObjs, bool skipNotChangingMaterial
            )
        {
            MaterialAdjustConf = materialAdjustConf;
            TargetTransforms = targetTransforms;
            DoDestroySrcObjs = doDestroySrcObjs;
            SkipNotChangingMaterial = skipNotChangingMaterial;
        }

        public virtual MAExecutorConf Copy()
        {
            return new MAExecutorConf(MaterialAdjustConf, TargetTransforms, DoDestroySrcObjs,
                SkipNotChangingMaterial);
        }

        /// <summary>
        /// 設定が妥当ならtrueを返します。
        /// 不当ならダイアログを出してfalseを返します。
        /// </summary>
        public bool Validate()
        {
            if (TargetTransforms.Count == 0)
            {
                Dialogue.Display("対象が選択されていません。\n対象を選択してください。", "OK");
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 属性情報でのマテリアル分けの設定です。
    /// </summary>
    internal class MAExecutorConfByAttr : MAExecutorConf
    {
        public string AttrKey;

        public MAExecutorConfByAttr(IMAConfig materialAdjustConf,
            UniqueParentTransformList targetTransforms,
            bool doDestroySrcObjs, bool skipNotChangingMaterial,
            string attrKey
            )
            : base(materialAdjustConf, targetTransforms, doDestroySrcObjs, skipNotChangingMaterial)
        {
            AttrKey = attrKey;
        }

        public override MAExecutorConf Copy()
        {
            return new MAExecutorConfByAttr(MaterialAdjustConf, TargetTransforms, DoDestroySrcObjs,
                SkipNotChangingMaterial, AttrKey);
        }
    }
}