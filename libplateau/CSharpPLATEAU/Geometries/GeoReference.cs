using System;
using PLATEAU.Interop;
using PLATEAU.IO;

namespace PLATEAU.Geometries
{
    /// <summary>
    /// 極座標と平面直角座標を変換します。
    /// また座標変換の基準を保持します。
    /// </summary>
    public class GeoReference : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public IntPtr Handle => this.handle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referencePoint">
        /// 平面直角座標に変換したあと、この座標が原点となるように並行移動します。
        /// </param>
        /// <param name="unitScale">
        /// 平面直角座標に変換したあと、拡大縮小します。
        /// </param>
        /// <param name="coordinateSystem">
        /// 平面直角座標のX,Y,Z軸の向きを決めます。
        /// </param>
        /// <param name="zoneID">
        /// 国土交通省告示第九号に基づく平面直角座標系の原点の番号です。
        /// 関東地方では 9 を選択すると歪みが少なくなりますが、
        /// この値を間違えても、ぱっと見ですぐ分かるような歪みにはなりません。
        /// 詳しくはこちらを参照してください :
        /// https://www.gsi.go.jp/sokuchikijun/jpc.html
        /// </param>
        public GeoReference(PlateauVector3d referencePoint, float unitScale, CoordinateSystem coordinateSystem,
            int zoneID)
        {
            var result = NativeMethods.plateau_create_geo_reference(
                out var geoReferencePtr, referencePoint, unitScale,
                coordinateSystem, zoneID
            );
            DLLUtil.CheckDllError(result);
            this.handle = geoReferencePtr;
        }

        public PlateauVector3d Project(GeoCoordinate geoCoordinate)
        {
            var result = NativeMethods.plateau_geo_reference_project(
                this.handle, out var outXyz,
                geoCoordinate);
            DLLUtil.CheckDllError(result);
            return outXyz;
        }

        public GeoCoordinate Unproject(PlateauVector3d point)
        {
            var result = NativeMethods.plateau_geo_reference_unproject(
                this.handle, out var outLatlon,
                point);
            DLLUtil.CheckDllError(result);
            return outLatlon;
        }

        public void Dispose()
        {
            if (this.isDisposed) return;
            var result = NativeMethods.plateau_delete_geo_reference(this.handle);
            DLLUtil.CheckDllError(result);
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }

        ~GeoReference()
        {
            Dispose();
        }
    }
}
