using PLATEAU.Editor.DynamicTile;
using PLATEAU.DynamicTile;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
	/// <summary>
	/// ビルド完了後にシーン上のEditingTilesルートを削除します。
	/// </summary>
	internal class CleanupEditingTilesInScene : IAfterTileAssetBuild
	{
		public bool AfterTileAssetBuild()
		{
            // TileManagerの子である編集中タイルを削除します。
			var editingTiles = GameObject.Find(TileRebuilder.EditingTilesParentName);
			if (editingTiles != null)
			{
				var parentTransform = editingTiles.transform.parent;
				if (parentTransform != null && parentTransform.GetComponent<PLATEAUTileManager>() != null)
				{
					Object.DestroyImmediate(editingTiles);
				}
			}
			return true;
		}
	}
}


