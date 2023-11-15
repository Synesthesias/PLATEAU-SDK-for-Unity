using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.Basemap;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
#else
using System;
#endif

namespace PLATEAU.CityImport.AreaSelector.Display.Maps
{
    /// <summary>
    /// 地理院地図タイルをダウンロードしてシーンに配置します。
    /// </summary>
    public static class GSIMapLoader
    {
        private static readonly string mapDownloadDest =
            Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "GSIMapImages"));

        #if UNITY_EDITOR
        private static readonly string mapMaterialDir = PathUtil.SdkPathToAssetPath("Materials");
        #endif
        private const string MapMaterialNameBuiltInRP = "MapUnlitMaterial_BuiltInRP.mat";
        private const string MapMaterialNameURP = "MapUnlitMaterial_URP.mat";
        private const string MapMaterialNameHDRP = "MapUnlitMaterial_HDRP.mat";
        private const int TimeOutSec = 10;
        public const string MapRootObjName = "Basemap";
        
        /// <summary>
        /// 地理院地図タイルをダウンロードして、シーンに配置します。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        /// <returns>生成したマテリアルのリストを返します。このマテリアルはメソッドの利用者が利用終了時に廃棄してください。</returns>
        public static async Task<List<Material>> DownloadAndPlaceAsync(Extent extent, GeoReference geoReference, int zoomLevel, CancellationToken cancel)
        {
            var generatedMaterials = new List<Material>();
            using var downloader = VectorTileDownloader.Create(mapDownloadDest, extent, zoomLevel);
            int tileCount = downloader.TileCount;
            for (int i = 0; i < tileCount; i++)
            {
                
                string mapFilePath = downloader.CalcDestPath(i);
                var tileCoord = downloader.GetTileCoordinate(i);

                var mapRoot = GameObjectUtil.AssureGameObject(MapRootObjName).transform;
                var zoomLevelTrans = GameObjectUtil.AssureGameObjectInChild(zoomLevel.ToString(), mapRoot).transform;
                var rowTrans = GameObjectUtil.AssureGameObjectInChild(tileCoord.Row.ToString(), zoomLevelTrans)
                    .transform;
                var mapName = $"{tileCoord.Column}";
                var mapTrans = rowTrans.Find(mapName);
                if (mapTrans != null)
                {
                    // すでにマップがシーンに配置済みのケース
                    mapTrans.gameObject.SetActive(true);
                    continue;
                }

                var mapTile = new MapTile(mapFilePath, tileCoord);
                bool isSucceed = await Task.Run(() => DownloadFileIfNotExist(downloader, i, mapFilePath));
                if (isSucceed)
                {
                    await PlaceAsGameObj(mapTile, geoReference, rowTrans, mapName, generatedMaterials);
                }
                else
                {
                    Debug.LogError("Failed to load a map tile image.");
                }

                if (cancel.IsCancellationRequested)
                {
                    break;
                }

            }

            return generatedMaterials;
        }

        private static bool DownloadFileIfNotExist(VectorTileDownloader downloader, int index, string mapFilePath)
        {
            // ファイルがすでにあるなら、そのファイルの書き込み完了を待って、そのファイルを利用します。
            if (File.Exists(mapFilePath))
            {
                int tryLeft = 5;
                while (true)
                {
                    if (!IsFileLocked(new FileInfo(mapFilePath))) break;
                    Thread.Sleep(50);
                    if (--tryLeft <= 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            // ファイルがなければダウンロードします。
            downloader.Download(index);
            return true;
        }

        private static Material LoadMapMaterial()
        {
            string matFileName;
            var pipelineAsset = GraphicsSettings.renderPipelineAsset;
            if (pipelineAsset == null) 
            {   // Built-in Render Pipeline のとき
                matFileName = MapMaterialNameBuiltInRP;
            }
            else
            {   // URP または HDRP のとき
                var pipelineName = pipelineAsset.GetType().Name;
                matFileName = pipelineName switch
                {
                    "UniversalRenderPipelineAsset" => MapMaterialNameURP,
                    "HDRenderPipelineAsset" => MapMaterialNameHDRP,
                    _ => throw new InvalidDataException("Unknown material for pipeline.")
                };
            }

#if UNITY_EDITOR
            string matFilePath = MaterialPathUtil.GetMapMatPath();
            var material = AssetDatabase.LoadAssetAtPath<Material>(matFilePath);
            return material;
#else
            throw new NotImplementedException("Map Load in PlayMode is not implemented.");
#endif
        }

        private static async Task PlaceAsGameObj(MapTile mapTile, GeoReference geoReference, Transform parentTrans, string mapObjName, List<Material> generatedMaterials)
        {
            if (parentTrans == null) return;
            if (parentTrans.Find(mapObjName) != null)
            {   // すでに配置済みのケース
                return;
            }

            var mapMaterial = LoadMapMaterial();

            // ダウンロードしたテクスチャファイルをロードします。
            var texture = await TextureLoader.LoadAsync(mapTile.Path, TimeOutSec);

            var gameObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            gameObj.name = mapObjName;
#if (UNITY_EDITOR && UNITY_2019_2_OR_NEWER)
            SceneVisibilityManager.instance.DisablePicking(gameObj, true);
#endif
            var trans = gameObj.transform;
            trans.position = mapTile.UnityCenter(geoReference);
            // UnityのPlaneは 10m×10m なので 0.1倍します。
            trans.localScale = mapTile.UnityScale(geoReference) * 0.1f;
            trans.eulerAngles = new Vector3(0, 180, 0);
            trans.parent = parentTrans;
            var renderer = gameObj.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mapMaterial;
            var mat = new Material(renderer.sharedMaterial)
            {
                mainTexture = texture
            };
            // ReSharper disable once Unity.InefficientPropertyAccess
            renderer.sharedMaterial = mat;
            generatedMaterials.Add(mat);
        }
        
        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using FileStream fs = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        private class MapTile
        {
            public readonly string Path;
            private readonly TileCoordinate tileCoordinate;

            public MapTile(string path, TileCoordinate tileCoordinate)
            {
                this.Path = path;
                this.tileCoordinate = tileCoordinate;
            }

            /// <summary>
            /// タイルに対応するUnity上の位置を (min, max) で返します。
            /// </summary>
            public (Vector3 min, Vector3 max) ToUnityRange(GeoReference geoReference)
            {
                var extent = TileProjection.Unproject(this.tileCoordinate);
                var min = geoReference.Project(extent.Min).ToUnityVector();
                var max = geoReference.Project(extent.Max).ToUnityVector();
                return (min, max);
            }

            public Vector3 UnityCenter(GeoReference geoReference)
            {
                var (min, max) = ToUnityRange(geoReference);
                return (min + max) * 0.5f;
            }

            public Vector3 UnityScale(GeoReference geoReference)
            {
                var (min, max) = ToUnityRange(geoReference);
                return new Vector3(max.x - min.x, 1, max.z - min.z);
            }
        }
    }
}
