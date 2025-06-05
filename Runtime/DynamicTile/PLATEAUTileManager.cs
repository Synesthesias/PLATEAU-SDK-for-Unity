using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        public static readonly float DefaultLoadDistance = 1500f; // デフォルトのロード距離

        [SerializeField]
        private string catalogPath;
        public string CatalogPath => catalogPath;

        [SerializeField]
        private bool showDebugTileInfo = true; // Debug情報を表示するかどうか

        [ConditionalShow("showDebugTileInfo")]
        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか

        // 使用中のタイルリスト
        public List<PLATEAUDynamicTile> DynamicTiles { get; private set; } = new();

        // TileとAddressのマッピング
        private Dictionary<string, PLATEAUDynamicTile> tileAddressesDict = new();

        // Parent TransformをLODごとに管理する辞書
        private Dictionary<int, Transform> lodParentDict = new();

        public enum ManagerState
        {
            None,
            Initializing,
            Operating,
            CleaningUp
        }

        // マネージャーの状態
        public ManagerState State { get; private set; } = ManagerState.None;

        // 最後にカメラが更新された位置
        public Vector3 LastCameraPosition { get; private set; } = Vector3.zero; 

        private AddressableLoader addressableLoader = new ();
        private PLATEAUDynamicTileJobSystem jobSystem;

        /// <summary>
        /// カタログからアドレスを初期化し、各タイルのアドレスを更新します。(暫定）
        /// 取得AddressとTileの順番が同じと仮定しています。
        /// 旧Addressと新Addressのマッピングが必要
        /// </summary>
        /// <returns></returns>
        public async Task InitializeTiles()
        {
            if (State == ManagerState.Initializing)
                return;

            State = ManagerState.Initializing;

            var handle = Addressables.LoadAssetAsync<PLATEAUDynamicTileMetaStore>("PLATEAUDynamicTileMetaStore");
            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("PLATEAUDynamicTileMetaStoreのロードに失敗しました");
                State = ManagerState.None;
                return;
            }

            var metaStore = handle.Result;

            Debug.Log($"InitializeTiles: {metaStore.TileMetaInfos.Count} tiles found in meta store.");
            ClearTileAssets(); // タイルAssetをアンロード
            ClearTiles(); // 既存のタイルリストをクリア
            foreach (var tileMeta in metaStore.TileMetaInfos)
            {
                var tile = new PLATEAUDynamicTile(tileMeta);
                AddTile(tile);
            }

            InitializeCameraPosition();

            State = ManagerState.Operating;
        }

        private void InitializeCameraPosition()
        {
            Camera currentCamera = null;
#if UNITY_EDITOR
            currentCamera = EditorApplication.isPlaying ? Camera.main : SceneView.currentDrawingSceneView?.camera;
#else
            currentCamera = Camera.main;
#endif
            if (currentCamera != null) 
                UpdateAssetByCameraPosition(currentCamera.transform.position);
        }

        public async Task LoadFromCatalog()
        {
            var metaStore = await addressableLoader.Initialize(catalogPath);
            foreach (var metaInfo in metaStore.TileMetaInfos)
            {
                await Load(metaInfo.AddressName);
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
                Debug.Log($"Already loaded: {address}");
                return await Task.FromResult<bool>(true);
            }

            tile.LoadStart();
            try
            {
                var instance = await addressableLoader.InstantiateAssetAsync(tile.Address, FindParent(tile.Lod));
                if (instance != null)
                {
                    instance.hideFlags = HideFlags.DontSave; // シーン保存時にオブジェクトを保存しない
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
            ClearTileAssets();
            Debug.Log("全てのアセットをアンロードしました");
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
            
            if (DynamicTiles.Contains(tile))
            {
                Debug.LogWarning("既に追加済みのタイルです");
                return;
            }

            DynamicTiles.Add(tile);

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
            DynamicTiles.Clear();
            tileAddressesDict.Clear();
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードする
        /// </summary>
        public void ClearTileAssets()
        {
            var originalState = State;
            State = ManagerState.CleaningUp;

            // すべてのロード済みオブジェクトをアンロード
            foreach (var tile in DynamicTiles)
            {
                if (tile.LoadedObject != null)
                {
                    if (!Addressables.ReleaseInstance(tile.LoadedObject))
                    {
                        DestroyImmediate(tile.LoadedObject);
                    }
                }
                tile.Reset();
            }

            ClearLodChildren();
            State = originalState;
        }

        /// <summary>
        /// LOD直下の子オブジェクトをすべてアンロードします。
        /// (TileからのUnloadでは消去されない場合があるため)
        /// </summary>
        private void ClearLodChildren()
        { 
            const int maxLod = 4; // 最大LOD数を定義

            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>();
            if (instance == null)
                return;

            // Lod直下の子オブジェクトをすべてアンロード
            for (int lod = 0; lod <= maxLod; lod++)
            {
                var lodName = $"LOD{lod}";
                var lodParent = instance.transform.Find(lodName)?.gameObject?.transform;
                if (lodParent != null)
                {
                    for (int i = 0; i < lodParent.childCount; i++)
                    {
                        var child = lodParent.GetChild(i);
                        if (child != null)
                        {
                            if (!Addressables.ReleaseInstance(child.gameObject))
                            {
                                DestroyImmediate(child.gameObject);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateAssetByCameraPosition(Vector3 position)
        {
            if ( State != ManagerState.Operating)
                return;

            if (DynamicTiles.Count <= 0)
                return;

            if (useJobSystem)
            {
                // Job Systemを使用する場合
                if (jobSystem == null)
                {
                    jobSystem = new PLATEAUDynamicTileJobSystem();
                    jobSystem.Initialize(this, DynamicTiles);
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
            foreach (var tile in DynamicTiles)
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
            foreach (var tile in DynamicTiles)
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

        /// <summary>
        /// 指定されたLODから親Transformを取得または作成します。
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        private Transform FindParent(int lod)
        {
            if(lodParentDict.TryGetValue(lod, out var parentTransform))
            {
                return parentTransform;
            }

            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>();
            if (instance == null)
            {
                Debug.LogError("PLATEAUInstancedCityModelが見つかりません。LODの作成に失敗しました。");
                return null;
            }

            var lodName = $"LOD{lod}";
            GameObject lodObject = instance.transform.Find(lodName)?.gameObject;
            if (lodObject == null)
            {
                lodObject = new GameObject(lodName);
                lodObject.AddComponent<MeshRenderer>();
                lodObject.AddComponent<BoxCollider>();
                lodObject.transform.SetParent(instance.transform, false);
            }

            lodParentDict[lod] = lodObject.transform;
            return lodObject.transform;
        }

        // Debug用
        public void ShowBounds()
        {
            foreach (var tile in DynamicTiles)
            {
                DebugEx.DrawBounds(tile.Extent, Color.red, 30f);
                DebugEx.DrawBounds(tile.Extent, Color.red, 30f);
            }
        }
    }
}