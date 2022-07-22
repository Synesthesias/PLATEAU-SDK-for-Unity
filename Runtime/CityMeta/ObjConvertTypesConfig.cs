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
    /// 具体的には 地物タイプ別 LOD 範囲設定、　地物タイプ別モード設定 (1.LOD範囲内複数 or 2.最大LODのみ) です。
    /// 設定は辞書形式で、 <see cref="GmlType"/> => 値 の形式で保持します。
    /// <see cref="CityImportConfig"/> がこのクラスを保持します。
    /// </summary>
    [Serializable]
    internal class ObjConvertTypesConfig : ISerializationCallbackReceiver
    {
        private Dictionary<GmlType, MinMax<int>> typeLodDict = GmlTypeConvert.ComposeTypeDict<MinMax<int>>();
        public Dictionary<GmlType, bool> TypeExportLowerLodDict = GmlTypeConvert.ComposeTypeDict(true);

        /// <summary> GUIでスライダーの値を一時的保持するためのメンバ </summary>
        [NonSerialized] public Dictionary<GmlType, MinMax<float>> TypeLodSliderDict = new Dictionary<GmlType, MinMax<float>>();

        // シリアライズ時に Dictionary を List形式にします。
        [SerializeField] private List<GmlType> keysLod = new List<GmlType>();
        [SerializeField] private List<MinMax<int>> valuesLod = new List<MinMax<int>>();
        [SerializeField] private List<GmlType> keysExportLower = new List<GmlType>();
        [SerializeField] private List<bool> valuesExportLower = new List<bool>();

        public GmlType[] GmlTypes => this.typeLodDict.Keys.ToArray();
        
        public ObjConvertTypesConfig()
        {
            LodToSliderVal();
        }

        public MinMax<int> GetLodRange(GmlType gmlType)
        {
            return this.typeLodDict[gmlType];
        }

        public void SetLodRange(GmlType gmlType, MinMax<int> minmax)
        {
            this.typeLodDict[gmlType] = minmax;
        }

        /// <summary> Lod設定の値をGUIスライダー用値に反映させます。 </summary>
        private void LodToSliderVal()
        {
            this.TypeLodSliderDict =
                this.typeLodDict.ToDictionary(
                    pair => pair.Key,
                    pair => new MinMax<float>(pair.Value.Min, pair.Value.Max)
                );
        }

        /// <summary>
        /// LOD設定が、仕様上ありえる範囲を超えている場合は、ありえる範囲まで設定範囲を縮めます。
        /// </summary>
        public void ClampLodRangeToPossibleVal()
        {
            foreach (var pair in this.typeLodDict)
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
            var typeLodRange = this.typeLodDict[gmlType];
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
            int min = this.typeLodDict[t].Min;
            int max = this.typeLodDict[t].Max;
            return (min, max);
        }

        public void SetLodRangeByFunc(Func<GmlType, MinMax<int>> gmlTypeToLodRangeFunc)
        {
            foreach (var type in this.typeLodDict.Keys)
            {
                this.typeLodDict[type].SetMinMax(gmlTypeToLodRangeFunc(type));
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
            DictionarySerializer.OnBeforeSerialize(this.typeLodDict, this.keysLod, this.valuesLod);
            DictionarySerializer.OnBeforeSerialize(this.TypeExportLowerLodDict, this.keysExportLower, this.valuesExportLower);
        }

        /// <summary> デシリアライズするときに List から Dictionary 形式に直します。 </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.typeLodDict = DictionarySerializer.OnAfterSerialize(this.keysLod, this.valuesLod);
            this.TypeExportLowerLodDict =
                DictionarySerializer.OnAfterSerialize(this.keysExportLower, this.valuesExportLower);
            LodToSliderVal();
        }
        
    }
}