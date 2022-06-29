using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class ObjConvertLodConfig : ISerializationCallbackReceiver
    {
        public Dictionary<GmlType, MinMax<int>> TypeLodDict;

        /// <summary> GUIでスライダーの値を一時的保持するためのメンバ </summary>
        [NonSerialized] public Dictionary<GmlType, MinMax<float>> TypeLodSliderDict = new Dictionary<GmlType, MinMax<float>>();

        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keys = new List<GmlType>();
        [SerializeField] private List<MinMax<int>> values = new List<MinMax<int>>();


        public ObjConvertLodConfig()
        {
            // 各タイプごとの設定を初期化します。
            this.TypeLodDict = GmlTypeConvert.ComposeTypeDict<MinMax<int>>();
            LodToSliderVal();
        }

        /// <summary> Lod設定の値をGUIスライダー用値に反映させます。 </summary>
        private void LodToSliderVal()
        {
            this.TypeLodSliderDict =
                this.TypeLodDict.ToDictionary(
                    pair => pair.Key,
                    pair => new MinMax<float>(pair.Value.Min, pair.Value.Max)
                );
        }
        
        /// <summary> シリアライズするときに List形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(this.TypeLodDict, this.keys, this.values);
        }

        /// <summary> デシリアライズするときに List から Dictionary 形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.TypeLodDict = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
            LodToSliderVal();
        }
    }
}