using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private Task mapLoadTask = null;
        
        

        public GSIMapLoaderZoomSwitch(GeoReference geoReference)
        {
            this.geoReference = geoReference;
            this.framesTillMapUpdate = mapUpdateInterval;
        }

        public void Update(Camera cam)
        {
            if (--this.framesTillMapUpdate <= 0)
            {
                if (!(this.mapLoadTask is { Status: TaskStatus.Running }))
                {
                    this.framesTillMapUpdate = mapUpdateInterval;
                    var extent = CalcCameraExtent(cam, this.geoReference);
                    this.mapLoadTask = GSIMapLoader
                        .DownloadAndPlaceAsync(extent, this.geoReference, 10, this.cancel.Token)
                        .ContinueWithErrorCatch();
                }
            }
        }

        private static Extent CalcCameraExtent(Camera cam, GeoReference geoRef)
        {
            var camTrans = cam.transform;
            float distance = camTrans.position.y;
            var lowerLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, distance));
            var upperRight = cam.ViewportToWorldPoint(new Vector3(1, 1, distance));
            var camExtent = ToExtent(lowerLeft, upperRight, geoRef);
            // Debug.Log($"lowerLeft={lowerLeft}, upperRight={upperRight}, cameraPos={camTrans.position}, camExtent={camExtent}");
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


    }
}
