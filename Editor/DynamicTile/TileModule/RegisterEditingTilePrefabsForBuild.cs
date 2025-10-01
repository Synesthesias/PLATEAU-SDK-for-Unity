using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 編集中の動的タイルのプレハブについて、Addressableとして再ビルドするためにAddressable登録＆メタ登録を行います。
    /// </summary>
    public class RegisterEditingTilePrefabsForBuild : IOnTileGenerateStart
    {
        private readonly DynamicTileProcessingContext context;

        public RegisterEditingTilePrefabsForBuild(DynamicTileProcessingContext context)
        {
            this.context = context;
        }
        
        public bool OnTileGenerateStart()
        {
			// 既存のメタがあれば取り込み（上書きせず維持）
            try
            {
                var existingMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(context.DataPath);
                if (existingMeta == null)
                {
                    Debug.LogError($"既存のメタデータが見つかりません。パスを確認してください: {context.DataPath}");
                    return false;
                }
                context.MetaStore.CopyFrom(existingMeta);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var groupName = context.AddressableGroupName;
            var metaStore = context.MetaStore;

            // シーン中の編集中タイルを検索します。
            var processedAddresses = new HashSet<string>();

            // 1) 既存メタに含まれる全アドレスをAddressablesに登録（非選択も含む）
            var getter = new TilePrefabGetter();
            var allAddresses = getter.GetDistinctAddressesFromMeta(metaStore);
            foreach (var addr in allAddresses)
            {
                if (string.IsNullOrEmpty(addr)) continue;
                if (!processedAddresses.Add(addr)) continue;
                var prefab = getter.GetPrefabFromAddress(addr, groupName);
                if (prefab == null || string.IsNullOrEmpty(prefab.Path)) continue;
                AddressablesUtility.RegisterAssetAsAddressable(
                    prefab.Path,
                    addr,
                    groupName,
                    new List<string> { DynamicTileExporter.AddressableLabel });
            }

            // 2) シーン上の編集中プレハブも登録（新規やメタ外の可能性に対処）。
            var parent = GameObject.Find(TileRebuilder.EditingTilesParentName);
            if (parent != null)
            {
                var editingTiles = parent.GetComponentsInChildren<PLATEAUEditingTile>(true);
                if (editingTiles != null && editingTiles.Length > 0)
                {
                    foreach (var editingTile in editingTiles)
                    {
                        if (editingTile == null) continue;
                        var instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(editingTile.gameObject);
                        if (instanceRoot == null) instanceRoot = editingTile.gameObject;
                        if (!PrefabUtility.IsPartOfPrefabInstance(instanceRoot)) continue;

                        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
                        if (string.IsNullOrEmpty(prefabPath)) continue;
                        var address = Path.GetFileNameWithoutExtension(prefabPath);
                        if (string.IsNullOrEmpty(address)) continue;
                        if (!processedAddresses.Add(address)) continue;

                        AddressablesUtility.RegisterAssetAsAddressable(
                            prefabPath,
                            address,
                            groupName,
                            new List<string> { DynamicTileExporter.AddressableLabel });
                    }
                }
            }
            
            return true;
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
        }
    }
}