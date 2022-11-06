using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.Basemap;
using PLATEAU.Interop;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class GSIMapLoader
    {
        private static readonly string mapDownloadDest =
            Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "GSIMapImages"));
        private const string MapMaterialPath = "Packages/com.synesthesias.plateau-unity-sdk/Materials/MapUnlitMaterial.mat";

        public static async Task Load(Extent extent)
        {
            await DownloadAsync(extent);
        }

        private static async Task DownloadAsync(Extent extent)
        {
            var mapPaths = new List<string>();
            using var downloader = VectorTileDownloader.Create(mapDownloadDest, extent);
            int tileCount = downloader.TileCount;
            for (int i = 0; i < tileCount; i++)
            {
                await Task.Run(() =>
                {
                    downloader.Download(i, out var tileCoord, out string path);
                });
            }
        }
    }
}
