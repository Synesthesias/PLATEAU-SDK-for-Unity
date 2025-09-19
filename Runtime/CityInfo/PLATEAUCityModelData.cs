using PLATEAU.Dataset;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PLATEAU.CityInfo
{

    /// <summary>
    /// シーンと共に保存される都市モデルの一時情報を保持するクラスです。
    /// 現状はフィルタリング条件のみを保持。その他の情報も将来的にここに保持する可能性があります。
    /// </summary>
    [Serializable]
    public class PLATEAUCityModelData
    {
        public FilterCondition FilterCondition; //フィルタリング

        public PLATEAUCityModelData()
        {
            FilterCondition = new FilterCondition();
        }
    }

    /// <summary>
    /// フィルター条件を保持するクラスです。
    /// </summary>
    [Serializable]
    public class FilterCondition
    {
        public bool HasData = false;
        public bool DisableDuplicate = true;
        public List<SerializableKeyValuePair<string, bool>> SelectionList = null;
        public List<SerializableKeyValuePair<PredefinedCityModelPackage, LodMinMax>> PackageLodList = null;

        public void Clear()
        {
            HasData = false;
            DisableDuplicate = true;
            SelectionList = null;
            PackageLodList = null;
        }

        public void SetData(bool disableDuplicate, ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> sel, ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> lod)
        {
            HasData = true;
            DisableDuplicate = disableDuplicate;
            SetSelectionDict(sel);
            SetPackageLodDict(lod);
        }

        private void SetSelectionDict(ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> dict)
        {
            var list = dict.ToList();
            SelectionList = list.Select(kv => new SerializableKeyValuePair<string, bool> { Key = kv.Key.GetDisplayName(), Value = kv.Value }).ToList();
        }

        private void SetPackageLodDict(ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> dict)
        {
            var list = dict.ToList();
            PackageLodList = list.Select(kv => new SerializableKeyValuePair<PredefinedCityModelPackage, LodMinMax> { Key = kv.Key, Value = new LodMinMax(kv.Value.minLod, kv.Value.maxLod) }).ToList();
        }
    }

    [Serializable]
    public class LodMinMax
    {
        public int Min;
        public int Max;
        public LodMinMax(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }

}
