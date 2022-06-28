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
    // 他クラスとの関係は CityImporterConfig -> 保持 -> GmlSearcherConfig -> 保持 -> GmlTypeTarget -> 保持 -> ImportGmlTypeConfig
    // という関係なので、 CityImporterConfig の注意事項に基づいてこのクラスには Serializable属性が付いている必要があります。
    
    [Serializable]
    internal class GmlTypeTarget : ISerializationCallbackReceiver
    {
        /// <summary> GmlTypeごとの設定の辞書です。 </summary>
        public Dictionary<GmlType, ImportGmlTypeConfig> GmlTypeConfigs { get; set; }

        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keys = new List<GmlType>();
        [SerializeField] private List<ImportGmlTypeConfig> values = new List<ImportGmlTypeConfig>();

        public GmlTypeTarget()
        {
            // 各タイプごとに ImportGmlTypeConfig を初期化します。
            GmlTypeConfigs =
                Enum.GetValues(typeof(GmlType))
                    .OfType<GmlType>()
                    .ToDictionary(t => t, _ => new ImportGmlTypeConfig());
        }

        /// <summary>
        /// 与えられたタイプが変換対象かどうかを返します。
        /// </summary>
        public bool IsTypeTarget(GmlType t)
        {
            return GmlTypeConfigs[t].isTarget;
        }

        /// <summary> 地物タイプを変換対象とするかについて、すべて true または すべて false にします。 </summary>
        public void SetAllTarget(bool val)
        {
            foreach (var key in GmlTypeConfigs.Keys.ToArray())
            {
                GmlTypeConfigs[key].isTarget = val;
            }
        }

        /// <summary>
        /// 現在の <see cref="GmlTypeTarget"/> の設定において、
        /// 指定のgmlファイルから何という名前の .obj ファイルが生成されるかを配列で返します。
        /// 例えば、abc.gml という名前のgmlファイルからは、LOD設定によって
        /// LOD0_abc.obj, LOD1_abc.obj などの0個以上の .obj ファイルが生成されます。
        /// </summary>
        public string[] ObjFileNamesForGml(string gmlFile)
        {
            string gmlFileWithoutExtension = GmlFileNameParser.FileNameWithoutExtension(gmlFile);
            var gmlType = GmlFileNameParser.GetGmlTypeEnum(gmlFile);
            var typeConf = GmlTypeConfigs[gmlType];
            int min = typeConf.minLod;
            int max = typeConf.maxLod;
            if (min > max) throw new Exception("Error. min > max.");
            var objFileNames = new List<string>();
            for (int i = min; i <= max; i++)
            {
                objFileNames.Add($"LOD{i}_{gmlFileWithoutExtension}");
            }

            return objFileNames.ToArray();
        }

        public (int min, int max) GetMinMaxLodForType(GmlType t)
        {
            int min = GmlTypeConfigs[t].minLod;
            int max = GmlTypeConfigs[t].maxLod;
            return (min, max);
        }

        /// <summary>
        /// シリアライズするときに List形式に直します。
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(GmlTypeConfigs, this.keys, this.values);
        }


        /// <summary>
        /// デシリアライズするときに List から Dictionary 形式に直します。
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            GmlTypeConfigs = DictionarySerializer.OnAfterSerialize(this.keys, this.values);
        }
        
    }
}