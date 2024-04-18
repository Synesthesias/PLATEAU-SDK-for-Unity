using PLATEAU.PolygonMesh;
using PLATEAU.Util;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor
{
    /// <summary>
    /// <see cref="IMaterialAdjustExecutor"/>の実行に必要な設定項目です。
    /// 属性情報でのマテリアル分けの場合は、これの代わりにサブクラスである<see cref="MAExecutorConfByAttr"/>を使います。
    /// なおMAとはMaterialAdjustの略です。
    /// </summary>
    internal class MAExecutorConf
    {
        public IMaterialAdjustConf MaterialAdjustConf { get; }
        public UniqueParentTransformList TargetTransforms { get; }
        public MeshGranularity MeshGranularity { get; }
        public bool DoDestroySrcObjs { get; }

        public MAExecutorConf(IMaterialAdjustConf materialAdjustConf, UniqueParentTransformList targetTransforms,
            MeshGranularity meshGranularity, bool doDestroySrcObjs)
        {
            this.MaterialAdjustConf = materialAdjustConf;
            this.TargetTransforms = targetTransforms;
            this.MeshGranularity = meshGranularity;
            this.DoDestroySrcObjs = doDestroySrcObjs;
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

    internal class MAExecutorConfByAttr : MAExecutorConf
    {
        public string AttrKey;

        public MAExecutorConfByAttr(IMaterialAdjustConf materialAdjustConf,
            UniqueParentTransformList targetTransforms,
            MeshGranularity meshGranularity, bool doDestroySrcObjs, string attrKey)
            : base(materialAdjustConf, targetTransforms, meshGranularity, doDestroySrcObjs)
        {
            AttrKey = attrKey;
        }
    }
}