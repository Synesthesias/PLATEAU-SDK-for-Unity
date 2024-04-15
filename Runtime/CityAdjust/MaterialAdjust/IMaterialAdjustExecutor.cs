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
        public IReadOnlyCollection<GameObject> TargetObjs { get; }
        public MeshGranularity MeshGranularity { get; }
        public bool DoDestroySrcObjs { get; }

        public AdjustExecutorConf(IMaterialAdjustConf materialAdjustConf, IReadOnlyCollection<GameObject> targetObjs, MeshGranularity meshGranularity, bool doDestroySrcObjs)
        {
            this.MaterialAdjustConf = materialAdjustConf;
            this.TargetObjs = targetObjs;
            this.MeshGranularity = meshGranularity;
            this.DoDestroySrcObjs = doDestroySrcObjs;
        }

        /// <summary>
        /// 設定が妥当ならtrueを返します。
        /// 不当ならダイアログを出してfalseを返します。
        /// </summary>
        public bool Validate()
        {
            if (TargetObjs.Count == 0)
            {
                Dialogue.Display("対象が選択されていません。\n対象を選択してください。", "OK");
                return false;
            }
            
            if (TargetObjs.Any(obj => obj == null))
            {
                Dialogue.Display("対象に削除されたゲームオブジェクトが含まれています。\n選択し直してください。", "OK");
                return false;
            }

            return true;
        }
    }

    internal class AdjustExecutorConfByAttr : AdjustExecutorConf
    {
        public string AttrKey;
        public AdjustExecutorConfByAttr(IMaterialAdjustConf materialAdjustConf, IReadOnlyCollection<GameObject> targetObjs,
            MeshGranularity meshGranularity, bool doDestroySrcObjs, string attrKey)
        :base(materialAdjustConf, targetObjs, meshGranularity, doDestroySrcObjs)
        {
            AttrKey = attrKey;
        }
    }
}