using System;
using System.Collections.Generic;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class ScenePlacementConfig : ISerializationCallbackReceiver
    {
        public Dictionary<GmlType, ScenePlacementConfigPerType> PerTypeConfigs;
        
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
            this.PerTypeConfigs = GmlTypeConvert.ComposeTypeDict<ScenePlacementConfigPerType>();
        }

        /// <summary> シリアライズするときに List形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(this.PerTypeConfigs, this.keys, this.values);
        }

        /// <summary> デシリアライズするときに List から Dictionary 形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.PerTypeConfigs = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
        }

        /// <summary>
        /// 設定の <see cref="ScenePlacementConfigPerType.selectedLod"/> を使うかどうかは
        /// <see cref="PlaceMethod"/> に依るので、使うかどうかを返します。
        /// </summary>
        public static bool DoUseSelectedLod(PlaceMethod method)
        {
            return method == PlaceMethod.PlaceSelectedLodOrMax ||
                   method == PlaceMethod.PlaceSelectedLodOrDoNotPlace;
        }
    }

    [Serializable]
    internal class ScenePlacementConfigPerType
    {
        public ScenePlacementConfig.PlaceMethod placeMethod;
        public int selectedLod;
    }
}