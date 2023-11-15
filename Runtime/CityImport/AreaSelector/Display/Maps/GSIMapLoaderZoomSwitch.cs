using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.Basemap;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Maps
{
    /// <summary>
    /// <see cref="GSIMapLoader"/> に次の機能を追加したものです:
    /// ・カメラの位置によってズームレベル（地図の詳細度）を切り替えて表示します
    /// </summary>
    public class GSIMapLoaderZoomSwitch : IDisposable
    {
        private const int MinZoomLevel = 3;
        private const int MaxZoomLevel = 16;
        private const float UpdateIntervalMilliSec = 500;
        
        
        /// <summary>
        /// 画面に同時に映る地図タイルの枚数がこの枚数以下になるようにズームレベルが調整されます。
        /// </summary>
        private const int MaxMapCountInScreen = 30;
        
        private readonly GeoReference geoReference;
        private Task mapUpdateTask;
        private readonly List<Material> mapMaterials = new List<Material>();
        private int prevZoomLevel = -1;
        private CancellationTokenSource zoomLoadCancel;
        private DateTime lastUpdateTime = DateTime.MinValue;
        private readonly Extent mapAvailableExtent;

        public GSIMapLoaderZoomSwitch(GeoReference geoReference, Extent entireExtent)
        {
            this.geoReference = geoReference;
            // 基準点から離れすぎると座標が歪むので、遠すぎる部分（日本からはみ出す程度）は描画しないようにします。
            this.mapAvailableExtent = new Extent(
                entireExtent.Min + new GeoCoordinate(-10, -10, -9999),
                entireExtent.Max + new GeoCoordinate(10, 10, 9999)
            );
        }

        public void Update(Camera cam)
        {
            if ((DateTime.Now - this.lastUpdateTime).Milliseconds <= UpdateIntervalMilliSec)
            {
                var extent = Extent.Intersection(
                    CalcCameraExtent(cam, this.geoReference),
                    this.mapAvailableExtent
                );
                int zoomLevel = CalcZoomLevel(extent);
                // ズームレベルが切り替わったとき、前のズームレベルを読み込む処理をキャンセルします。
                bool zoomLevelChanged = this.prevZoomLevel != zoomLevel;
                if (zoomLevelChanged)
                {
                    this.zoomLoadCancel?.Cancel();
                    this.zoomLoadCancel = new CancellationTokenSource();
                    DisableExceptZoomLevel(zoomLevel);
                }
                else if (this.mapUpdateTask == null || this.mapUpdateTask.IsCompleted)
                {
                    this.mapUpdateTask = MapUpdateTask(zoomLevel, extent, this.zoomLoadCancel);
                    this.mapUpdateTask.ContinueWithErrorCatch();
                    this.lastUpdateTime = DateTime.Now;
                }
                this.prevZoomLevel = zoomLevel;

            }
        }

        public void Dispose()
        {
            foreach (var mat in this.mapMaterials)
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
            this.zoomLoadCancel?.Cancel();
        }

        private async Task MapUpdateTask(int zoomLevel, Extent extent, CancellationTokenSource downloadCancel)
        {
            var materials = await GSIMapLoader
                .DownloadAndPlaceAsync(extent, this.geoReference, zoomLevel, downloadCancel.Token);
            this.mapMaterials.AddRange(materials);
            
        }

        public static Extent CalcCameraExtent(Camera cam, GeoReference geoRef)
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
            for (int zoom = MaxZoomLevel; zoom >= MinZoomLevel; zoom--)
            {
                var tile = TileProjection.Project(cameraExtent.Center, zoom);
                var tileExtent = TileProjection.Unproject(tile);
                double tileCountX =
                    (cameraExtent.Max.Longitude - cameraExtent.Min.Longitude) /
                    (tileExtent.Max.Longitude - tileExtent.Min.Longitude);
                double tileCountY =
                    (cameraExtent.Max.Latitude - cameraExtent.Min.Latitude) /
                    (tileExtent.Max.Latitude - tileExtent.Min.Latitude);
                if (tileCountX * tileCountY <= MaxMapCountInScreen)
                {
                    return zoom;
                }
            }

            return MaxZoomLevel;
        }

        private static void DisableExceptZoomLevel(int zoomLevel)
        {
            var mapRoot = GameObject.Find(GSIMapLoader.MapRootObjName);
            if (mapRoot == null) return;
            for (int zoom = MinZoomLevel; zoom <= MaxZoomLevel; zoom++)
            {

                // GameObject を SetActiveします。       
                var zoomLevelTrans = mapRoot.transform.Find(zoom.ToString());
                if (zoomLevelTrans == null) continue;
                zoomLevelTrans.gameObject.SetActive(zoom == zoomLevel);
            }
        }
        
    }
}
