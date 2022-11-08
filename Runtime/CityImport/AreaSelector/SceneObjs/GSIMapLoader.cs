﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.Basemap;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 地理院地図タイルをダウンロードしてシーンに配置します。
    /// </summary>
    public static class GSIMapLoader
    {
        private static readonly string mapDownloadDest =
            Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "GSIMapImages"));
        private const string mapMaterialPath = "Packages/com.synesthesias.plateau-unity-sdk/Materials/MapUnlitMaterial.mat";
        private const int timeOutSec = 10;
        public const string MapRootObjName = "GSIMaps";
        
        /// <summary>
        /// 地理院地図タイルをダウンロードして、シーンに配置します。
        /// メインスレッドで呼ぶ必要があります。
        /// </summary>
        public static async Task DownloadAndPlaceAsync(Extent extent, GeoReference geoReference, int zoomLevel, CancellationToken cancel)
        {
            using var downloader = VectorTileDownloader.Create(mapDownloadDest, extent, zoomLevel);
            int tileCount = downloader.TileCount;
            for (int i = 0; i < tileCount; i++)
            {

                MapTile mapTile = null;
                string mapFilePath = downloader.CalcDestPath(i);
                var tileCoord = downloader.GetTileCoordinate(i);
                
                var mapRoot = GameObjectUtil.AssureGameObject(MapRootObjName).transform;
                var zoomLevelTrans = GameObjectUtil.AssureGameObjectInChild(zoomLevel.ToString(), mapRoot).transform;
                var rowTrans = GameObjectUtil.AssureGameObjectInChild(tileCoord.Row.ToString(), zoomLevelTrans).transform;
                var mapName = $"{tileCoord.Column}";
                var mapTrans = rowTrans.Find(mapName);
                if ( mapTrans != null)
                {   // すでにマップがシーンに配置済みのケース
                    mapTrans.gameObject.SetActive(true);
                    continue;
                }
                
                await Task.Run(() =>
                {
                    
                    if (!File.Exists(mapFilePath))
                    {
                        downloader.Download(i, out var downloadedTileCoord, out string downloadDestPath);
                        Assert.AreEqual(mapFilePath, downloadDestPath);
                        Assert.AreEqual(tileCoord, downloadedTileCoord);
                    }
                    mapTile = new MapTile(mapFilePath, tileCoord);
                });
                if (cancel.IsCancellationRequested)
                {
                    Debug.Log("Map Download is Cancelled.");
                    break;
                }
                
                await PlaceAsGameObj(mapTile, geoReference, rowTrans, mapName);
            }
        }

        private static async Task PlaceAsGameObj(MapTile mapTile, GeoReference geoReference, Transform parentTrans, string mapObjName)
        {
            if (parentTrans.Find(mapObjName) != null)
            {   // すでに配置済みのケース
                return;
            }
            var mapMaterial = AssetDatabase.LoadAssetAtPath<Material>(mapMaterialPath);
            if (mapMaterial == null)
            {
                Debug.LogError("Could not find material for map.");
            }
            var texture = await TextureLoader.LoadAsync(mapTile.Path, timeOutSec);
            var gameObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            gameObj.name = mapObjName;
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
            renderer.sharedMaterial = mat;
        }

        private class MapTile
        {
            public string Path;
            public TileCoordinate TileCoordinate;

            public MapTile(string path, TileCoordinate tileCoordinate)
            {
                this.Path = path;
                this.TileCoordinate = tileCoordinate;
            }

            /// <summary>
            /// タイルに対応するUnity上の位置を (min, max) で返します。
            /// </summary>
            public (Vector3 min, Vector3 max) ToUnityRange(GeoReference geoReference)
            {
                var extent = TileProjection.Unproject(this.TileCoordinate);
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
