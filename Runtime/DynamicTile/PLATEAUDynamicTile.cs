using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Util;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileオブジェクトに付与し、ダウンサンプルレベルごとのAddress情報を保持するコンポーネント
    /// </summary>
    //[Serializable]
    public class PLATEAUDynamicTile // : UnityEngine.Object
    {
        [SerializeField]
        private string address;
        public string Address => address;


        [SerializeField]
        private bool isExcludeTile;
        public bool IsExcludeTile
        {
            get => isExcludeTile;
            set => isExcludeTile = value;
        }

        /// <summary>
        /// AddressablesでロードされたGameObjectを保持する。
        /// </summary>
        [SerializeField]
        private GameObject loadedObject;
        public GameObject LoadedObject
        {
            get => loadedObject;
            set 
            { 
                loadedObject = value;
                IsLoading = false;
            }
        }

        [SerializeField]
        private int lod;
        public int Lod
        {
            get => lod;
        }

        /// <summary>
        /// Addressablesをロードする際の親Transformを保持する。
        /// </summary>
        [SerializeField]
        private Transform parent;
        public Transform Parent
        {
            get => parent;
        }

        /// <summary>
        /// タイルの範囲を示すBoundsを保持する。
        /// </summary>
        [SerializeField]
        private Bounds extent;
        public Bounds Extent
        {
            get => extent;
        }

        /// <summary>
        /// カメラからの距離を保持する。デバッグ用。
        /// </summary>
        [ConditionalShowBool(PLATEAUTileManager.showDebugTileInfo)]
        [ReadOnly]
        [SerializeField]
        private float distanceFromCamera = 0f;
        public float DistanceFromCamera
        {
            get => distanceFromCamera;
            set => distanceFromCamera = value;
        }

        /// <summary>
        /// 次にAddressablesをロードする状態を示す。
        /// </summary>
        [ConditionalShowBool(PLATEAUTileManager.showDebugTileInfo)]
        [ReadOnly]
        [SerializeField]
        private LoadState nextLoadState = LoadState.None;
        public LoadState NextLoadState
        {
            get => nextLoadState;
            set => nextLoadState = value;
        }

        // ロード中かどうかを示すフラグ
        public bool IsLoading { get; private set; }

        public bool IsLoadedOrLoading
        {
            get => LoadedObject != null || IsLoading;
        }

        // Job Systemで使用するための構造体を返す
        public TileBounds GetTileBoundsStruct()
        {
            return new TileBounds(Extent.min, Extent.max);
        }

        /// <summary>
        /// PLATEAUDynamicTileのコンストラクタ。
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <param name="original"></param>
        public PLATEAUDynamicTile(string address, Transform parent, GameObject original = null)
        {
            this.address = address;
            this.parent = parent;

            if (original != null)
            {
                InitializeExtentFromGameObject(original);
                LoadedObject = original;
            }
            else
            {
                var meshcode = GetMeshCode();
                InitializeExtentFromMeshCode(meshcode);
            }
        }

        /// <summary>
        /// PLATEAUDynamicTileのコンストラクタ。
        /// </summary>
        /// <param name="info">Scriptable Object データ</param>
        public PLATEAUDynamicTile(PLATEAUDynamicTileMetaInfo info)
        {
            this.address = info.AddressName;
            this.lod = info.LOD;
            extent = info.Extent;
        }
        
        public void LoadStart()
        {
            IsLoading = true;
        }

        public void LoadEnd()
        {
            IsLoading = false;
        }

        public void Reset()
        {
            IsLoading = false;
            nextLoadState = LoadState.None;
        }

        public void ReplaceAddress(string addr)
        {
            address = addr;
        }

        /// <summary>
        /// カメラからの距離を計算する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="ignoreY"></param>
        /// <returns></returns>
        public float GetDistance(Vector3 position, bool ignoreY)
        {
            if (extent == null)
            {
                return 0f;
            }

            if (ignoreY)
            {
                var extent2d = new Bounds(new Vector3(Extent.center.x, 0, Extent.center.z), new Vector3(Extent.size.x, 0, Extent.size.z));
                var position2d = new Vector3(position.x, 0, position.z);
                Vector3 closestPoint2d = extent2d.ClosestPoint(position2d);
                return Vector3.Distance(position2d, closestPoint2d);
            }

            Vector3 closestPoint = Extent.ClosestPoint(position);
            var distance = Vector3.Distance(position, closestPoint);

            //debug用
            DistanceFromCamera = distance;

            return distance;
        }

        /// <summary>
        /// メッシュコードからタイルの範囲を初期化する。
        /// </summary>
        /// <param name="meshcode"></param>
        /// <returns></returns>
        private Bounds InitializeExtentFromMeshCode(string meshcode)
        {
            GridCode gridCode = GridCode.Create(meshcode);

            var geo = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>().GeoReference;

            var min = geo.Project(gridCode.Extent.Min);
            var max = geo.Project(gridCode.Extent.Max);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3((float)min.X, (float)min.Y, (float)min.Z), new Vector3((float)max.X, (float)max.Y, (float)max.Z));

            extent = bounds;

            // Debug Draw Rect
            if (PLATEAUTileManager.showDebugTileInfo)
                DebugEx.DrawBounds(bounds, Color.red, 30f);

            return bounds;
        }

        /// <summary>
        /// GameObjectからタイルの範囲を初期化する。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Bounds InitializeExtentFromGameObject(GameObject obj)
        {
            var bounds = obj.GetComponent<Renderer>().bounds;
            extent = bounds;

            // Debug Draw Rect
            if (PLATEAUTileManager.showDebugTileInfo)
                DebugEx.DrawBounds(bounds, Color.red, 30f);

            return bounds;
        }

        /// <summary>
        /// メッシュコードを取得する。
        /// GameObject名：tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
        /// 例:tile_zoom_0_grid_meshcode_gameobjectname_0
        /// </summary>
        /// <returns></returns>
        private string GetMeshCode()
        {
            Match match = Regex.Match(address, @"_grid_([^_]+)_");
            if (match.Success)
            {
                string meshcode = match.Groups[1].Value;

                //Debug.Log($"メッシュコード: {meshcode}");
                return meshcode;
            }
            Debug.LogError("メッシュコードが見つかりません");
            return null;
        }
    }
} 