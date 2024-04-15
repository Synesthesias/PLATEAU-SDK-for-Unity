using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using Material = UnityEngine.Material;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// 検索で見つかった各分類項目に対して、そのマテリアルをどのように変更するかの設定値の辞書です。
    /// </summary>
    internal class MaterialAdjustConf<KeyT> : IMaterialAdjustConf
        // : IEnumerable<KeyValuePair<KeyT, MaterialChangeConf>>
        where KeyT : class
    {
        private readonly SortedList<KeyT, ChangeConfPerMaterial> data;

        /// <summary>
        /// 分類項目の配列をキーとして初期化します。
        /// </summary>
        public MaterialAdjustConf(IReadOnlyCollection<KeyT> keys)
        {
            data = new();
            foreach (var key in keys)
            {
                data.TryAdd(key, new ChangeConfPerMaterial());
            }
        }

        /// <summary>
        /// 引数の<see cref="KeyT"/>に対応した設定を返します。
        /// なければnullを返します。
        /// </summary>
        public ChangeConfPerMaterial GetConfFor(KeyT key)
        {
            if (key != null && data.TryGetValue(key, out var typeConf))
            {
                return typeConf;
            }

            return null;
        }

        /// <summary> 対象の地物型の種類数です。 </summary>
        public int Length => data.Count;
        
        public string GetKeyNameAt(int i)
        {
            var key = data.Keys[i];
            if (key is CityObjectTypeHierarchy.Node typeNode)
            {
                return typeNode.GetDisplayName();
            }
            return data.Keys[i].ToString();
        }

        public ChangeConfPerMaterial GetMaterialChangeConfAt(int i)
        {
            return data.Values[i];
        }
    }
    
    internal interface IMaterialAdjustConf
    {
        public string GetKeyNameAt(int i);
        public ChangeConfPerMaterial GetMaterialChangeConfAt(int i);
        public int Length { get; }
    }

    /// <summary>
    /// 各分類に対してマテリアルをどのように変更するかの設定値です。
    /// </summary>
    internal class ChangeConfPerMaterial
    {
        public bool ChangeMaterial { get; set; }
        public Material Material { get; set; }

        public bool ShouldChangeMaterial => ChangeMaterial && Material != null;
    }
}