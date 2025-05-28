using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileオブジェクトに付与し、ダウンサンプルレベルごとのAddress情報を保持するコンポーネント
    /// </summary>
    [Serializable]
    public class PLATEAUDynamicTile
    {

        [Serializable]
        public struct DebugInfoStruct
        {
            public float Distance;
            public LoadState NextLoadState;
        }

        [SerializeField]
        public DebugInfoStruct DebugInfo;


        //public enum LoadState
        //{
        //    None,
        //    Load,
        //    Unload
        //}

        public LoadState NextLoadState { get; set; } = LoadState.None;

        public bool IsLoadedOrLoading
        {
            get => LoadedObject != null || IsLoading;
        }


        [SerializeField]
        private string originalAddress;
        public string OriginalAddress
        {
            get => originalAddress;
            set => originalAddress = value;
        }

        [SerializeField]
        private bool isExcludeTile;
        public bool IsExcludeTile
        {
            get => isExcludeTile;
            set => isExcludeTile = value;
        }

        /// <summary>
        /// 指定したダウンサンプルレベルのAddressを取得
        /// </summary>
        public string GetAddress(int downSampleLevel)
        {
            return originalAddress + "_down_" + downSampleLevel;
        }

        /// <summary>
        /// オリジナル（downSampleLevel=0）のアドレスを返す。
        /// </summary>
        public string GetOriginalAddress()
        {
            return GetAddress(0);
        }

        public string GetTileAddress(int downSampleLevel)
        {
            return IsExcludeTile
                ? GetOriginalAddress()
                : GetAddress(downSampleLevel);
        }

        //public string GetTileAddress(int downSampleLevel, bool excluded)
        //{
        //    return excluded
        //        ? GetOriginalAddress()
        //        : GetAddress(downSampleLevel);
        //}

        public TileBounds GetTileState()
        {
            return new TileBounds(Extent.min, Extent.max);//, LoadedObject != null ? LoadedState.Loaded : LoadedState.NotLoaded)
            //{
            //    CameraPosition = Vector3.zero,
            //    DefaultLoadDistance = 0f
            //};
        }

        public PLATEAUDynamicTile(string address, Transform parent, GameObject original = null)
        {
            originalAddress = address;
            Parent = parent;

            if(original != null)
            {
                InitializeExtentFromGameObject(original);
            }
            else
            {
                var meshcode = GetMeshCode();
                InitializeExtentFromMeshCode(meshcode);
            }
        }

        public bool IsLoading { get; set; }

        [SerializeField]
        private GameObject loadedObject;
        public GameObject LoadedObject
        {
            get => loadedObject;
            set => loadedObject = value;
        }
        //GameObject LoadedObject { get; set; }

        [SerializeField]
        private Transform Parent;

        public Transform GetParent()
        {
            return Parent;
        }

        [SerializeField]
        private Bounds Extent;

        public Bounds GetExtent()
        {
            return Extent;
        }

        public float GetDistance(Vector3 position, bool ignoreY)
        {
            if (Extent == null)
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

            //debug info =============================================
            DebugInfo.Distance = distance;
            //debug info =============================================

            return distance;
        }

        private Bounds InitializeExtentFromMeshCode(string meshcode)
        {
            GridCode gridCode = GridCode.Create(meshcode);

            var geo = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>().GeoReference;

            var min = geo.Project(gridCode.Extent.Min);
            var max = geo.Project(gridCode.Extent.Max);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3((float)min.X, (float)min.Y, (float)min.Z), new Vector3((float)max.X, (float)max.Y, (float)max.Z));

            Extent = bounds;

            // Debug Draw Rect
            DrawBounds(bounds, Color.red, 30f);

            return bounds;
        }

        private Bounds InitializeExtentFromGameObject(GameObject obj)
        {
            var bounds = obj.GetComponent<Renderer>().bounds;
            Extent = bounds;

            // Debug Draw Rect
            DrawBounds(bounds, Color.red, 30f);

            return bounds;
        }

        //tile_zoom_(タイルのズームレベル)_grid_(タイルの位置を示すメッシュコード)_(従来のゲームオブジェクト名)_(同名の場合のID)
        //tile_zoom_0_grid_meshcode_gameobjectname_0
        private string GetMeshCode()
        {
            Match match = Regex.Match(originalAddress, @"_grid_([^_]+)_");
            if (match.Success)
            {
                string meshcode = match.Groups[1].Value;

                //Debug.Log($"メッシュコード: {meshcode}");
                return meshcode;
            }
            Debug.LogError("メッシュコードが見つかりません");
            return null;     
        }

        // Debug Draw Bounds
        public static void DrawBounds(Bounds bounds, Color color, float duration = 0f)
        {
            // 底面の4点
            Vector3 p1 = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            Vector3 p2 = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            Vector3 p3 = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            Vector3 p4 = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);

            // 上面の4点
            Vector3 p5 = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            Vector3 p6 = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            Vector3 p7 = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
            Vector3 p8 = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);

            // 底面の線
            Debug.DrawLine(p1, p2, color, duration);
            Debug.DrawLine(p2, p3, color, duration);
            Debug.DrawLine(p3, p4, color, duration);
            Debug.DrawLine(p4, p1, color, duration);

            // 上面の線
            Debug.DrawLine(p5, p6, color, duration);
            Debug.DrawLine(p6, p7, color, duration);
            Debug.DrawLine(p7, p8, color, duration);
            Debug.DrawLine(p8, p5, color, duration);

            // 側面の線
            Debug.DrawLine(p1, p5, color, duration);
            Debug.DrawLine(p2, p6, color, duration);
            Debug.DrawLine(p3, p7, color, duration);
            Debug.DrawLine(p4, p8, color, duration);
        }

    }
} 