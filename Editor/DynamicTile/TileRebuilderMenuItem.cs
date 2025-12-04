using PLATEAU.DynamicTile;
using PLATEAU.Util.Async;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.DynamicTile
{
    /// <summary>
    /// <see cref="TileRebuilder"/>の機能をメニューバーから実行して動作確認できるようにするためのクラスです。
    /// </summary>
    public static class TileRebuilderMenuItem
    {
        /// <summary>
        /// タイルのプレハブをすべてシーンに配置します。
        /// </summary>
        [MenuItem("PLATEAU/Debug/Tile Prefabs To Scene (All)")]
        public static void TilePrefabsToSceneAll()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            var dummyCancelToken = new CancellationTokenSource().Token; // メニューバーからの実行ではキャンセルは不要とします。
            new TileRebuilder().TilePrefabsToScene(manager, dummyCancelToken).ContinueWithErrorCatch();
        }

        /// <summary>
        /// シーンに配置したタイルのプレハブを適用してすべてリビルドします。
        /// </summary>
        [MenuItem("PLATEAU/Debug/Rebuild Tiles (All)")]
        public static void RebuildInSceneAll()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            new TileRebuilder().Rebuild(manager).ContinueWithErrorCatch();
        }

        /// <summary>
        /// タイルのプレハブのうち選択中のものだけをシーンに配置します。
        /// </summary>
        [MenuItem("PLATEAU/Debug/Tile Prefabs To Scene (Selected Tiles)")]
        public static void TilePrefabsToSceneSelected()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            var selected = GetSelectedTiles(manager);
            if (selected == null || selected.Count == 0)
            {
                Debug.LogWarning("No selected tiles found in the scene.");
                return;
            }

            var dummyCancelToken = new CancellationTokenSource().Token; // メニューバーからの実行ではキャンセルは不要とします。
            new TileRebuilder().TilePrefabsToScene(manager, selected, dummyCancelToken).ContinueWithErrorCatch();
        }

        /// <summary>
        /// シーンに配置したタイルのプレハブのうち、選択中のものだけを差分ビルドします。
        /// </summary>
        [MenuItem("PLATEAU/Debug/Rebuild Tiles (Selected Tiles)")]
        public static void RebuildInSceneSelected()
        {
            var manager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (manager == null)
            {
                Debug.LogError("tile manager is not found.");
                return;
            }

            var selected = GetSelectedTiles(manager);
            if (selected == null || selected.Count == 0)
            {
                Debug.LogWarning("No selected tiles found in the scene.");
                return;
            }

            new TileRebuilder().RebuildByTiles(manager, selected).ContinueWithErrorCatch();
        }

        private static System.Collections.Generic.List<PLATEAUDynamicTile> GetSelectedTiles(PLATEAUTileManager manager)
        {
            var result = new System.Collections.Generic.List<PLATEAUDynamicTile>();
            var selectedGos = Selection.gameObjects;
            if (selectedGos == null || selectedGos.Length == 0) return result;

            foreach (var tile in manager.DynamicTiles)
            {
                if (tile == null) continue;
                var root = tile.LoadedObject;
                if (root == null) continue;
                var rootTf = root.transform;
                foreach (var go in selectedGos)
                {
                    if (go == null) continue;
                    var tf = go.transform;
                    if (tf == rootTf || tf.IsChildOf(rootTf))
                    {
                        result.Add(tile);
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// シーンに配置したGameObjectからPrefabを生成して保存します。
        /// </summary>
        [MenuItem("PLATEAU/Debug/Convert And Save Tile Asset")]
        public static void ConvertAndSaveAsset()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                var manager = Object.FindObjectOfType<PLATEAUTileManager>();
                if (manager == null)
                {
                    Debug.LogError("tile manager is not found.");
                    return;
                }
                new TileRebuilder().SavePrefabAsset(manager, selected).ContinueWithErrorCatch();
            }

        }
    }
}
