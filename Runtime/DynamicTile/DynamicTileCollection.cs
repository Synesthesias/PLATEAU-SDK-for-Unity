using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PLATEAUDynamicTileのコレクションを管理するクラス。
    /// タイルの追加、削除、クリア、アンロードなどの操作を担当します。
    /// </summary>
    public class DynamicTileCollection : IEnumerable<PLATEAUDynamicTile>
    {
        /// <summary>
        /// タイルのリスト
        /// </summary>
        private readonly List<PLATEAUDynamicTile> tiles = new();

        /// <summary>
        /// タイルのアドレスとタイルのマッピング
        /// </summary>
        private readonly Dictionary<string, PLATEAUDynamicTile> tileAddressesDict = new();

        /// <summary>
        /// LODごとの親Transformを管理する辞書
        /// </summary>
        private readonly Dictionary<int, Transform> lodParentDict = new();

        /// <summary>
        /// 最大LODレベル
        /// </summary>
        private const int MaxLodLevel = 4;

        private readonly ConditionalLogger logger;

        public DynamicTileCollection(ConditionalLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// タイルをコレクションに追加します。
        /// </summary>
        /// <param name="tile">追加するタイル</param>
        /// <returns>追加に成功したかどうか</returns>
        public bool AddTile(PLATEAUDynamicTile tile)
        {
            if (tile == null)
            {
                logger.LogWarn("nullのタイルは追加できません");
                return false;
            }

            if (tileAddressesDict.ContainsKey(tile.Address))
            {
                logger.LogWarn("既に追加済みのタイルAddressです");
                return false;
            }
            
            if (tiles.Contains(tile))
            {
                logger.LogWarn("既に追加済みのタイルです");
                return false;
            }

            tiles.Add(tile);
            tileAddressesDict[tile.Address] = tile;
            return true;
        }

        /// <summary>
        /// アドレスを指定してタイルを取得します。
        /// </summary>
        /// <param name="address">タイルのアドレス</param>
        /// <returns>対応するタイル、見つからない場合はnull</returns>
        public PLATEAUDynamicTile GetTileByAddress(string address)
        {
            return tileAddressesDict.GetValueOrDefault(address);
        }
        
        /// <summary>
        /// タイルのメタ情報からタイルリストを構築します。すでに存在するタイルはリソース解放後クリアされます。
        /// </summary>
        public void RefreshByTileMetas(IEnumerable<PLATEAUDynamicTileMetaInfo> metas, PLATEAUInstancedCityModel cityModel)
        {

            var newTiles = new List<PLATEAUDynamicTile>();
            foreach (var meta in metas)
            {
                var tile = new PLATEAUDynamicTile(meta, logger);
                newTiles.Add(tile);
            }
            
            ClearTiles(cityModel); // 既存のタイルリストをクリア
            foreach (var tile in newTiles)
            {
                AddTile(tile);
            }
        }

        /// <summary>
        /// すべてのタイルをクリアします。
        /// </summary>
        public void ClearTiles(PLATEAUInstancedCityModel cityModel)
        {
            ClearTileAssets(cityModel);
            tiles.Clear();
            tileAddressesDict.Clear();
            lodParentDict.Clear();
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードします。
        /// </summary>
        public void ClearTileAssets(PLATEAUInstancedCityModel cityModel)
        {
            foreach (var tile in tiles)
            {
                try
                {
                    if (tile.LoadHandle.IsValid())
                    {
                        Addressables.Release(tile.LoadHandle);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarn($"タイルのアンロード中にエラーが発生しました: {tile.Address} {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    DeleteGameObjectInstance(tile.LoadedObject);
                    tile.Reset();
                }
            }

            ClearLodChildren(cityModel);
        }

        /// <summary>
        /// LOD直下の子オブジェクトをすべてアンロードします。
        /// </summary>
        private void ClearLodChildren(PLATEAUInstancedCityModel cityModel)
        {
            if (cityModel == null)
                return;

            for (int lod = 0; lod <= MaxLodLevel; lod++)
            {
                var lodName = $"LOD{lod}";
                var lodParent = cityModel.transform.Find(lodName)?.gameObject;
                if (lodParent != null)
                {
                    for (int i = lodParent.transform.childCount - 1; i >= 0; i--)
                    {
                        var child = lodParent.transform.GetChild(i);
                        if (child != null)
                        {
                            DeleteGameObjectInstance(child.gameObject);
                        }
                    }
                    DeleteGameObjectInstance(lodParent);
                }
            }
        }

        /// <summary>
        /// 指定されたLODから親Transformを取得または作成します。
        /// </summary>
        /// <param name="lod">LODレベル</param>
        /// <param name="parentTransform">親Transform</param>
        /// <returns>LODの親Transform</returns>
        public Transform FindParent(int lod, Transform parentTransform)
        {
            if (parentTransform == null)
            {
                logger.LogWarn("親Transformがnullです。LODの親Transformを取得できません。");
                return null;
            }
            
            if (lodParentDict.TryGetValue(lod, out var existingParent))
            {
                if (existingParent != null)
                    return existingParent;
                else
                    lodParentDict.Remove(lod); // nullエントリをキャッシュから削除
            }

            var lodName = $"LOD{lod}";
            GameObject lodObject = null;

            if (parentTransform != null)
            {
                lodObject = parentTransform.Find(lodName)?.gameObject;
                if (lodObject == null)
                {
                    lodObject = new GameObject(lodName);
                    lodObject.transform.SetParent(parentTransform, false);
                }
            }

            if (lodObject != null)
            {
                lodParentDict[lod] = lodObject.transform;
                return lodObject.transform;
            }

            logger.LogWarn("LOD親オブジェクトの作成に失敗しました。");
            return null;
        }

        /// <summary>
        /// GameObjectインスタンスを削除します。
        /// </summary>
        /// <param name="obj">削除するGameObject</param>
        public static void DeleteGameObjectInstance(GameObject obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// コレクション内のタイル数を取得します。
        /// </summary>
        /// <returns>タイル数</returns>
        public int Count => tiles.Count;

        /// <summary>
        /// コレクションが空かどうかを確認します。
        /// </summary>
        /// <returns>空の場合true</returns>
        public bool IsEmpty => tiles.Count == 0;

        public PLATEAUDynamicTile At(int index)
        {
            return tiles[index];
        }

        public List<PLATEAUDynamicTile> ToList()
        {
            return new List<PLATEAUDynamicTile>(tiles);
        }
        

        public void ComposeTileBounds(NativeArray<TileBounds> outBounds)
        {
            if (outBounds.Length != tiles.Count)
            {
                throw new ArgumentException($"配列サイズが一致しません。必要: {tiles.Count}, 実際: {outBounds.Length}");
            }
            
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                outBounds[i] = tile.GetTileBoundsStruct();
            }
        }
        
        public IEnumerator<PLATEAUDynamicTile> GetEnumerator()
        {
            return tiles.GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
} 