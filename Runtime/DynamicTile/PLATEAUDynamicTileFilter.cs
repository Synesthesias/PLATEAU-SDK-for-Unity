using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// GameObject ON/OFFの条件を保持し、条件に基づいてフィルタリングするクラス。
    /// PLATEAUDynamicTileが保持し、Tileごとに処理を行うことで動的にロードされたオブジェクトに対応。
    /// </summary>
    public class FilterCondition
    {
        public bool IsActive { get; set; } = true;
        public int MinLod { get; set; } = 0;
        public int MaxLod { get; set; } = 0;

        public FilterCondition(bool isActive, int minLod, int maxLod)
        {
            IsActive = isActive;
            MinLod = minLod;
            MaxLod = maxLod;
        }

        /// <summary>
        /// フィルター条件に基づいてGameObjectのアクティブ状態を設定します。
        /// 現状、最小地物には未対応
        /// 地物変換実装後に、最小地物対応するか、最小地物のUIを省略するかを検討
        /// </summary>
        public void ApplyFilter(GameObject loadedObject)
        {
            if (loadedObject != null)
            {
                loadedObject.SetActive(IsActive);
                // LODフィルタリング
                for (int i = 0; i < loadedObject.transform.childCount; i++)
                {
                    var child = loadedObject.transform.GetChild(i);
                    if (child != null)
                    {
                        // LODxの形式で名前が付いている
                        if (int.TryParse(child.name.Replace("LOD", ""), out int lod))
                        {
                            child.gameObject.SetActive(lod >= MinLod && lod <= MaxLod);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// PLATEAUDynamicTileのフィルタリングを行う静的クラス。
    /// </summary>
    public static class PLATEAUDynamicTileFilter
    {
        /// <summary>
        /// GameObjectのON/OFFを行います
        /// 各TileにFilterConditionを生成し、それに基づいてTile側で処理を行います。
        /// </summary>
        /// <param name="selectionDict"></param>
        public static void FilterByCityObjectTypeAndLod(
            List<PLATEAUDynamicTile> dynamicTiles,
            ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> selectionDict,
            ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> packageToLodRangeDict)
        {
            if (dynamicTiles == null || selectionDict == null || packageToLodRangeDict == null)
                return;

            foreach (var tile in dynamicTiles)
            {
                var package = tile.Package;
                var node = CityObjectTypeHierarchy.GetNodeByPackage(package);
                if (node != null &&
                    selectionDict.TryGetValue(node, out var isActive) &&
                    packageToLodRangeDict.TryGetValue(package, out var lodRange))
                {
                    tile.FilterCondition = new FilterCondition(isActive, lodRange.minLod, lodRange.maxLod);
                }
                else
                {
                    tile.FilterCondition = null;
                }
            }
        }

        /// <summary>
        /// Addressからパッケージを取得する。
        /// </summary>
        public static PredefinedCityModelPackage GetPackage(string address)
        {
            if (string.IsNullOrEmpty(address))
                return PredefinedCityModelPackage.None;

            Match match = Regex.Match(address, @"^tile_zoom_\d+_grid_[^_]+_(.+)$");
            if (match.Success)
            {
                string originalName = match.Groups[1].Value;
                string type = originalName.Split('_').FirstOrDefault();
                if (string.IsNullOrEmpty(type))
                    return PredefinedCityModelPackage.None;
                return DatasetAccessor.FeatureTypeToPackage(type);
            }
            else
            {
                Debug.LogError("アドレスからパッケージ名が取得できませんでした。Address: " + address);
            }
            return PredefinedCityModelPackage.None;
        }
    }

}
