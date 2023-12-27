using System;
using System.Collections;
using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using Material = UnityEngine.Material;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// 各<see cref="CityObjectType"/>に対して、そのマテリアルをどのように変更するかの設定値の辞書です。
    /// </summary>
    internal class MaterialAdjustConf : IEnumerable<KeyValuePair<CityObjectTypeHierarchy.Node, MaterialAdjustConfPerType>>
    {
        private readonly SortedDictionary<CityObjectTypeHierarchy.Node, MaterialAdjustConfPerType> data;

        /// <summary>
        /// <see cref="CityObjectType"/>の配列をキーとして初期化します。
        /// </summary>
        public MaterialAdjustConf(IReadOnlyCollection<CityObjectType> types)
        {
            data = new();
            foreach (var type in types)
            {
                var typeNode = CityObjectTypeHierarchy.GetNodeByType(type);
                if (typeNode == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(types), $"Unknown Type: {type.ToString()}");
                }
                data.TryAdd(CityObjectTypeHierarchy.GetNodeByType(type), new MaterialAdjustConfPerType());
            }
        }

        /// <summary>
        /// 引数の<see cref="CityObjectType"/>に応じた設定を返します。
        /// なければnullを返します。
        /// </summary>
        public MaterialAdjustConfPerType GetConfFor(CityObjectType type)
        {
            var typeNode = CityObjectTypeHierarchy.GetNodeByType(type);
            if (typeNode != null && data.TryGetValue(typeNode, out var typeConf))
            {
                return typeConf;
            }

            return null;
        }

        /// <summary> 対象の地物型の種類数です。 </summary>
        public int Length => data.Count;

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
        public bool ChangeMaterial { get; set; }
        public Material Material { get; set; }

        public bool ShouldChangeMaterial => ChangeMaterial && Material != null;
    }
}