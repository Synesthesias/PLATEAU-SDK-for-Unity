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
            var typeLodRange = this.TypeLodDict[gmlType];
            int min = typeLodRange.Min;
            int max = typeLodRange.Max;
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
            int min = this.TypeLodDict[t].Min;
            int max = this.TypeLodDict[t].Max;
            return (min, max);
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