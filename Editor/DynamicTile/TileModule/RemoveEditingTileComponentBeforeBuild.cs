using System.Threading.Tasks;
using PLATEAU.DynamicTile;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
	/// <summary>
	/// ビルド直前に <see cref="PLATEAUEditingTile"/> コンポーネントを削除します。
	/// - シーン上の編集中プレハブ（`EditingTiles` 配下）
	/// - 対応するプレハブアセット
	/// の両方から除去します。
	/// </summary>
	internal sealed class RemoveEditingTileComponentBeforeBuild : IBeforeTileAssetBuild
	{
		public async Task<bool> BeforeTileAssetBuildAsync(CancellationToken ct)
		{
			try
            {
                // シーン上の全ての PLATEAUEditingTile を探索
                var editingTiles = Object.FindObjectsByType<PLATEAUEditingTile>(FindObjectsSortMode.None);
                foreach (var editingTile in editingTiles)
                {
                    ct.ThrowIfCancellationRequested();
                    if (editingTile == null) continue;
                    var go = editingTile.gameObject;
                    // 近傍のPrefabインスタンスルートを取得
                    var instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(go);
                    if (instanceRoot == null) instanceRoot = go;

                    // 対応するプレハブアセットからも除去
                    if (PrefabUtility.IsPartOfPrefabInstance(instanceRoot))
                    {
                        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
                        if (!string.IsNullOrEmpty(prefabPath))
                        {
                            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                            if (prefabAsset != null)
                            {
                                var comp = prefabAsset.GetComponent<PLATEAUEditingTile>();
                                if (comp != null)
                                {
                                    Object.DestroyImmediate(comp, true);
                                    EditorUtility.SetDirty(prefabAsset);
                                }
                            }
                        }
                    }

                    // シーン上のコンポーネントも除去
                    Object.DestroyImmediate(editingTile, false);
                }

                AssetDatabase.SaveAssets();
                await Task.CompletedTask;
                return true;
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to remove PLATEAUEditingTile before build: {e.Message}\n{e.StackTrace}");
				return false;
			}
		}
	}
}



