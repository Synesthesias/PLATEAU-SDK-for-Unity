using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// SDKのモデル調整のマテリアル分け機能を提供する基底クラスです。
    /// 地物型によるマテリアル分け<see cref="MaterialAdjustExecutorByType"/>と、
    /// 属性情報によるマテリアル分け<see cref="MaterialAdjustExecutorByAttr"/>の共通部分を定義します。
    /// </summary>
    internal abstract class MaterialAdjustExecutorBase
    {
        public abstract Task Exec(AdjustExecutorConf conf);
        
    }

    /// <summary>
    /// <see cref="MaterialAdjustExecutorBase"/>の実行に必要な設定項目です。
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
    }
}