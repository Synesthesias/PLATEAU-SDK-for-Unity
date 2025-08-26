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

    public class PLATEAUDynamicTileFilter
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
            dynamicTiles.ForEach(tile =>
            {
                var package = tile.Package;
                if (selectionDict.TryGetValue(CityObjectTypeHierarchy.GetNodeByPackage(package), out var isActive))
                {
                    tile.FilterCondition = new FilterCondition(isActive, packageToLodRangeDict[package].minLod, packageToLodRangeDict[package].maxLod);
                }
                else
                {
                    tile.FilterCondition = null;
                }
            });
        }

        /// <summary>
        /// Addressからパッケージを取得する。
        /// </summary>
        public static PredefinedCityModelPackage GetPackage(string address)
        {
            if (string.IsNullOrEmpty(address))
                return PredefinedCityModelPackage.None;

            Match match = Regex.Match(address, @"^tile_zoom_\d+_grid_\d+_(.+)$");
            if (match.Success)
            {
                string originalName = match.Groups[1].Value;
                string type = originalName.Split('_').FirstOrDefault();
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
