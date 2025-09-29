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
        public FilterCondition FilterCondition = null; //フィルタリング

        public PLATEAUCityModelData()
        {
        }
    }

    /// <summary>
    /// フィルター条件を保持するクラスです。
    /// </summary>
    [Serializable]
    public class FilterCondition
    {
        public bool DisableDuplicate;
        public List<FilterSelectionItem> SelectionList;
        public List<FilterPackageLodItem> PackageLodList;

        public FilterCondition()
        {
            DisableDuplicate = true;
            SelectionList = new();
            PackageLodList = new();
        }

        /// <param name="dict"><see cref="FilterConditionGui"/>のselectionDict</param>
        /// <param name="lodDict"><see cref="FilterConditionGui"/>のsliderPackageLod</param>
        public FilterCondition(bool disableDuplicate, ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> sel, ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> lod)
        {
            DisableDuplicate = disableDuplicate;
            if (sel != null) SetSelectionDict(sel); else SelectionList = new();
            if (lod != null) SetPackageLodDict(lod); else PackageLodList = new();
        }

        private void SetSelectionDict(ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> dict)
        {
            if (dict == null)
            {
                SelectionList = new(); 
                return;
            }
            SelectionList = dict
            .OrderBy(kv => kv.Key.GetDisplayName(), StringComparer.Ordinal)
            .Select(kv => new FilterSelectionItem { Key = kv.Key.GetDisplayName(), Value = kv.Value })
            .ToList();
        }

        private void SetPackageLodDict(ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> dict)
        {
            if (dict == null) 
            { 
                PackageLodList = new(); 
                return; 
            }
            PackageLodList = dict
            .OrderBy(kv => kv.Key)
            .Select(kv => new FilterPackageLodItem { Key = kv.Key, Value = new LodMinMax(kv.Value.minLod, kv.Value.maxLod) })
            .ToList();
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
    public class FilterSelectionItem
    {
        public string Key;
        public bool Value;
    }

    [Serializable]
    public class FilterPackageLodItem
    {
        public PredefinedCityModelPackage Key;
        public LodMinMax Value;
    }

}
