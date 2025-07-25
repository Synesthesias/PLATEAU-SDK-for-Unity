using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        private List<PLATEAUDynamicTile> Tiles { get; } = new();

        /// <summary>
        /// タイルのアドレスとタイルのマッピング
        /// </summary>
        private Dictionary<string, PLATEAUDynamicTile> tileAddressesDict = new();

        /// <summary>
        /// LODごとの親Transformを管理する辞書
        /// </summary>
        private Dictionary<int, Transform> lodParentDict = new();

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

            if (Tiles.Contains(tile))
            {
                logger.LogWarn("既に追加済みのタイルです");
                return false;
            }

            if (tileAddressesDict.ContainsKey(tile.Address))
            {
                logger.LogWarn("既に追加済みのタイルAddressです");
                return false;
            }

            Tiles.Add(tile);
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
        /// タイルのメタ情報からタイルリストを構築します。すでに存在するタイルはクリアされます。
        /// </summary>
        public void RefreshByTileMetas(IEnumerable<PLATEAUDynamicTileMetaInfo> metas)
        {
            ClearTiles(); // 既存のタイルリストをクリア
            foreach (var meta in metas)
            {
                var tile = new PLATEAUDynamicTile(meta, logger);
                AddTile(tile);
            }
        }

        /// <summary>
        /// すべてのタイルをクリアします。
        /// </summary>
        public void ClearTiles()
        {
            ClearTileAssets();
            Tiles.Clear();
            tileAddressesDict.Clear();
            lodParentDict.Clear();
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードします。
        /// </summary>
        public void ClearTileAssets()
        {
            foreach (var tile in Tiles)
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
                    logger.LogWarn($"タイルのアンロード中にエラーが発生しました: {tile.Address} {ex.Message}");
                }
                finally
                {
                    DeleteGameObjectInstance(tile.LoadedObject);
                    tile.Reset();
                }
            }

            ClearLodChildren();
        }

        /// <summary>
        /// LOD直下の子オブジェクトをすべてアンロードします。
        /// </summary>
        private void ClearLodChildren()
        {
            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>()?.gameObject;
            if (instance == null)
                return;

            for (int lod = 0; lod <= MaxLodLevel; lod++)
            {
                var lodName = $"LOD{lod}";
                var lodParent = instance.transform.Find(lodName)?.gameObject;
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
            if (lodParentDict.TryGetValue(lod, out var existingParent))
            {
                if (existingParent != null)
                    return existingParent;
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
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// コレクション内のタイル数を取得します。
        /// </summary>
        /// <returns>タイル数</returns>
        public int Count => Tiles.Count;

        /// <summary>
        /// コレクションが空かどうかを確認します。
        /// </summary>
        /// <returns>空の場合true</returns>
        public bool IsEmpty => Tiles.Count == 0;

        public PLATEAUDynamicTile At(int index)
        {
            return Tiles[index];
        }

        public List<PLATEAUDynamicTile> ToList()
        {
            return new List<PLATEAUDynamicTile>(Tiles);
        }
        

        public void ComposeTileBounds(NativeArray<TileBounds> outBounds)
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                var tile = Tiles[i];
                outBounds[i] = tile.GetTileBoundsStruct();
            }
        }
        
        public IEnumerator<PLATEAUDynamicTile> GetEnumerator()
        {
            return Tiles.GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
} 