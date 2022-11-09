using System;
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
        private GeoReference geoReference;
        private CancellationTokenSource cancel = new CancellationTokenSource();
        private const int mapUpdateInterval = 1;
        private int framesTillMapUpdate;
        private Task mapUpdateTask = null;
        private const int minZoomLevel = 3;
        private const int maxZoomLevel = 16;
        
        // 国土地理院の地図タイルは 256×256 ピクセルで統一されています。
        // 参考 : https://maps.gsi.go.jp/development/siyou.html
        private static int mapImagePixelWidth = 256; 
        
        

        public GSIMapLoaderZoomSwitch(GeoReference geoReference)
        {
            this.geoReference = geoReference;
            this.framesTillMapUpdate = mapUpdateInterval;
        }

        public void Update(Camera cam)
        {
            if (--this.framesTillMapUpdate <= 0)
            {
                if (!(this.mapUpdateTask is { Status: TaskStatus.Running }))
                {
                    this.mapUpdateTask =
                        MapUpdateTask(cam).ContinueWithErrorCatch();
                }
            }
        }

        private async Task MapUpdateTask(Camera cam)
        {
            this.framesTillMapUpdate = mapUpdateInterval;
            var extent = CalcCameraExtent(cam, this.geoReference);
            int zoomLevel = CalcZoomLevel(extent, Screen.width);
            DisableExceptZoomLevel(extent, zoomLevel);
            await GSIMapLoader
                .DownloadAndPlaceAsync(extent, this.geoReference, zoomLevel, this.cancel.Token)
                .ContinueWithErrorCatch();
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

        private static int CalcZoomLevel(Extent cameraExtent, int screenWidth)
        {
            for (int zoom = minZoomLevel; zoom <= maxZoomLevel; zoom++)
            {
                var tile = TileProjection.Project(cameraExtent.Center, zoom);
                var tileExtent = TileProjection.Unproject(tile);
                var screenPixelWidthPerImage = 
                    (tileExtent.Max.Longitude - tileExtent.Min.Longitude) /
                    (cameraExtent.Max.Longitude - cameraExtent.Min.Longitude)
                    * screenWidth;
                double textureZoomRatio = screenPixelWidthPerImage / mapImagePixelWidth;
                if (textureZoomRatio <= 1.5)
                {
                    return zoom;
                }
            }

            return maxZoomLevel;
        }

        private static void DisableExceptZoomLevel(Extent cameraExtent, int zoomLevel)
        {
            var mapRoot = GameObject.Find(GSIMapLoader.MapRootObjName);
            if (mapRoot == null) return;
            for (int zoom = minZoomLevel; zoom <= maxZoomLevel; zoom++)
            {
                
                var zoomLevelTrans = mapRoot.transform.Find(zoom.ToString());
                if (zoomLevelTrans == null) continue;
                zoomLevelTrans.gameObject.SetActive(zoom == zoomLevel);
                // var tileMin = TileProjection.Project(cameraExtent.Min, zoom);
                // var tileMax = TileProjection.Project(cameraExtent.Max, zoom);
                // int rowMin = Math.Min(tileMin.Row, tileMax.Row);
                // int rowMax = Math.Max(tileMin.Row, tileMax.Row);
                // int colMin = Math.Min(tileMin.Column, tileMax.Column);
                // int colMax = Math.Max(tileMin.Column, tileMax.Column);
                // // Debug.Log($"found. zoomLevel={zoom}, rowMin={rowMin}, rowMax={rowMax}, colMin={colMin}, colMax={colMax}");
                // // TODO 同じオブジェクトが複数生成されてしまうのはなんで？
                // int rowObjCount = zoomLevelTrans.childCount;
                // for (int childRowId = 0; childRowId < rowObjCount; childRowId++)
                // {
                //     var rowTrans = zoomLevelTrans.GetChild(childRowId);
                //     string rowStr = rowTrans.name;
                //     bool isValidRow = int.TryParse(rowStr, out int row);
                //     if (!isValidRow) continue;
                //     if (rowMin <= row && row <= rowMax)
                //     {
                //         int colObjCount = zoomLevelTrans.childCount;
                //         for (int childColumnId = 0; childColumnId < colObjCount; childColumnId++)
                //         {
                //             var colTrans = rowTrans.GetChild(childColumnId);
                //             var colStr = colTrans.name;
                //             bool isValidCol = int.TryParse(colStr, out int col);
                //             if (!isValidCol) continue;
                //             if (colMin <= col && col <= colMax)
                //             {
                //                 colTrans.gameObject.SetActive(false);
                //             }
                //         }
                //     }
                // }// var tileMin = TileProjection.Project(cameraExtent.Min, zoom);
                // var tileMax = TileProjection.Project(cameraExtent.Max, zoom);
                // int rowMin = Math.Min(tileMin.Row, tileMax.Row);
                // int rowMax = Math.Max(tileMin.Row, tileMax.Row);
                // int colMin = Math.Min(tileMin.Column, tileMax.Column);
                // int colMax = Math.Max(tileMin.Column, tileMax.Column);
                // // Debug.Log($"found. zoomLevel={zoom}, rowMin={rowMin}, rowMax={rowMax}, colMin={colMin}, colMax={colMax}");
                // // TODO 同じオブジェクトが複数生成されてしまうのはなんで？
                // int rowObjCount = zoomLevelTrans.childCount;
                // for (int childRowId = 0; childRowId < rowObjCount; childRowId++)
                // {
                //     var rowTrans = zoomLevelTrans.GetChild(childRowId);
                //     string rowStr = rowTrans.name;
                //     bool isValidRow = int.TryParse(rowStr, out int row);
                //     if (!isValidRow) continue;
                //     if (rowMin <= row && row <= rowMax)
                //     {
                //         int colObjCount = zoomLevelTrans.childCount;
                //         for (int childColumnId = 0; childColumnId < colObjCount; childColumnId++)
                //         {
                //             var colTrans = rowTrans.GetChild(childColumnId);
                //             var colStr = colTrans.name;
                //             bool isValidCol = int.TryParse(colStr, out int col);
                //             if (!isValidCol) continue;
                //             if (colMin <= col && col <= colMax)
                //             {
                //                 colTrans.gameObject.SetActive(false);
                //             }
                //         }
                //     }
                // }
                // for (int row = rowMin; row <= rowMax; row++)
                // {
                //     var rowTrans = zoomLevelTrans.Find(row.ToString());
                //     if (rowTrans == null) continue;
                //     for (int col = colMin; col <= colMax; col++)
                //     {
                //         var colTrans = rowTrans.Find(col.ToString());
                //         if (colTrans == null) continue;
                //         colTrans.gameObject.SetActive(false);
                //         Debug.Log("disabled");
                //     }
                // }
            }
        }


    }
}
