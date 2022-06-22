using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{

    /// <summary>
    /// gmlファイル群は地物オブジェクトのタイプ別にフォルダ分けで格納されていますが、
    /// そのうちのどのタイプ（フォルダ）を変換ターゲットとするかを決めます。
    /// 例: udx/bldg下のgmlは対象にするが、udx/veg下のgmlは対象にしない、など。
    ///
    /// 他クラスとの関係は <see cref="CityImporterConfig"/> -> 保持 -> <see cref="GmlSearcherConfig"/> -> 保持 -> <see cref="GmlTypeTarget"/>
    /// という関係なので、<see cref="CityImporterConfig"/> の注意事項に基づいてこのクラスには Serializable属性が付いている必要があります。
    /// </summary>
    [Serializable]
    public class GmlTypeTarget : ISerializationCallbackReceiver
    {
        /// <summary> GmlTypeごとに、変換対象とするかどうかの辞書です。 </summary>
        public Dictionary<GmlType, bool> TargetDict { get; set; }

        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keys = new List<GmlType>();
        [SerializeField] private List<bool> values = new List<bool>();

        public GmlTypeTarget()
        {
            // 各タイプ true で初期化します。
            TargetDict =
                Enum.GetValues(typeof(GmlType))
                    .OfType<GmlType>()
                    .ToDictionary(t => t, _ => true);
        }

        public bool IsTypeTarget(GmlType t)
        {
            return TargetDict[t];
        }

        /// <summary> すべて true または すべて false にします。 </summary>
        public void SetAll(bool val)
        {
            foreach (var key in TargetDict.Keys.ToArray())
            {
                TargetDict[key] = val;
            }
        }

        /// <summary>
        /// シリアライズするときに List形式に直します。
        /// </summary>
        public void OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(TargetDict, this.keys, this.values);
        }


        /// <summary>
        /// デシリアライズするときに List から Dictionary 形式に直します。
        /// </summary>
        public void OnAfterDeserialize()
        {
            TargetDict = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
        }
        
    }
}