using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Basemap
{
    /// <summary>
    /// 緯度経度と、地理院地図タイル番号を変換します。
    /// </summary>
    public static class TileProjection
    {
        /// <summary>
        /// 緯度経度から地理院地図タイル番号に変換します。
        /// </summary>
        public static TileCoordinate Project(GeoCoordinate geoCoord, int zoomLevel = 15)
        {
            var result = NativeMethods.plateau_tile_projection_project(
                geoCoord, zoomLevel, out var tileCoord);
            DLLUtil.CheckDllError(result);
            return tileCoord;
        }

        /// <summary>
        /// 地理院地図タイル番号から緯度経度範囲に変換します。
        /// </summary>
        public static Extent Unproject(TileCoordinate tileCoord)
        {
            var result = NativeMethods.plateau_tile_projection_unproject(
                tileCoord, out var extent
            );
            DLLUtil.CheckDllError(result);
            return extent;
        }

        private static class NativeMethods
        {
            // ***************
            //  vector_tile_downloader_c.cpp
            // ***************
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_tile_projection_project(
                [In] GeoCoordinate geoCoordinate,
                int zoomLevel,
                out TileCoordinate outTileCoordinate);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_tile_projection_unproject(
                [In] TileCoordinate tileCoordinate,
                out Extent outExtent);
        }
    }
}
