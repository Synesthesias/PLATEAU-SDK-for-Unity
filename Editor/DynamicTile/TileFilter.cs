using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.DynamicTile;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    public static class TileFilter
    {
        /// <summary>
        /// Tileの元アセットとなるPrefab内要素のON/OFFの更新を行います。
        /// 各TileにFilterConditionを生成し、それに基づいたON/OFFを行います
        /// </summary>
        /// <param name="selectionDict"></param>
        public static async Task FilterByCityObjectTypeAndLod(
            PLATEAUTileManager tileManager,
            ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool> selectionDict,
            ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> packageToLodRangeDict,
            CancellationToken ct)
        {
            var dynamicTiles = tileManager.DynamicTiles;
            if (dynamicTiles == null || selectionDict == null || packageToLodRangeDict == null)
                return;

            List<PLATEAUDynamicTile> selectedTiles = new List<PLATEAUDynamicTile>();
            List<TileFilterCondition> filterConditions = new List<TileFilterCondition>();

            foreach (var tile in dynamicTiles)
            {
                var package = tile.Package;
                var node = CityObjectTypeHierarchy.GetNodeByPackage(package);
                if (node != null &&
                    selectionDict.TryGetValue(node, out var isActive) &&
                    packageToLodRangeDict.TryGetValue(package, out var lodRange))
                {
                    var cond = new TileFilterCondition(tile, isActive, lodRange.minLod, lodRange.maxLod);
                    selectedTiles.Add(tile);
                    filterConditions.Add(cond);
                }
            }

            var rebuilder = new TileRebuilder();
            await rebuilder.TilePrefabsToScene(tileManager, selectedTiles, ct);
            var editingTiles = tileManager.transform.Find(TileRebuilder.EditingTilesParentName);
            // 各Prefabにフィルター条件を適用
            foreach (var cond in filterConditions)
            {
                cond.ApplyFilter(editingTiles.transform.Find(cond.Tile.Address)?.gameObject);
            }
            await rebuilder.RebuildByTiles(tileManager, selectedTiles);
        }
    }

    /// <summary>
    /// 一時的にTileのフィルター条件を保持し、GameObjectに適用するためのクラス
    /// 最小地物未対応
    /// </summary>
    internal class TileFilterCondition
    {
        public PLATEAUDynamicTile Tile { get; set; }
        public bool IsActive { get; set; } = true;
        public int MinLod { get; set; } = 0;
        public int MaxLod { get; set; } = 0;

        public TileFilterCondition(PLATEAUDynamicTile tile, bool isActive, int minLod, int maxLod)
        {
            Tile = tile;
            IsActive = isActive;
            MinLod = minLod;
            MaxLod = maxLod;
        }

        /// <summary>
        /// フィルター条件に基づいてGameObjectのアクティブ状態を設定します。
        /// 現状、最小地物には未対応
        /// 地物変換実装後に、最小地物対応するか、最小地物のUIを省略するかを検討
        /// </summary>
        public void ApplyFilter(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(IsActive);
                // LODフィルタリング
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    var child = obj.transform.GetChild(i);
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
}
