using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PLATEAU.Util;
using UnityEngine;
using static PLATEAU.CityMeta.ScenePlacementConfig;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// 3Dモデルをロードして現在のシーンに配置する設定です。
    /// 
    /// 目的は以下の2つです。
    /// ・モデル配置時に ScenePlacementGUI からユーザー選択の設定を受け取り、 CityMeshPlacerToScene に渡すこと
    /// ・インポート時の設定を保存する目的で SerializeField を保持すること。 
    /// </summary>
    [Serializable]
    internal class ScenePlacementConfig : ISerializationCallbackReceiver
    {
        private Dictionary<GmlType, ScenePlacementConfigPerType> PerTypeConfigs;
        
        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keys = new List<GmlType>();
        [SerializeField] private List<ScenePlacementConfigPerType> values = new List<ScenePlacementConfigPerType>();

        public enum PlaceMethod
        {
            /// <summary> 変換したLODをすべてシーンに配置します。 </summary>
            PlaceAllLod,
            /// <summary> 変換したLODのうち最大のものをシーンに配置します。 </summary>
            PlaceMaxLod,
            /// <summary> 選択したLODを配置します。そのLODが見つからなければ配置しません。 </summary>
            PlaceSelectedLodOrDoNotPlace,
            /// <summary> 選択したLODを配置します。そのLODが見つからなければ、見つかる中で最大のLODを配置します。 </summary>
            PlaceSelectedLodOrMax,
            /// <summary> シーンに配置しません。 </summary>
            DoNotPlace
        }

        public static readonly string[] PlaceMethodDisplay = new string[]
        {
            "全LODを配置", "最大LODを配置", "選択LODを配置、なければ配置しない", "選択LODを配置、なければ最大LODを配置", "配置しない"
        };

        public ScenePlacementConfig()
        {
            // 各タイプごとの設定を初期化します。
            this.PerTypeConfigs = GmlTypeConvert.ComposeTypeDict<ScenePlacementConfigPerType>();
        }

        // TODO このメソッドを使って一括設定のGUIを作ると便利かも？
        public void SetPlaceMethodForAllTypes(PlaceMethod placeMethod)
        {
            var dict = this.PerTypeConfigs;
            foreach (var type in dict.Keys)
            {
                dict[type].placeMethod = placeMethod;
            }
        }

        // TODO このメソッドを使って一括設定のGUIを作ると便利かも？
        public void SetSelectedLodForAllTypes(int lod)
        {
            var dict = this.PerTypeConfigs;
            foreach (var type in dict.Keys)
            {
                dict[type].selectedLod = lod;
            }
        }

        public ScenePlacementConfigPerType GetPerTypeConfig(GmlType type)
        {
            return this.PerTypeConfigs[type];
        }

        public IReadOnlyList<GmlType> AllGmlTypes()
        {
            return this.PerTypeConfigs.Keys.ToArray();
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
        
    }

    /// <summary>
    /// <see cref="ScenePlacementConfig"/> の 1タイプあたりの設定項目です。
    /// </summary>
    [Serializable]
    internal class ScenePlacementConfigPerType
    {
        public PlaceMethod placeMethod;
        public int selectedLod;
    }

    internal static class PlaceMethodExtension
    {
        /// <summary>
        /// 設定項目で <see cref="ScenePlacementConfigPerType.selectedLod"/> を使うかどうかは
        /// <see cref="ScenePlacementConfig.PlaceMethod"/> に依るので、使うかどうかを返します。
        /// </summary>
        public static bool DoUseSelectedLod(this PlaceMethod method)
        {
            return method == PlaceMethod.PlaceSelectedLodOrMax ||
                   method == PlaceMethod.PlaceSelectedLodOrDoNotPlace;
        }
    }
}