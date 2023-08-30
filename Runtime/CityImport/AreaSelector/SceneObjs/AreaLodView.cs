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
        
        #if UNITY_EDITOR
        private static readonly string iconDirPath = PathUtil.SdkPathToAssetPath("Images/AreaSelect");
        private static readonly float maxIconWidth = 50 * EditorGUIUtility.pixelsPerPoint;
        #else
        private static readonly float maxIconWidth = 50;
        #endif
        
        /// <summary> 利用可能を意味するアイコンの不透明度です。 </summary>
        private const float IconOpacityAvailable = 0.95f;
        /// <summary> アイコンの幅がメッシュコード幅の何分の1であるかです。 </summary>
        private const float IconWidthDivider = 4.3f;
        private const int MaxIconCnt = 8;
        private const int MaxIconCol = 4;
        private const int MaxIconRow = 2;
        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> iconDict;
        private struct LodTexturePair {
            public int Lod;
            public Texture IconTexture;
        }

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
            
            // アイコンが存在するパッケージ種について、表示すべきアイコンを求めます
            var iconAvailableForPackages = iconDict.Keys.Select(tuple => tuple.package).Distinct();
            var iconInfoDict = new Dictionary<string, LodTexturePair>();
            foreach(var package in iconAvailableForPackages)
            {
                // パッケージが存在しないときは -1 になります
                var maxLod = this.packageToLodDict.GetLod(package);
                if (maxLod < 0)
                    continue;
                
                // パッケージとLODに対応するテクスチャを求めます
                if (!iconDict.TryGetValue((package, (uint)maxLod), out var iconTex)) continue;

                // アイコンはパッケージ毎に割当てられているが、同名のアイコンファイルが利用されるのは防ぐ
                var lodTexturePair = new LodTexturePair { Lod = maxLod, IconTexture = iconTex };
                var iconFileName = GetIconFileName(package);
                if (iconInfoDict.ContainsKey(iconFileName))
                {
                    if (iconInfoDict[iconFileName].Lod < maxLod) 
                    {
                        iconInfoDict[iconFileName] = lodTexturePair;
                    }
                } 
                else 
                {
                    iconInfoDict.Add(iconFileName, lodTexturePair);
                }
            }

            // 範囲選択画面に表示する順番に並び替える
            var lodTexturePairList = (from iconFileName in GetIconFileNameList() where iconInfoDict.ContainsKey(iconFileName) select iconInfoDict[iconFileName]).ToList();
            if (lodTexturePairList.Count <= 0)
            {
                Debug.LogError("lodTexturePairList.Count <= 0");
                return;
            }

            var monitorDpiScalingFactor = EditorGUIUtility.pixelsPerPoint;
            var meshCodeScreenWidth = (camera.WorldToScreenPoint(this.meshCodeUnityPositionLowerRight) - camera.WorldToScreenPoint(this.meshCodeUnityPositionUpperLeft)).x;

            // 地域メッシュコードの枠内にアイコンが4つ並ぶ程度の大きさ
            var iconWidth = Mathf.Min(maxIconWidth, meshCodeScreenWidth / IconWidthDivider) / monitorDpiScalingFactor;
            var iconCnt = Math.Min(lodTexturePairList.Count, MaxIconCnt);
            
            // アイコンを中央揃えで左から右に並べたとき、左上の座標を求めます。
            var meshCodeCenterUnityPos = (this.meshCodeUnityPositionUpperLeft + this.meshCodeUnityPositionLowerRight) * 0.5f;

            // アイコンを表示します。
            var offsetVec = Vector3.zero;
            for (var i = 0; i < iconCnt; ++i) 
            {
                var colIndex = i % MaxIconCol;
                var rowIndex = i / MaxIconCol;
                // 表示するアイコン数に応じて現在の行において最大何個のアイコンを表示するか求める
                var colCount = 0 < (iconCnt - rowIndex * MaxIconCol) / MaxIconCol ? MaxIconCol : (iconCnt - rowIndex * MaxIconCol) % MaxIconCol;
                // 表示するアイコン数に応じて最大の行数を求める
                var rowCount = MaxIconCol < iconCnt ? MaxIconRow : 1;
                var xOffset = iconCnt is > MaxIconCol and <= MaxIconCnt && 0 < rowIndex
                    // 2行目のオフセット値
                    ? iconWidth * colIndex - iconWidth * 2
                    // 1行目のオフセット値
                    : iconWidth * colIndex - iconWidth * colCount * 0.5f;
                var yOffset = 1 < rowCount ? iconWidth - iconWidth * rowIndex : iconWidth * 0.5f;

                offsetVec.x = xOffset * monitorDpiScalingFactor;
                offsetVec.y = yOffset * monitorDpiScalingFactor;
                var iconPos = camera.ScreenToWorldPoint(camera.WorldToScreenPoint(meshCodeCenterUnityPos) + offsetVec);

                var prevBackgroundColor = GUI.contentColor;
                GUI.contentColor = new Color(1f, 1f, 1f, IconOpacityAvailable);
                var style = new GUIStyle(EditorStyles.label)
                {
                    fixedHeight = iconWidth,
                    fixedWidth = iconWidth,
                    alignment = TextAnchor.UpperLeft,
                    stretchWidth = true
                };
                var content = new GUIContent(lodTexturePairList[i].IconTexture);
                Handles.Label(iconPos, content, style);
                GUI.contentColor = prevBackgroundColor;
            }
#endif
        }

        /// <summary>
        /// 指定パッケージで表示するアイコンファイル名を取得
        /// </summary>
        /// <param name="package">対応するアイコンファイル名を知りたいパッケージ</param>
        /// <returns>アイコンファイル名</returns>
        private static string GetIconFileName(PredefinedCityModelPackage package) 
        {
            return package switch 
            {
                PredefinedCityModelPackage.Building => "building.png",
                PredefinedCityModelPackage.Road => "traffic.png",
                PredefinedCityModelPackage.UrbanPlanningDecision => "other.png",
                PredefinedCityModelPackage.LandUse => "other.png",
                PredefinedCityModelPackage.CityFurniture => "props.png",
                PredefinedCityModelPackage.Vegetation => "plants.png",
                PredefinedCityModelPackage.Relief => "terrain.png",
                PredefinedCityModelPackage.DisasterRisk => "other.png",
                PredefinedCityModelPackage.Railway => "traffic.png",
                PredefinedCityModelPackage.Waterway => "traffic.png",
                PredefinedCityModelPackage.WaterBody => "other.png",
                PredefinedCityModelPackage.Bridge => "bridge.png",
                PredefinedCityModelPackage.Track => "traffic.png",
                PredefinedCityModelPackage.Square => "traffic.png",
                PredefinedCityModelPackage.Tunnel => "bridge.png",
                PredefinedCityModelPackage.UndergroundFacility => "underground.png",
                PredefinedCityModelPackage.UndergroundBuilding => "underground.png",
                PredefinedCityModelPackage.Area => "other.png",
                PredefinedCityModelPackage.OtherConstruction => "other.png",
                PredefinedCityModelPackage.Generic => "other.png",
                PredefinedCityModelPackage.Unknown => "other.png",
                _ => "other.png"
            };
        }

        /// <summary>
        /// 範囲選択画面に表示する順番でアイコンファイル名リストを返す
        /// </summary>
        /// <returns>アイコンファイル名リスト</returns>
        private static IEnumerable<string> GetIconFileNameList() {
            return new List<string> { "building.png", "plants.png", "props.png", "traffic.png", "underground.png", "bridge.png", "terrain.png", "other.png" };
        }
        
        /// <summary>
        /// 利用可能なLODをアイコンで表示するためのアイコン画像をロードし、辞書にして返します。
        /// </summary>
        /// <returns>キーはパッケージ種とLOD数値の組、値はテクスチャとするアイコン辞書を返します。</returns>
        private static ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture> LoadIconFiles()
        {
            var concurrentDict = new ConcurrentDictionary<(PredefinedCityModelPackage package, uint lod), Texture>();
            var allPackages = EnumUtil.EachFlags(PredefinedCityModelPackageExtension.All());
            var lodDirNames = new List<string> {"LOD1", "LOD1", "LOD2", "LOD3", "LOD4"};
            foreach (var package in allPackages) {
                for (uint i = 0; i <= 4; i++) {
                    concurrentDict.AddOrUpdate((package, i), LoadIcon($"{lodDirNames[(int)i]}/{GetIconFileName(package)}"), (_, texture) => texture);
                }
            }

            return concurrentDict;
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
    }
}
