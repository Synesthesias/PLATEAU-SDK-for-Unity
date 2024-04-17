using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// SDKのモデル調整のマテリアル分け機能を提供するインターフェイスです。
    /// 地物型によるマテリアル分け<see cref="MaterialAdjustExecutorByType"/>と、
    /// 属性情報によるマテリアル分け<see cref="MaterialAdjustExecutorByAttr"/>の2つの実装があります。
    /// </summary>
    internal interface IMaterialAdjustExecutor
    {
        public Task Exec(AdjustExecutorConf conf);
        
    }

    /// <summary>
    /// <see cref="IMaterialAdjustExecutor"/>の実行に必要な設定項目です。
    /// 属性情報でのマテリアル分けの場合は、これの代わりにサブクラスである<see cref="AdjustExecutorConfByAttr"/>を使います。
    /// </summary>
    internal class AdjustExecutorConf
    {
        public IMaterialAdjustConf MaterialAdjustConf { get; }
        public UniqueParentTransformList TargetTransforms { get; }
        public MeshGranularity MeshGranularity { get; }
        public bool DoDestroySrcObjs { get; }

        public AdjustExecutorConf(IMaterialAdjustConf materialAdjustConf, UniqueParentTransformList targetTransforms, MeshGranularity meshGranularity, bool doDestroySrcObjs)
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

    internal class AdjustExecutorConfByAttr : AdjustExecutorConf
    {
        public string AttrKey;
        public AdjustExecutorConfByAttr(IMaterialAdjustConf materialAdjustConf, UniqueParentTransformList targetTransforms,
            MeshGranularity meshGranularity, bool doDestroySrcObjs, string attrKey)
        :base(materialAdjustConf, targetTransforms, meshGranularity, doDestroySrcObjs)
        {
            AttrKey = attrKey;
        }
    }
}