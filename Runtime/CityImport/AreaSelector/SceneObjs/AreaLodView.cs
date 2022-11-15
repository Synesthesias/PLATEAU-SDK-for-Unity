using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Udx;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 範囲選択画面で、メッシュコードごとに利用可能なLODを表示します。
    /// <see cref="AreaLodController"/> によって保持されます。
    /// </summary>
    public class AreaLodView
    {
        private readonly PackageToLodDict packageToLodDict;
        private readonly Vector3 meshCodeUnityPositionUpperLeft;
        private readonly Vector3 meshCodeUnityPositionLowerRight;
        private const string iconDirPath = "Packages/com.synesthesias.plateau-unity-sdk/Images/AreaSelect";
        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> iconDict;

        public AreaLodView(PackageToLodDict packageToLodDict, Vector3 meshCodeUnityPositionUpperLeft, Vector3 meshCodeUnityPositionLowerRight)
        {
            this.packageToLodDict = packageToLodDict;
            this.meshCodeUnityPositionUpperLeft = meshCodeUnityPositionUpperLeft;
            this.meshCodeUnityPositionLowerRight = meshCodeUnityPositionLowerRight;
        }

        /// <summary>
        /// 地図上に利用可能LODを表示するためのアイコンをロードします。
        /// </summary>
        public static void Init()
        {
            iconDict = ComposeIconDict();
        }

        public void DrawHandles(Camera camera)
        {
            #if UNITY_EDITOR
            if (this.packageToLodDict == null) return;
            if (iconDict == null)
            {
                Debug.LogError("Failed to load icons.");
                return;
            }

            // アイコンの表示を開始する地点は、地域メッシュコードの左上から、オフセット(スクリーンスペース)だけ動かした地点とします。
            var pos = this.meshCodeUnityPositionUpperLeft;
            var posOffsetScreenSpace = new Vector3(5, -5, 0);
            pos = camera.ScreenToWorldPoint(camera.WorldToScreenPoint(pos) + posOffsetScreenSpace);
            
            // アイコンを表示します。
            foreach (var packageToLods in this.packageToLodDict)
            {
                if (packageToLods.Value.IsEmpty) continue;
                uint maxLod = packageToLods.Value.Max();
                var package = packageToLods.Key;
                if (!iconDict.TryGetValue((package, maxLod), out var iconTex)) continue;

                float meshCodeScreenWidth =
                    (camera.WorldToScreenPoint(this.meshCodeUnityPositionLowerRight) -
                     camera.WorldToScreenPoint(this.meshCodeUnityPositionUpperLeft))
                    .x;
                
                // 地域メッシュコードの枠内にアイコンが4つ並ぶ程度の大きさ
                float iconWidth = Mathf.Min(70, meshCodeScreenWidth / 4);
                
                var style = new GUIStyle(EditorStyles.label)
                {
                    fixedHeight = iconWidth,
                    fixedWidth = iconWidth,
                    alignment = TextAnchor.UpperLeft
                };
                var content = new GUIContent(iconTex);
                Handles.Label(pos, content, style);

                var iconScreenPosLeft = camera.WorldToScreenPoint(pos);
                var iconScreenPosRight = iconScreenPosLeft + new Vector3(iconWidth, 0, 0);
                var distance = Mathf.Abs(camera.transform.position.y - pos.y);
                var iconWorldPosRight = camera.ScreenToWorldPoint(new Vector3(iconScreenPosRight.x, iconScreenPosRight.y, distance));
                pos += new Vector3(iconWorldPosRight.x - pos.x, 0, 0);
            }
            #endif
        }

        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> ComposeIconDict()
        {
            return new ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture>(
                new Dictionary<(PredefinedCityModelPackage package, uint lod), Texture>
                {
                    {(PredefinedCityModelPackage.Building, 0), LoadIcon("icon_building_lod1.png")},
                    {(PredefinedCityModelPackage.Building, 1), LoadIcon("icon_building_lod1.png")},
                    {(PredefinedCityModelPackage.Building, 2), LoadIcon("icon_building_lod2.png")},
                    {(PredefinedCityModelPackage.Building, 3), LoadIcon("icon_building_lod3.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 0), LoadIcon("icon_cityfurniture_lod1.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 1), LoadIcon("icon_cityfurniture_lod1.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 2), LoadIcon("icon_cityfurniture_lod2.png")},
                    {(PredefinedCityModelPackage.CityFurniture, 3), LoadIcon("icon_cityfurniture_lod3.png")},
                    {(PredefinedCityModelPackage.Road, 0), LoadIcon("icon_road_lod1.png")},
                    {(PredefinedCityModelPackage.Road, 1), LoadIcon("icon_road_lod1.png")},
                    {(PredefinedCityModelPackage.Road, 2), LoadIcon("icon_road_lod2.png")},
                    {(PredefinedCityModelPackage.Road, 3), LoadIcon("icon_road_lod3.png")},
                    {(PredefinedCityModelPackage.Vegetation, 0), LoadIcon("icon_vegetation_lod1.png")},
                    {(PredefinedCityModelPackage.Vegetation, 1), LoadIcon("icon_vegetation_lod1.png")},
                    {(PredefinedCityModelPackage.Vegetation, 2), LoadIcon("icon_vegetation_lod2.png")},
                    {(PredefinedCityModelPackage.Vegetation, 3), LoadIcon("icon_vegetation_lod3.png")},
                });
        }

        /// <summary>
        /// このパッケージの利用可能LODを地図上で表示するかどうかを返します。
        /// </summary>
        public static bool HasIconOfPackage(PredefinedCityModelPackage package)
        {
            iconDict ??= ComposeIconDict();
            return iconDict.TryGetValue((package, 1), out _);
        }

        private static Texture LoadIcon(string relativePath)
        {
            #if UNITY_EDITOR
            string path = Path.Combine(iconDirPath, relativePath).Replace('\\', '/');
            var texture =  AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (texture == null)
            {
                Debug.LogError($"Icon image file is not found : {path}");
            }
            return texture;
            #else
            return null;
            #endif
        }
    }
}
