using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    // 主にUI等でで使用する各種処理のまとめ
    internal class TileConvertCommon
    {
        /// <summary>
        /// observableSelectedに含まれるタイルをハイライトする
        /// </summary>
        /// <param name="forceLoad"></param>
        /// <returns></returns>
        public static async Task HighlightSelectedTiles(ObservableCollection<TileSelectionItem> observableSelectedTiles, PLATEAUTileManager tileManager, bool forceLoad = true)
        {
            var transforms = new List<Transform>();
            if (forceLoad)
            {
                var allTransforms = await TileConvertCommon.GetTransformListFromAddr<Component>(observableSelectedTiles, tileManager);
                transforms = allTransforms.Where(trans => observableSelectedTiles
                    .Any(selected => trans.GetPathToParent(selected.TileAddress) == selected.TilePath || trans.FindChildOfNamedParent(PLATEAUTileManager.TileParentName)?.name == selected.TilePath)).ToList();
            }
            else
            {
                transforms = TileConvertCommon.GetTransformsFromTiles(TileConvertCommon.GetSelectedTiles(observableSelectedTiles, tileManager));
            }
            Selection.objects = transforms.Select(trans => trans.gameObject).Cast<UnityEngine.Object>().ToArray();
        }

        /// <summary>
        /// observableSelectedのAddressに含まれるタイルを読み込んで、Transformリストを取得する
        /// LODにPLATEAUCityObjectGroupが設定されていないので、全てのコンポーネントを対象とする
        /// </summary>
        /// <returns></returns>
        public static async Task<UniqueParentTransformList> GetSelectionAsTransformList(ObservableCollection<TileSelectionItem> observableSelectedTiles, PLATEAUTileManager tileManager)
        {
            var transforms = await GetTransformListFromAddr<Component>(observableSelectedTiles, tileManager);
            return new UniqueParentTransformList(transforms);
        }

        /// <summary>
        /// Addressに含まれるタイルを読み込んで、Transformリストを取得する
        /// </summary>
        /// <typeparam name="T">指定したタイプのコンポーネントを持つGameObjectのみが対象</typeparam>
        /// <returns></returns>
        public static async Task<List<Transform>> GetTransformListFromAddr<T>(IList<TileSelectionItem> addr, PLATEAUTileManager tileManager) where T : Component
        {
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    var addresses = addr.ToList().Select(x => x.TileAddress).Distinct().ToList();
                    var selectedTiles = await tileManager.ForceLoadTiles(addresses, cts.Token);
                    var transforms = new List<Transform>();
                    foreach (var tile in selectedTiles)
                    {
                        if (tile.LoadedObject != null)
                        {
                            TransformEx.GetAllChildrenWithComponent<T>(tile.LoadedObject.transform).ForEach(t => transforms.Add(t));
                        }
                    }
                    return transforms;
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning("タイルの読み込みがキャンセルされました。");
                    return new List<Transform>();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"タイルの読み込み中にエラーが発生しました: {ex.Message}");
                    return new List<Transform>();
                }
            }
        }

        /// <summary>
        /// タイルリストからTransformリストを取得する
        /// 読み込まれていないタイルは無視する
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public static List<Transform> GetTransformsFromTiles(List<PLATEAUDynamicTile> tiles)
        {
            var transforms = new List<Transform>();
            foreach (var tile in tiles)
            {
                if (tile.LoadedObject != null)
                {
                    transforms.Add(tile.LoadedObject.transform);
                }
            }
            return transforms;
        }

        /// <summary>
        /// ObservableCollection<string> から同一AddressのPLATEAUDynamicTileをtileManagerから探して返す
        /// </summary>
        /// <returns></returns>
        public static List<PLATEAUDynamicTile> GetSelectedTiles(ObservableCollection<TileSelectionItem> observableSelectedTiles, PLATEAUTileManager tileManager)
        {
            return GetTilesFromAddr(observableSelectedTiles.ToList(), tileManager);
        }

        /// <summary>
        /// Addressリストから同一AddressのPLATEAUDynamicTileをtileManagerから探して返す
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public static List<PLATEAUDynamicTile> GetTilesFromAddr(List<TileSelectionItem> addresses, PLATEAUTileManager tileManager)
        {
            var tiles = new List<PLATEAUDynamicTile>();
            foreach (var tile in tileManager.DynamicTiles)
            {
                if (addresses.Any(x => x.TileAddress == tile.Address))
                    tiles.Add(tile);
            }
            return tiles;
        }

        /// <summary>
        /// Tileの編集可能Prefabルートをシーンに配置し、そのTransformを返す
        /// </summary>
        /// <returns></returns>
        public static async Task<Transform> GetEditableTransformParent(ObservableCollection<TileSelectionItem> observableSelectedTiles, PLATEAUTileManager tileManager, TileRebuilder rebuilder, CancellationToken token)
        {
            var selectedTiles = GetSelectedTiles(observableSelectedTiles, tileManager);

            await rebuilder.TilePrefabsToScene(tileManager, selectedTiles, token);
            token.ThrowIfCancellationRequested();
            var editingTile = tileManager.transform.Find(TileRebuilder.EditingTilesParentName);
            if (editingTile == null)
                throw new InvalidOperationException($"{TileRebuilder.EditingTilesParentName} が見つかりませんでした。");
            return editingTile;
        }


        /// <summary>
        /// Tileの編集可能Prefabルート内から選択された子のTransformリストを返す
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<Transform> GetEditableTransforms(ObservableCollection<TileSelectionItem> observableSelectedTiles, Transform parent, bool asTile = false)
        {
            //変換用のTransformリストを作成しconfにセットします。
            var tileTransforms = new List<Transform>();
            foreach (var addr in observableSelectedTiles)
            {
                var edittrans = parent.transform.Find(addr.TileAddress)?.transform;
                if (edittrans == null)
                {
                    Debug.LogWarning($"タイル {addr.TileAddress} が見つかりませんでした。");
                    continue;
                }

                if (addr.IsTile || asTile)
                {
                    // タイル直下の場合はそのまま
                    tileTransforms.Add(edittrans);

                }
                else
                {
                    // 子タイルの場合はパスを辿って同一のパスを探す
                    var children = edittrans.GetAllChildren();
                    foreach (var child in children)
                    {
                        var path = child.GetPathToParent(addr.TileAddress);
                        if (path == addr.TilePath)
                        {
                            tileTransforms.Add(child);
                            break;
                        }
                    }
                }
            }
            return tileTransforms;
        }

        /// <summary>
        /// Transformからタイルパスを取得する
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parentName"> PLATEAUTileManager.TileParentName / TileRebuilder.EditingTilesParentName </param>
        /// <returns></returns>
        public static string GetTilePath(Transform obj, string parentName)
        {
            var pathToRoot = obj.GetPathToParent(parentName);
            if (pathToRoot == null)
                return obj.name; // 親が見つからなかった場合はオブジェクト名のみを返す
            var prefix = parentName + "/";
            if (!pathToRoot.StartsWith(prefix))
                return pathToRoot;
            return pathToRoot.Substring(prefix.Length); // "DynamicTileRoot/" の分をスキップ
        }

        /// <summary>
        /// 各TransformのPrefab Assetを保存する
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="transforms">Prefabとして保存するTransform</param>
        /// <param name="rebuilder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SavePrefabAssets(PLATEAUTileManager manager, List<Transform> transforms, TileRebuilder rebuilder, CancellationToken token)
        {
            foreach (var trans in transforms)
            {
                if (trans == null) continue;
                await rebuilder.SavePrefabAsset(manager, trans.gameObject);
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
