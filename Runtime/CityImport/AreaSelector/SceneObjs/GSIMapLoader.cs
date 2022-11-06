using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.Basemap;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class GSIMapLoader
    {
        private static readonly string mapDownloadDest =
            Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "GSIMapImages"));
        private const string mapMaterialPath = "Packages/com.synesthesias.plateau-unity-sdk/Materials/MapUnlitMaterial.mat";
        private const int timeOutSec = 10;

        public static async Task Load(Extent extent, GeoReference geoReference)
        {
            var mapTiles = await DownloadAsync(extent);
            await PlaceAsGameObj(mapTiles, geoReference);
        }

        private static async Task<List<MapTile>> DownloadAsync(Extent extent)
        {
            var mapTiles = new List<MapTile>();
            using var downloader = VectorTileDownloader.Create(mapDownloadDest, extent);
            int tileCount = downloader.TileCount;
            for (int i = 0; i < tileCount; i++)
            {
                await Task.Run(() =>
                {
                    downloader.Download(i, out var tileCoord, out string path);
                    var mapTile = new MapTile(path, tileCoord);
                    mapTiles.Add(mapTile);
                });
            }

            return mapTiles;
        }

        private static async Task PlaceAsGameObj(IEnumerable<MapTile> mapTiles, GeoReference geoReference)
        {
            var mapMaterial = AssetDatabase.LoadAssetAtPath<Material>(mapMaterialPath);
            if (mapMaterial == null)
            {
                Debug.LogError("Could not find material for map.");
            }
            foreach (var mapTile in mapTiles)
            {
                var texture = await Task.Run(() =>
                    TextureLoader.LoadAsync(mapTile.Path, timeOutSec)
                );
                var gameObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                var trans = gameObj.transform;
                trans.position = mapTile.UnityCenter(geoReference);
                trans.localScale = mapTile.UnityScale(geoReference);
                var renderer = gameObj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = mapMaterial;
                var mat = renderer.material;
                mat.mainTexture = texture;
            }
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
