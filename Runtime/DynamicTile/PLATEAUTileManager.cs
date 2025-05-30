using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        public static readonly float DefaultLoadDistance = 1500f; // デフォルトのロード距離

        public const bool showDebugTileInfo = true; // Debug情報を表示するかどうか

        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか

        public Vector3 LastCameraPosition { get; private set; } = Vector3.zero; // 最後にカメラが更新された位置

        [SerializeField]
        private List<PLATEAUDynamicTile> dynamicTiles = new();

        // TileとAddressのマッピング
        private Dictionary<string, PLATEAUDynamicTile> tileAddressesDict = new();

        [SerializeField]
        private string catalogPath;
        public string CatalogPath => catalogPath;

        private const string DynamicTileLabelName = "DynamicTile";

        private AddressableLoader addressableLoader = new ();

        private PLATEAUDynamicTileJobSystem jobSystem;

        /// <summary>
        /// カタログからアドレスを初期化し、各タイルのアドレスを更新します。(暫定）
        /// 取得AddressとTileの順番が同じと仮定しています。
        /// </summary>
        /// <returns></returns>
        public async Task ReinitializeFromCatalog()
        {
            var addresses = await addressableLoader.Initialize(catalogPath);
            if( addresses.Count != dynamicTiles.Count)
                Debug.LogWarning($"アドレスの数とタイルの数が一致しません: アドレス数={addresses.Count}, タイル数={dynamicTiles.Count}");

            for (var i = 0; i < addresses.Count; i++)
            {
                var address = addresses[i];
                var tile = dynamicTiles[i];
                tile.ReplaceAddress(address);
            }
        }

        public async Task LoadFromCatalog()
        {
            var addresses = await addressableLoader.Initialize(catalogPath);
            foreach (var address in addresses)
            {
                await Load(address);
            }
        }

        /// <summary>
        /// カタログパスを保存します。
        /// </summary>
        /// <param name="path"></param>
        public void SaveCatalogPath(string path)
        {
            // パスを正規化（バックスラッシュをスラッシュに変換）
            catalogPath = path.Replace('\\', '/');
        }

        /// <summary>
        /// Tileを指定してAddressablesからロードする
        /// </summary>
        public async Task<bool> Load(PLATEAUDynamicTile tile)
        {
            string address = tile.Address;
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {address}");
                return await Task.FromResult<bool>(false);
            }
            // 既にロードされている場合はスキップ
            if (tile.IsLoadedOrLoading)
            {
                //Debug.Log($"Already loaded: {address}");
                return await Task.FromResult<bool>(true);
            }

            tile.LoadStart();
            try
            {
                var instance = await addressableLoader.InstantiateAssetAsync(tile.Address, tile.Parent);
                if (instance != null)
                {
                    tile.LoadedObject = instance;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"アセットのロード中にエラーが発生しました: {ex.Message}");
            }
            tile.LoadEnd();
            return false;
        }

        /// <summary>
        /// Addressを指定してAddressablesからロードする
        /// </summary>
        public async Task<bool> Load(string address)
        {
            var tile = tileAddressesDict.GetValueOrDefault(address);
            if (tile == null)
            {
                Debug.LogWarning($"指定したアドレスに対応するタイルが見つかりません: {address}");
                return await Task.FromResult<bool>(false);
            }
            return await Load(tile);
        }

        /// <summary>
        /// Tileを指定してAddressablesからアンロードする
        /// </summary>
        /// <param name="address"></param>
        public bool Unload(PLATEAUDynamicTile tile)
        {
            string address = tile.Address;
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {address}");
                return false;
            }

            if (tile.LoadedObject != null)
            {
                if (Addressables.ReleaseInstance(tile.LoadedObject))
                {
                    tile.LoadedObject = null;
                    tile.LoadEnd();
                }
                else
                {
                    // AddressablesのReleaseInstanceが失敗した場合、オブジェクトを破棄
                    DestroyImmediate(tile.LoadedObject);
                    tile.LoadedObject = null;
                    tile.LoadEnd();
                    Debug.LogWarning($"Failed to ReleaseInstance : {address}");
                }
            }
            return true;
        }

        /// <summary>
        /// Addressを指定してAddressablesからアンロードする
        /// </summary>
        public bool Unload(string address)
        {
            var tile = tileAddressesDict.GetValueOrDefault(address);
            if (tile == null)
            {
                Debug.LogWarning($"指定したアドレスに対応するタイルが見つかりません: {address}");
                return false;
            }
            return Unload(tile);
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        private void OnDestroy()
        {
            ClearAll();
            Debug.Log("全てのアセットをアンロードしました");
        }

        /// <summary>
        /// Tileデータを追加
        /// </summary>
        /// <param name="tile"></param>
        public void AddTile(PLATEAUDynamicTile tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("nullのタイルは追加できません");
                return;
            }
            
            if (dynamicTiles.Contains(tile))
            {
                Debug.LogWarning("既に追加済みのタイルです");
                return;
            }

            dynamicTiles.Add(tile);

            if(tileAddressesDict.ContainsKey(tile.Address))
                Debug.LogWarning("既に追加済みのタイルAddressです");
            else
                tileAddressesDict[tile.Address] = tile;
        }
        
        /// <summary>
        /// 登録したタイルをクリア
        /// </summary>
        public void ClearTiles()
        {
            dynamicTiles.Clear();
        }

        /// <summary>
        /// Job Systemを使用している場合、OnDisableでDisposeする
        /// </summary>
        private void OnDisable()
        {
            jobSystem?.Dispose();
            jobSystem = null;
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードする
        /// </summary>
        public void ClearAll()
        {
            // すべてのロード済みオブジェクトをアンロード
            foreach (var tile in dynamicTiles)
            {
                if (tile == null) continue; // タイルがnullの場合はスキップ
                if (tile.LoadedObject != null)
                {
                    if (!Addressables.ReleaseInstance(tile.LoadedObject))
                    {
                        DestroyImmediate(tile.LoadedObject);
                    }
                }
                tile.Reset();
            }
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateAssetByCameraPosition(Vector3 position)
        {
            if (useJobSystem)
            {
                // Job Systemを使用する場合
                if (jobSystem == null)
                {
                    jobSystem = new PLATEAUDynamicTileJobSystem();
                    jobSystem.Initialize(this, dynamicTiles);
                }
                jobSystem.UpdateAssetByCameraPosition(position);
            }
            else
            {
                // Job Systemを使用しない場合
                UpdateAssetByCameraPositionInternal(position);
            }

            LastCameraPosition = position; // 最後のカメラ位置を更新
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateAssetByCameraPositionInternal(Vector3 position)
        {
            foreach (var tile in dynamicTiles)
            {
                var distance = tile.GetDistance(position, true);
                if (distance < DefaultLoadDistance)
                {
                    if (tile.IsLoadedOrLoading)
                        tile.NextLoadState = LoadState.None;
                    else
                        tile.NextLoadState = LoadState.Load;
                }
                else
                {
                    if (tile.IsLoadedOrLoading)
                        tile.NextLoadState = LoadState.Unload;
                    else
                        tile.NextLoadState = LoadState.None;
                }
            }
            ExecuteLoadTask();
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。
        /// </summary>
        private async void ExecuteLoadTask()
        {
            foreach (var tile in dynamicTiles)
            {
                if (tile.NextLoadState == LoadState.None)
                {
                    // 何もしない
                    continue;
                }
                else if (tile.NextLoadState == LoadState.Load)
                {
                    await Load(tile);
                }
                else if (tile.NextLoadState == LoadState.Unload)
                {
                    Unload(tile);
                }
            }
        }

        // Debug用
        public void ShowBounds()
        {
            foreach (var tile in dynamicTiles)
            {
                DebugEx.DrawBounds(tile.Extent, Color.red, 30f);
                DebugEx.DrawBounds(tile.Extent, Color.red, 30f);
            }
        }
    }
}