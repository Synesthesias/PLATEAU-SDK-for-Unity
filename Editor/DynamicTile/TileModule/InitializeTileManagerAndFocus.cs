using PLATEAU.DynamicTile;
using UnityEditor;
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
			

            // タイルを初期化します。
            PLATEAUSceneViewCameraTracker.Initialize();
            
            EditorApplication.delayCall += () =>
            {
                var manager = Object.FindObjectOfType<PLATEAUTileManager>();
                if (manager == null)
                {
                    Debug.LogError("PLATEAUTileManager がシーンに見つかりませんでした。");
                    return;
                }

                var task = manager.InitializeTiles();
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        EditorApplication.delayCall += () =>
                            Debug.LogError(
                                $"タイル初期化で例外: \n{t.Exception?.GetBaseException()?.Message}\n{t.Exception?.GetBaseException()?.StackTrace}");
                        return;
                    }

                    EditorApplication.delayCall += () =>
                    {
                        if (manager != null)
                        {
                            TileManagerGenerator.FocusSceneViewCameraToTiles(manager);
                        }
                    };
                });
            };
            
            EditorApplication.QueuePlayerLoopUpdate();
			return true;
		}
	}
}


