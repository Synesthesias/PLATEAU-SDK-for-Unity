using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// SDKのモデル調整のマテリアル分け機能に関する基底クラスです。
    /// 地物型によるマテリアル分け<see cref="MaterialAdjusterByType"/>と、
    /// 属性情報によるマテリアル分け<see cref="MaterialAdjusterByAttr"/>の共通部分を定義します。
    /// </summary>
    internal abstract class MaterialAdjusterBase
    {
        
        protected IReadOnlyCollection<GameObject> targetObjs;
        public MeshGranularity Granularity = MeshGranularity.PerPrimaryFeatureObject;
        public MaterialAdjustConf MaterialAdjustConf { get; protected set; }
        public bool DoDestroySrcObjects { get; set; }

        public abstract Task Exec();
    }
}