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
        /// <summary> GmlTypeごとの設定の辞書です。 </summary>
        private Dictionary<GmlType, bool> IsTypeTargetDict { get; set; }

        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keys = new List<GmlType>();
        [SerializeField] private List<bool> values = new List<bool>();

        public GmlTypeTarget()
        {
            // 各タイプごとに ImportGmlTypeConfig を初期化します。
            IsTypeTargetDict = GmlTypeConvert.ComposeTypeDict(true);
        }

        /// <summary>
        /// 与えられたタイプが変換対象かどうかを返します。
        /// </summary>
        public bool IsTypeTarget(GmlType t)
        {
            return IsTypeTargetDict[t];
        }

        /// <summary>
        /// 地物タイプ <paramref name="t"/> を対象とするかどうかセットします。
        /// </summary>
        public void SetIsTypeTarget(GmlType t, bool isTarget)
        {
            IsTypeTargetDict[t] = isTarget;
        }

        public GmlType[] Keys => IsTypeTargetDict.Keys.ToArray();

        /// <summary> 地物タイプを変換対象とするかについて、すべて true または すべて false にします。 </summary>
        public void SetAllTarget(bool val)
        {
            foreach (var key in IsTypeTargetDict.Keys.ToArray())
            {
                IsTypeTargetDict[key] = val;
            }
        }

        /// <summary> シリアライズするときに List形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(IsTypeTargetDict, this.keys, this.values);
        }


        /// <summary> デシリアライズするときに List から Dictionary 形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            IsTypeTargetDict = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
        }
        
    }
}