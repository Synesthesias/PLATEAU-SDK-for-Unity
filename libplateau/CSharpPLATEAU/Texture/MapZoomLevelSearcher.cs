using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.Texture
{
    public class MapZoomLevelSearcher
    {
        public MapZoomLevelSearchResult Search(string urlTemplate, double latitude, double longitude)
        {
            var apiResult =
                NativeMethods.plateau_map_zoom_level_searcher_search(out var result, urlTemplate, latitude, longitude);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }
        
        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_map_zoom_level_searcher_search(
                [Out] out MapZoomLevelSearchResult result,
                [In] string urlTemplate,
                [In] double latitude,
                [In] double longitude
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MapZoomLevelSearchResult
    {
        [MarshalAs(UnmanagedType.U1)] public bool IsValid;
        public int AvailableZoomLevelMin;
        public int AvailableZoomLevelMax;
    }
}
