using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEngine;
using Material = UnityEngine.Material;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// SDKのモデル調整のマテリアル分けに関する機能を担います。
    /// </summary>
    internal class CityMaterialAdjuster
    {
        private readonly IReadOnlyCollection<GameObject> targetObjs;
        public MaterialAdjustConf MaterialAdjustConf { get; }

        public CityMaterialAdjuster(IReadOnlyCollection<GameObject> targetObjs)
        {
            this.targetObjs = targetObjs;
            var targetTransforms = targetObjs.Select(obj => obj.transform).ToArray();
            var foundTypes = new CityTypeSearcher().Search(targetTransforms);
            MaterialAdjustConf = new MaterialAdjustConf(foundTypes);
        }
    }

    /// <summary>
    /// 各<see cref="CityObjectType"/>に対して、そのマテリアルをどのように変更するかの設定値の辞書です。
    /// </summary>
    internal class MaterialAdjustConf : IEnumerable<KeyValuePair<CityObjectTypeHierarchy.Node, MaterialAdjustConfPerType>>
    {
        private SortedDictionary<CityObjectTypeHierarchy.Node, MaterialAdjustConfPerType> data;

        /// <summary>
        /// <see cref="CityObjectType"/>の配列をキーとして初期化します。
        /// </summary>
        public MaterialAdjustConf(IReadOnlyCollection<CityObjectType> types)
        {
            data = new();
            foreach (var type in types)
            {
                data.TryAdd(CityObjectTypeHierarchy.GetNodeByType(type), new MaterialAdjustConfPerType());
            }
        }

        /// <summary> foreachで回せるようにします </summary>
        public IEnumerator<KeyValuePair<CityObjectTypeHierarchy.Node, MaterialAdjustConfPerType>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class MaterialAdjustConfPerType
    {
        public bool ChangeMaterial { get; set; } = false;
        public Material Material { get; set; } = null;
    }
}