using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {

        [SerializeField]
        public float loadDistance = 1500f; // ロードを行うカメラからの距離

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

        // 全タイルのロードタスク実行時のCancellationTokenSource
        public CancellationTokenSource LoadTaskCancellationTokenSource { get; set; } = new();

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
        /// カタログに変わるScriptableObjectを使用して、タイルの初期化を行います。
        /// </summary>
        /// <returns></returns>
        public async Task InitializeTiles()
        {
            if (State == ManagerState.Initializing)
                return;

            State = ManagerState.Initializing;

            // PLATEAUDynamicTileMetaStoreをAddressablesからロード
            // TODO: ↓で取得できるように変更
            //var metaStore = await addressableLoader.Initialize(catalogPath);

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
            ClearTiles(); // 既存のタイルリストをクリア
            foreach (var tileMeta in metaStore.TileMetaInfos)
            {
                var tile = new PLATEAUDynamicTile(tileMeta);
                AddTile(tile);
            }

            Addressables.Release(handle); // ハンドルを解放

            State = ManagerState.Operating;
            InitializeCameraPosition();
        }

        /// <summary>
        /// 初期化時にタイルのロード状態をカメラの位置に基づいて更新します。
        /// </summary>
        private void InitializeCameraPosition()
        {
            Camera currentCamera = null;
#if UNITY_EDITOR
            currentCamera = EditorApplication.isPlaying ? Camera.main : SceneView.currentDrawingSceneView?.camera ?? SceneView.lastActiveSceneView?.camera;
#else
            currentCamera = Camera.main;
#endif
            if (currentCamera != null) 
                UpdateAssetsByCameraPosition(currentCamera.transform.position);
        }

        // TODO:　この処理は削除予定
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
            if (tile.LoadHandle.IsValid() || tile.LoadedObject != null)
            {
                Debug.Log($"Already loaded: {address}");
                return await Task.FromResult<bool>(true);
            }

            try
            {
                // Addressablesでは、Cancel処理がサポートされていないため、CancellationTokenSourceを使用してキャンセル可能なロードを実装
                tile.LoadHandleCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(LoadTaskCancellationTokenSource.Token);

                var handle = Addressables.InstantiateAsync(address, FindParent(tile.Lod));
                tile.LoadHandle = handle;
                await handle.Task;

                //Cancel処理
                tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    //Debug.Log($"アセットのロードに成功しました: {address}");
                    var instance = handle.Result;
                    if (instance != null)
                    {
                        instance.name = address;
                        instance.hideFlags = HideFlags.DontSave; // シーン保存時にオブジェクトを保存しない
                        return true;
                    }
                }
                else
                {
                    // ロードに失敗した場合は、1回リトライ
                    tile.LoadHandle = default;
                    tile.LoadHandleCancellationTokenSource?.Dispose();
                    bool success = await Load(tile);
                    if (!success)
                        Debug.LogWarning($"アセットのロードに失敗しました: {address}");
                    return success;
                }
            }
            catch (OperationCanceledException)
            {
                //Debug.LogWarning($"アセットのロードがキャンセルされました: {address}");
                if (tile.LoadHandle.IsValid())
                    Addressables.ReleaseInstance(tile.LoadHandle);
                tile.LoadHandle = default; // ハンドルをリセット
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"アセットのロード中にエラーが発生しました: {address} {ex.Message}");
            }
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
            try
            {
                if (tile.LoadHandle.IsValid())
                {
                    if (!tile.LoadHandle.IsDone)
                    {
                        // ロードが完了していない場合はキャンセル
                        tile.LoadHandleCancellationTokenSource?.Cancel();
                        return false;
                    }

                    if (!Addressables.ReleaseInstance(tile.LoadHandle))
                    {
                        throw new Exception($"AddressablesのReleaseInstanceに失敗しました: {address}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"アセットのRelease中にエラーが発生しました: {address} {ex.Message}");
                if(tile.LoadedObject != null)
                {
                    // AddressablesのReleaseInstanceが失敗した場合、オブジェクトを破棄
                    DestroyImmediate(tile.LoadedObject);
                }
                tile.LoadHandle = default; // ハンドルをリセット
                Debug.Log($"Addressablesのハンドルをリセットしました: {address}");
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
            ClearTiles();
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
            ClearTileAssets();
            DynamicTiles.Clear();
            tileAddressesDict.Clear();

            // LODの親Transformもクリア
            lodParentDict.Clear();

            State = ManagerState.None; // マネージャーの状態をリセット
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードする
        /// </summary>
        public void ClearTileAssets()
        {
            var originalState = State == ManagerState.CleaningUp ? ManagerState.Operating : State;
            State = ManagerState.CleaningUp;

            // すべてのロード済みオブジェクトをアンロード
            foreach (var tile in DynamicTiles)
            {
                try
                {
                    if (tile.LoadHandle.IsValid())
                    {
                        if (!Addressables.ReleaseInstance(tile.LoadHandle))
                        {
                            throw new Exception($"AddressablesのReleaseInstanceに失敗しました: {tile.Address}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"タイルのアンロード中にエラーが発生しました: {tile.Address} {ex.Message}");
                    if (tile.LoadedObject != null)
                    {
                        // AddressablesのReleaseInstanceが失敗した場合、オブジェクトを破棄
                        DestroyImmediate(tile.LoadedObject);
                    }
                }
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

            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>()?.gameObject;
            if (instance == null)
                instance = this.gameObject;

            // Lod直下の子オブジェクトをすべてアンロード
            for (int lod = 0; lod <= maxLod; lod++)
            {
                var lodName = $"LOD{lod}";
                var lodParent = instance.transform.Find(lodName)?.gameObject;
                if (lodParent != null)
                {
                    for (int i = 0; i < lodParent.transform.childCount; i++)
                    {
                        var child = lodParent.transform.GetChild(i);
                        if (child != null)
                        {
                            try
                            {
                                if (!Addressables.ReleaseInstance(child.gameObject))
                                {
                                    throw new Exception($"AddressablesのReleaseInstanceに失敗しました: {child.gameObject.name}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"GameObjectのアンロードでエラーが発生しました。{child.gameObject.name} {ex.Message}");
                                // アドレスのリリースに失敗した場合、直接破棄
                                DestroyImmediate(child.gameObject);
                            }
                        }
                    }
                    // LODの親Transformも削除
                    DestroyImmediate(lodParent);
                }
            }
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateAssetsByCameraPosition(Vector3 position)
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

                jobSystem.UpdateAssetsByCameraPosition(position);
            }
            else
            {
                // Job Systemを使用しない場合
                UpdateAssetsByCameraPositionInternal(position);
            }

            LastCameraPosition = position; // 最後のカメラ位置を更新
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateAssetsByCameraPositionInternal(Vector3 position)
        {
            foreach (var tile in DynamicTiles)
            {
                var distance = tile.GetDistance(position, true);
                if (distance < loadDistance)
                {
                    if (tile.LoadHandle.IsValid())
                        tile.NextLoadState = LoadState.None;
                    else
                        tile.NextLoadState = LoadState.Load;
                }
                else
                {
                    if (tile.LoadHandle.IsValid())
                        tile.NextLoadState = LoadState.Unload;
                    else
                        tile.NextLoadState = LoadState.None;
                }
            }

            try
            {
                var task = ExecuteLoadTask(LoadTaskCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("タイルのロードTaskがキャンセルされました。");
            }
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。
        /// </summary>
        private async Task ExecuteLoadTask(CancellationToken token)
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
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// 実行中のロードタスクをキャンセルし、CancellationTokenSourceをリセットします。
        /// </summary>
        public void CancelLoadTask()
        {
            LoadTaskCancellationTokenSource?.Cancel();
            LoadTaskCancellationTokenSource?.Dispose();
            LoadTaskCancellationTokenSource = new();
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
                if (parentTransform != null)
                    return parentTransform;
            }

            var lodName = $"LOD{lod}";
            GameObject lodObject = null;
            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>()?.gameObject;
            if (instance == null)
                instance = this.gameObject;

            lodObject = instance.transform.Find(lodName)?.gameObject;
            if (lodObject == null)
            {
                lodObject = new GameObject(lodName);
                lodObject.AddComponent<MeshRenderer>();
                lodObject.AddComponent<BoxCollider>();
                lodObject.transform.SetParent(instance?.transform, false);
            }

            lodParentDict[lod] = lodObject.transform;
            return lodObject.transform;
        }

        // Debug用Bounds表示
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