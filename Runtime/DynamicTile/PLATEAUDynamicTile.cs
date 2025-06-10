using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileの情報を保持するクラス。
    /// </summary>
    public class PLATEAUDynamicTile // : UnityEngine.Object
    {
        /// <summary>
        /// Addressablesのアドレスを保持する。
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// タイルが除外されているかどうかを示すフラグ。
        /// </summary>
        public bool IsExcludeTile { get; set; }

        /// <summary>
        /// AddressablesでロードされたGameObjectを保持する。
        /// </summary>
        public GameObject LoadedObject{
            get {
                if (LoadHandle.IsValid() && LoadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return LoadHandle.Result as GameObject;
                }
                return null;
            }
        }

        /// <summary>
        /// Addressablesのロードハンドルを保持する。
        /// </summary>
        public AsyncOperationHandle<GameObject> LoadHandle { get; set; } = default;

        /// <summary>
        /// Addressablesのロードハンドルのキャンセルトークンを保持する。
        /// Tileごとのキャンセルをサポートするためのもの。
        /// 現状、AddressablesはCancelをサポートしていないので、普通のフラグでも良いが、将来のために保持しておく。
        /// </summary>
        public CancellationTokenSource LoadHandleCancellationTokenSource { get; set; }

        /// <summary>
        /// LOD（Level of Detail）を保持する。
        /// Addressablesをロードする際の親Transform取得用。
        /// </summary>
        public int Lod { get; private set; } = 0;

        /// <summary>
        /// タイルの範囲を示すBoundsを保持する。
        /// </summary>
        public Bounds Extent { get; private set; }

        /// <summary>
        /// カメラからの距離を保持する。デバッグ用。
        /// </summary>
        public float DistanceFromCamera { get; set; } = 0f;

        /// <summary>
        /// 次にAddressablesをロードする状態を示す。
        /// </summary>
        public LoadState NextLoadState { get; set; } = LoadState.None;

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
        public PLATEAUDynamicTile(string address, int lod, Bounds bounds)
        {
            Address = address;
            Lod = lod;

            if (bounds.size != Vector3.zero)
            {
                Extent = bounds;
            }
            else
            {
                // 現状この処理は未使用なのでここで行っているが、もし使用する場合GeoReferenceの取得は各Tileの生成前に一度だけ行うのが望ましい。
                var geo = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>().GeoReference;
                if (geo != null)
                {
                    var meshcode = GetMeshCode();
                    InitializeExtentFromMeshCode(meshcode, geo);
                }
            }
        }

        /// <summary>
        /// PLATEAUDynamicTileのコンストラクタ。
        /// </summary>
        /// <param name="info">Scriptable Object データ</param>
        public PLATEAUDynamicTile(PLATEAUDynamicTileMetaInfo info)
        {
            Address = info.AddressName;
            Lod = info.LOD;
            Extent = info.Extent;
        }

        /// <summary>
        /// カメラからの距離を計算する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="ignoreY"></param>
        /// <returns></returns>
        public float GetDistance(Vector3 position, bool ignoreY)
        {
            if (Extent.size == Vector3.zero)
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
        private Bounds InitializeExtentFromMeshCode(string meshcode, GeoReference geo)
        {
            GridCode gridCode = GridCode.Create(meshcode);

            var min = geo.Project(gridCode.Extent.Min);
            var max = geo.Project(gridCode.Extent.Max);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3((float)min.X, (float)min.Y, (float)min.Z), new Vector3((float)max.X, (float)max.Y, (float)max.Z));

            Extent = bounds;
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
            Match match = Regex.Match(Address, @"_grid_([^_]+)_");
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