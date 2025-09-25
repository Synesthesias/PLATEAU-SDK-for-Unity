using PLATEAU.DynamicTile;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
	/// <summary>
	/// タイルマネージャーの初期化とシーンビューカメラのフォーカスを行う事後処理。
	/// 他のフローでも使い回せるように <see cref="IAfterTileAssetBuild"/> として分離。
	/// </summary>
	public class InitializeTileManagerAndFocus : IAfterTileAssetBuild
	{
		public bool AfterTileAssetBuild()
		{
			var manager = Object.FindObjectOfType<PLATEAUTileManager>();
			if (manager == null)
			{
				Debug.LogError("PLATEAUTileManager がシーンに見つかりませんでした。");
				return false;
			}

			// タイルを初期化します。
			PLATEAUSceneViewCameraTracker.Initialize();
			manager.InitializeTiles().Wait();

			// タイルのある場所にシーンビューカメラをフォーカスします。
			TileManagerGenerator.FocusSceneViewCameraToTiles(manager);
			return true;
		}
	}
}


