using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CommonDataStructure;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// インポート時の Objファイル変換について、地物タイプ別の変換設定を保持するクラスです。
    /// 具体的には 地物タイプ別 LOD 範囲設定、　地物タイプ別 (LOD範囲内複数 or 最大LODのみ) 出力選択設定です。
    /// 設定は辞書形式で、 <see cref="GmlType"/> => 値 の形式で保持します。
    /// <see cref="CityImportConfig"/> がこのクラスを保持します。
    /// </summary>
    [Serializable]
    internal class ObjConvertTypesConfig : ISerializationCallbackReceiver
    {
        public Dictionary<GmlType, MinMax<int>> TypeLodDict = GmlTypeConvert.ComposeTypeDict<MinMax<int>>();
        public Dictionary<GmlType, bool> TypeExportLowerLodDict = GmlTypeConvert.ComposeTypeDict(true);

        /// <summary> GUIでスライダーの値を一時的保持するためのメンバ </summary>
        [NonSerialized] public Dictionary<GmlType, MinMax<float>> TypeLodSliderDict = new Dictionary<GmlType, MinMax<float>>();

        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keysLod = new List<GmlType>();
        [SerializeField] private List<MinMax<int>> valuesLod = new List<MinMax<int>>();
        [SerializeField] private List<GmlType> keysExportLower = new List<GmlType>();
        [SerializeField] private List<bool> valuesExportLower = new List<bool>();


        public ObjConvertTypesConfig()
        {
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
        /// LOD設定が、仕様上ありえる範囲を超えている場合は、ありえる範囲まで設定範囲を縮めます。
        /// </summary>
        public void ClampLodRangeToPossibleVal()
        {
            foreach (var pair in this.TypeLodDict)
            {
                var type = pair.Key;
                var possibleMinMax = type.PossibleLodRange();
                var lodVal = pair.Value;
                int min = Math.Max(lodVal.Min, possibleMinMax.Min);
                int max = Math.Min(lodVal.Max, possibleMinMax.Max);
                lodVal.SetMinMax(min, max);
            }

            foreach (var pair in this.TypeLodSliderDict)
            {
                var type = pair.Key;
                var possibleMinMax = type.PossibleLodRange();
                var sliderVal = pair.Value;
                float min = Math.Max(sliderVal.Min, possibleMinMax.Min);
                float max = Math.Min(sliderVal.Max, possibleMinMax.Max);
                sliderVal.SetMinMax(min, max);
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

        public void SetLodRangeByFunc(Func<GmlType, MinMax<int>> gmlTypeToLodRangeFunc)
        {
            foreach (var type in this.TypeLodDict.Keys)
            {
                this.TypeLodDict[type].SetMinMax(gmlTypeToLodRangeFunc(type));
            }
            LodToSliderVal();
        }

        /// <summary> 各タイプのLOD範囲を、仕様上ありうる範囲すべてに設定します。 </summary>
        public void SetLodRangeToAllRange()
        {
            SetLodRangeByFunc(type => type.PossibleLodRange().ToWritable);
        }
        
        /// <summary> 各タイプのLOD範囲を、仕様上の最小LODのみに設定します。 </summary>
        public void SetLodRangeToOnlyMin()
        {
            SetLodRangeByFunc(type =>
            {
                int min = type.PossibleLodRange().Min;
                return new MinMax<int>(min, min);
            });
        }

        /// <summary> 各タイプのLOD範囲を、仕様上の最大LODのみに設定します。 </summary>
        public void SetLodRangeToOnlyMax()
        {
            SetLodRangeByFunc(type =>
            {
                int max = type.PossibleLodRange().Max;
                return new MinMax<int>(max, max);
            });
        }
        
        // TODO このメソッドを使って、 ExportLowerLod のGUIで一括選択ボタンを置いた方が便利そう
        public void SetExportLowerLodForAllTypes(bool exportLower)
        {
            var dict = this.TypeExportLowerLodDict;
            foreach (var type in dict.Keys.ToArray())
            {
                dict[type] = exportLower;
            }
        }
        
        /// <summary> シリアライズするときに List形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(this.TypeLodDict, this.keysLod, this.valuesLod);
            DictionarySerializer.OnBeforeSerialize(this.TypeExportLowerLodDict, this.keysExportLower, this.valuesExportLower);
        }

        /// <summary> デシリアライズするときに List から Dictionary 形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.TypeLodDict = DictionarySerializer.OnAfterSerialize(this.keysLod, this.valuesLod);
            this.TypeExportLowerLodDict =
                DictionarySerializer.OnAfterSerialize(this.keysExportLower, this.valuesExportLower);
            LodToSliderVal();
        }
    }
}