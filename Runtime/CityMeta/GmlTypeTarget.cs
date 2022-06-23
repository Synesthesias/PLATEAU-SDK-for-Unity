using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{

    /// <summary>
    /// <para>
    /// 街モデルのインポート時、インポート対象を 地物タイプ と 地域ID で絞り込むことができますが、
    /// そのうち 地物タイプ での絞り込み設定を保持するのがこのクラスの役割です。</para>
    /// <para>
    /// 例: udx/bldg 下のgmlは対象にするが、udx/veg 下のgmlは対象にしない、など。</para>
    /// </summary>
    
    // 補足:
    // 他クラスとの関係は CityImporterConfig -> 保持 -> GmlSearcherConfig -> 保持 -> GmlTypeTarget
    // という関係なので、 CityImporterConfig の注意事項に基づいてこのクラスには Serializable属性が付いている必要があります。
    
    [Serializable]
    internal class GmlTypeTarget : ISerializationCallbackReceiver
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

        /// <summary>
        /// 与えられたタイプが変換対象かどうかを返します。
        /// </summary>
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
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(TargetDict, this.keys, this.values);
        }


        /// <summary>
        /// デシリアライズするときに List から Dictionary 形式に直します。
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            TargetDict = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
        }
        
    }
}