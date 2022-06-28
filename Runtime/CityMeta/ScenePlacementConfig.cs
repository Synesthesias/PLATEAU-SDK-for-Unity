using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Util;
using UnityEditor.SearchService;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class ScenePlacementConfig : ISerializationCallbackReceiver
    {
        public Dictionary<GmlType, ScenePlacementConfigPerType> perTypeConfigs;
        
        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keys = new List<GmlType>();
        [SerializeField] private List<ScenePlacementConfigPerType> values = new List<ScenePlacementConfigPerType>();

        public enum PlaceMethod
        {
            /// <summary> 変換したLODをすべてシーンに配置します。 </summary>
            PlaceAllLod,
            /// <summary> 変換したLODのうち最大のものをシーンに配置します。 </summary>
            PlaceMaxLod,
            /// <summary> 変換したLODのうち最小のものをシーンに配置します。 </summary>
            PlaceMinLod,
            /// <summary> 選択したLODを配置します。そのLODが見つからなければ配置しません。 </summary>
            PlaceSelectedLodOrDoNotPlace,
            /// <summary> 選択したLODを配置します。そのLODが見つからなければ、見つかる中で最大のLODを配置します。 </summary>
            PlaceSelectedLodOrMax,
            /// <summary> シーンに配置しません。 </summary>
            DoNotPlace
        }

        public ScenePlacementConfig()
        {
            // 各タイプごとの設定を初期化します。
            this.perTypeConfigs = GmlTypeConvert.ComposeTypeDict<ScenePlacementConfigPerType>();
        }

        /// <summary> シリアライズするときに List形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(this.perTypeConfigs, this.keys, this.values);
        }

        /// <summary> デシリアライズするときに List から Dictionary 形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.perTypeConfigs = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
        }
    }

    [Serializable]
    internal class ScenePlacementConfigPerType
    {
        public ScenePlacementConfig.PlaceMethod placeMethod;
        public int selectedLod;
    }
}