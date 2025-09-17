using PLATEAU.DynamicTile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    internal class TilePrefabGetter
    {

        /// <summary>
        /// メタから元になったプレハブを取得します。
        /// </summary>
        public TilePrefab[] GetTilePrefabsFromMeta(PLATEAUDynamicTileMetaStore meta, string groupName)
        {
            var addresses = GetDistinctAddressesFromMeta(meta);
            if (addresses == null) return Array.Empty<TilePrefab>();
            var prefabs = new List<TilePrefab>();
            foreach (var address in addresses)
            {
                var prefab = GetPrefabFromAddress(address, groupName);
                if (prefab == null) continue;
                prefabs.Add(prefab);
            }

            return prefabs.ToArray();
        }

        public TilePrefab GetTilePrefabFromTile(PLATEAUDynamicTile tile)
        {
            return GetPrefabFromAddress(tile.Address, tile.GroupName);
        }

        private TilePrefab GetPrefabFromAddress(string address, string groupName)
        {
            var guids = AssetDatabase.FindAssets($"t:Prefab {address}");
            if (guids != null && guids.Length > 0)
            {
                foreach(var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path)) continue;
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (!string.Equals(name, address, StringComparison.Ordinal)) continue;
                    var prefab = new TilePrefab(path, address, groupName);
                    return prefab;
                }
            }
            return null;
        }
        
        /// <summary>
        /// メタストアから重複のないアドレス一覧を抽出します。
        /// </summary>
        public List<string> GetDistinctAddressesFromMeta(PLATEAUDynamicTileMetaStore meta)
        {
            if (meta == null || meta.TileMetaInfos == null) return new List<string>();
            return meta.TileMetaInfos
                .Where(i => i != null && !string.IsNullOrEmpty(i.AddressName))
                .Select(i => i.AddressName)
                .Distinct()
                .ToList();
        }
        
    }
    
    internal class TilePrefab
    {
        public TilePrefab(string path, string address, string groupName)
        {
            Path = path;
            Address = address;
            GroupName = groupName;
        }
        public string Path { get; }
        public string Address { get; }
        public string GroupName { get; }
    }
}