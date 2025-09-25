using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
                    // 念のため検索（保存先配下）
                    // var rootAssetPath = AssetPathUtil.NormalizeAssetPath(context.AssetConfig.AssetPath);
                    // var metaGuids = AssetDatabase.FindAssets("t:PLATEAUDynamicTileMetaStore", new[] { rootAssetPath });
                    // if (metaGuids != null && metaGuids.Length > 0)
                    // {
                    //     var metaPath = AssetDatabase.GUIDToAssetPath(metaGuids[0]);
                    //     existingMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(metaPath);
                    // }
                    Debug.LogError($"既存のメタデータが見つかりません。パスを確認してください: {context.DataPath}");
                    return false;
                }

                if (existingMeta != null)
                {
                    context.MetaStore.CopyFrom(existingMeta);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

			var groupName = context.AddressableGroupName;
			var metaStore = context.MetaStore;

            // シーン中の編集中タイルを検索します。
            var parent = GameObject.Find(TileRebuilder.EditingTilesParentName);
            if (parent == null)
            {
                Debug.LogError("no editing tiles parent found.");
                return false;
            }
			var editingTiles = parent.GetComponentsInChildren<PLATEAUEditingTile>(true);
			if (editingTiles == null || editingTiles.Length == 0)
            {
                Debug.LogError("no editing tiles found.");
                return false;
            }

			var processedAddresses = new HashSet<string>();
			foreach (var editingTile in editingTiles)
			{
				if (editingTile == null) continue;
				var instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(editingTile.gameObject);
				if (instanceRoot == null) instanceRoot = editingTile.gameObject;
				if (!PrefabUtility.IsPartOfPrefabInstance(instanceRoot)) continue; // プレハブでない

				var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
				if (string.IsNullOrEmpty(prefabPath)) continue;

				var address = Path.GetFileNameWithoutExtension(prefabPath);
				if (string.IsNullOrEmpty(address)) continue;
				if (!processedAddresses.Add(address)) continue; // 重複防止

				// Addressables 登録
				AddressablesUtility.RegisterAssetAsAddressable(
					prefabPath,
					address,
					groupName,
					new List<string> { DynamicTileExporter.AddressableLabel });

				// メタ情報の登録（既存メタを優先。なければ推定）
				var bounds = default(Bounds);
				var zoomLevel = 0;

				var old = metaStore?.GetMetaInfo(address);
				if (old != null)
				{
					bounds = old.Extent;
					zoomLevel = old.ZoomLevel;
				}
				else
				{
					// Prefab から Bounds を推定（Renderer合成）。ZoomLevel は不明のため 0。
					var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
					if (prefabAsset != null)
					{
						var renderers = prefabAsset.GetComponentsInChildren<Renderer>(true);
						if (renderers != null && renderers.Length > 0)
						{
							var first = renderers.FirstOrDefault();
							if (first != null)
							{
								bounds = first.bounds;
								foreach (var r in renderers)
								{
									if (r == null) continue;
									bounds.Encapsulate(r.bounds);
								}
							}
						}
					}
				}

				// 既に同アドレスが MetaStore に存在する場合は追加しない（CopyFrom 済みを維持）
				if (metaStore.GetMetaInfo(address) == null)
				{
					metaStore.AddMetaInfo(address, groupName, bounds, 0, zoomLevel);
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