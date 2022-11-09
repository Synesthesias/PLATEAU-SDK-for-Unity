using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.Basemap;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// <see cref="GSIMapLoader"/> に次の機能を追加したものです:
    /// ・カメラの位置によってズームレベル（地図の詳細度）を切り替えて表示します
    /// </summary>
    public class GSIMapLoaderZoomSwitch
    {
        private const int minZoomLevel = 3;
        private const int maxZoomLevel = 16;
        
        
        /// <summary>
        /// 画面に同時に映る地図タイルの枚数がこの枚数以下になるようにズームレベルが調整されます。
        /// </summary>
        private const int maxMapCountInScreen = 30;
        
        private readonly GeoReference geoReference;
        private const int mapUpdateInterval = 5;
        private int framesTillMapUpdate = 0;
        private Task mapUpdateTask;
        private readonly List<Material> mapMaterials = new List<Material>();
        private int prevZoomLevel = -1;
        private CancellationTokenSource cancel;

        public GSIMapLoaderZoomSwitch(GeoReference geoReference, CancellationTokenSource cancel)
        {
            this.geoReference = geoReference;
            this.cancel = cancel;
        }

        public void Update(Camera cam)
        {
            if (--this.framesTillMapUpdate <= 0)
            {
                this.framesTillMapUpdate = mapUpdateInterval;
                if (this.mapUpdateTask == null || this.mapUpdateTask.IsCompleted)
                {
                    this.mapUpdateTask = MapUpdateTask(cam);
                    this.mapUpdateTask.ContinueWithErrorCatch();
                }
            }
        }

        /// <summary>
        /// 利用終了時にコールしてください。
        /// </summary>
        public void DestroyMaterials()
        {
            foreach (var mat in this.mapMaterials)
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        private async Task MapUpdateTask(Camera cam)
        {
            this.framesTillMapUpdate = mapUpdateInterval;
            var extent = CalcCameraExtent(cam, this.geoReference);
            int zoomLevel = CalcZoomLevel(extent);
            if (zoomLevel != this.prevZoomLevel)
            {
                DisableExceptZoomLevel(zoomLevel);
            }

            var materials = await GSIMapLoader
                .DownloadAndPlaceAsync(extent, this.geoReference, zoomLevel, this.cancel.Token);
            this.mapMaterials.AddRange(materials);
            this.prevZoomLevel = zoomLevel;
        }

        private static Extent CalcCameraExtent(Camera cam, GeoReference geoRef)
        {
            var camTrans = cam.transform;
            float distance = camTrans.position.y;
            var lowerLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, distance));
            var upperRight = cam.ViewportToWorldPoint(new Vector3(1, 1, distance));
            var camExtent = ToExtent(lowerLeft, upperRight, geoRef);
            return camExtent;
        }

        private static Extent ToExtent(Vector3 lowerLeft, Vector3 upperRight, GeoReference geoReference)
        {
            float minX = Math.Min(lowerLeft.x, upperRight.x);
            float maxX = Math.Max(lowerLeft.x, upperRight.x);
            float minZ = Math.Min(lowerLeft.z, upperRight.z);
            float maxZ = Math.Max(lowerLeft.z, upperRight.z);
            var min = new PlateauVector3d(minX, -9999, minZ);
            var max = new PlateauVector3d(maxX, 9999, maxZ);
            var geoMin = geoReference.Unproject(min);
            var geoMax = geoReference.Unproject(max);
            return new Extent(geoMin, geoMax);
        }

        private static int CalcZoomLevel(Extent cameraExtent)
        {
            for (int zoom = maxZoomLevel; zoom >= minZoomLevel; zoom--)
            {
                var tile = TileProjection.Project(cameraExtent.Center, zoom);
                var tileExtent = TileProjection.Unproject(tile);
                double tileCountX =
                    (cameraExtent.Max.Longitude - cameraExtent.Min.Longitude) /
                    (tileExtent.Max.Longitude - tileExtent.Min.Longitude);
                double tileCountY =
                    (cameraExtent.Max.Latitude - cameraExtent.Min.Latitude) /
                    (tileExtent.Max.Latitude - tileExtent.Min.Latitude);
                if (tileCountX * tileCountY <= maxMapCountInScreen)
                {
                    return zoom;
                }
            }

            return maxZoomLevel;
        }

        private void DisableExceptZoomLevel(int zoomLevel)
        {
            var mapRoot = GameObject.Find(GSIMapLoader.MapRootObjName);
            if (mapRoot == null) return;
            for (int zoom = minZoomLevel; zoom <= maxZoomLevel; zoom++)
            {

                // GameObject を SetActiveします。       
                var zoomLevelTrans = mapRoot.transform.Find(zoom.ToString());
                if (zoomLevelTrans == null) continue;
                zoomLevelTrans.gameObject.SetActive(zoom == zoomLevel);
            }
        }


    }
}
