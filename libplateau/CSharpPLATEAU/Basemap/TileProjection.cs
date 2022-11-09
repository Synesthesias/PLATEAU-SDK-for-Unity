using PLATEAU.Interop;

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
    }
}
