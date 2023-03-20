using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Util;
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
        private const string iconsBoxImagePath = "round-window-wide.png";
        
        #if UNITY_EDITOR
        private static readonly string iconDirPath = PathUtil.SdkPathToAssetPath("Images/AreaSelect");
        private static readonly float maxIconWidth = 60 * EditorGUIUtility.pixelsPerPoint;
        #else
        private static readonly float maxIconWidth = 60;
        #endif
        
        /// <summary> 利用可能を意味するアイコンの不透明度です。 </summary>
        private const float iconOpacityAvailable = 0.95f;
        /// <summary> 利用不可を意味するアイコンの不透明度です。 </summary>
        private const float iconOpacityNotAvailable = 0.2f;
        /// <summary> アイコンを包むボックスについて、そのパディング幅がアイコンの何倍であるかです。 </summary>
        private const float boxPaddingRatio = 0.05f;
        /// <summary> アイコンの幅がメッシュコード幅の何分の1であるかです。 </summary>
        private const float iconWidthDivider = 5;
        private static readonly Color boxColor = new Color(0.25f, 0.25f, 0.25f, 0.35f);
        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> iconDict;
        private static Texture boxTex;

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
            iconDict = LoadIconFiles();
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

            // アイコンが存在するパッケージ種について、表示すべきアイコンを求めます。
            var iconAvailableForPackages =
                iconDict.Keys
                    .Select(tuple => tuple.package)
                    .Distinct();
            var iconsToShow = new List<IconToShow>();
            foreach(var package in iconAvailableForPackages)
            {
                int maxLod = this.packageToLodDict.GetLod(package); // パッケージが存在しないときは -1 になります。
                uint iconLod = (uint)Math.Max(maxLod, 0); // 存在しない時は「LOD0」アイコンを半透明で表示します。
                bool isAvailable = maxLod >= 0;
                // パッケージとLODに対応するテクスチャを求めます。
                if (!iconDict.TryGetValue((package, iconLod), out var iconTex)) continue;
                
                iconsToShow.Add(new IconToShow(iconTex, isAvailable));
            }
            
            float monitorDpiScalingFactor = EditorGUIUtility.pixelsPerPoint;

            float meshCodeScreenWidth =
                (camera.WorldToScreenPoint(this.meshCodeUnityPositionLowerRight) -
                 camera.WorldToScreenPoint(this.meshCodeUnityPositionUpperLeft))
                .x;

            // 地域メッシュコードの枠内にアイコンが5つ並ぶ程度の大きさ
            float iconWidth = Mathf.Min(maxIconWidth, meshCodeScreenWidth / iconWidthDivider) / monitorDpiScalingFactor;
            
            // アイコンを中央揃えで左から右に並べたとき、左上の座標を求めます。
            var meshCodeCenterUnityPos = (this.meshCodeUnityPositionUpperLeft + this.meshCodeUnityPositionLowerRight) * 0.5f;
            var posOffsetScreenSpace = new Vector3(-iconWidth * iconsToShow.Count * 0.5f, iconWidth * 0.5f, 0) * monitorDpiScalingFactor;  
            var iconsUpperLeft = camera.ScreenToWorldPoint(camera.WorldToScreenPoint(meshCodeCenterUnityPos) + posOffsetScreenSpace);

            // アイコンを包むボックスを表示します。
            var iconsBoxPaddingScreen = iconWidth * boxPaddingRatio;
            var boxSizeScreen = new Vector2(
                iconWidth * iconsToShow.Count + iconsBoxPaddingScreen * 2,
                iconWidth + iconsBoxPaddingScreen * 2
                );
            var boxPosScreen = camera.WorldToScreenPoint(iconsUpperLeft) + new Vector3(-1,1,0) * iconsBoxPaddingScreen;
            Handles.BeginGUI();
            var prevColor = GUI.color;
            GUI.color = boxColor;
            // ボックスを描画します。ただし、Handles.BeginGUI(); の中では座標系が異なる（特にスクリーン座標系における y座標の向きが異なる）ので変換します。
            var boxRect = new Rect(new Vector2(boxPosScreen.x, boxPosScreen.y * -1 + camera.pixelHeight), boxSizeScreen);
            boxRect.position /= monitorDpiScalingFactor;
            GUI.DrawTexture(boxRect, boxTex, ScaleMode.StretchToFill);
            GUI.color = prevColor;
            Handles.EndGUI();
            
            // アイコンを表示します。
            var iconPos = iconsUpperLeft;
            int i = 0;
            foreach (var iconToShow in iconsToShow)
            {
                i++;
                var prevBackgroundColor = GUI.contentColor;
                GUI.contentColor = new Color(1f, 1f, 1f, iconToShow.IsAvailable ? iconOpacityAvailable : iconOpacityNotAvailable);
                var style = new GUIStyle(EditorStyles.label)
                {
                    fixedHeight = iconWidth,
                    fixedWidth = iconWidth,
                    alignment = TextAnchor.UpperLeft,
                    stretchWidth = true
                };
                var content = new GUIContent(iconToShow.Texture);
                Handles.Label(iconPos, content, style);
                GUI.contentColor = prevBackgroundColor;

                var iconScreenPosLeft = camera.WorldToScreenPoint(iconPos);
                var iconScreenPosRight = iconScreenPosLeft + new Vector3(iconWidth * monitorDpiScalingFactor, 0, 0);
                var distance = Mathf.Abs(camera.transform.position.y - iconPos.y);
                var iconWorldPosRight = camera.ScreenToWorldPoint(new Vector3(iconScreenPosRight.x, iconScreenPosRight.y, distance));
                iconPos += new Vector3(iconWorldPosRight.x - iconPos.x, 0, 0);
            }
            
#endif
        }

        /// <summary>
        /// 利用可能なLODをアイコンで表示するためのアイコン画像をロードし、辞書にして返します。
        /// </summary>
        /// <returns>キーはパッケージ種とLOD数値の組、値はテクスチャとするアイコン辞書を返します。</returns>
        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> LoadIconFiles()
        {
            boxTex = LoadIcon(iconsBoxImagePath);
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
            iconDict ??= LoadIconFiles();
            return iconDict.ContainsKey((package, 1));
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

        private class IconToShow
        {
            public Texture Texture;
            public bool IsAvailable; // falseだと半透明表示にします。

            public IconToShow(Texture texture, bool isAvailable)
            {
                this.Texture = texture;
                this.IsAvailable = isAvailable;
            }
        }
    }
}
