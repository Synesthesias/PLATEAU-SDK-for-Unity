using System;
using PLATEAU.Interop;

namespace PLATEAU.Basemap
{
    /// <summary>
    /// 地理院地図の地図タイルをダウンロードします。
    /// </summary>
    public class VectorTileDownloader : PInvokeDisposable
    {
        public VectorTileDownloader(IntPtr handle) : base(handle)
        {
        }

        public static VectorTileDownloader Create(string destinationPath, Extent extent, int zoomLevel)
        {
            var result = NativeMethods.plateau_create_vector_tile_downloader(
                out var pointer, destinationPath, extent, zoomLevel
            );
            DLLUtil.CheckDllError(result);
            return new VectorTileDownloader(pointer);
        }

        public int TileCount => DLLUtil.GetNativeValue<int>(Handle,
            NativeMethods.plateau_vector_tile_downloader_get_tile_count);

        /// <summary>
        /// 地理院地図タイルをダウンロードして pngファイルに保存します。
        /// </summary>
        public void Download(int index)
        {
            var result = NativeMethods.plateau_vector_tile_downloader_download(
                Handle, index
            );
            DLLUtil.CheckDllError(result);
        }

        public string CalcDestPath(int index)
        {
            string path = DLLUtil.GetNativeStringByValue(Handle,
                (IntPtr handle, out int outStrSize) =>
                    NativeMethods.plateau_vector_tile_downloader_calc_destination_path_size(handle, out outStrSize,
                        index),
                (handle, strPtr) =>
                    NativeMethods.plateau_vector_tile_downloader_calc_destination_path(handle, strPtr, index)
            );
            return path;
        }

        public TileCoordinate GetTileCoordinate(int index)
        {
            return DLLUtil.GetNativeValue<TileCoordinate>(Handle, index,
                NativeMethods.plateau_vector_tile_downloader_get_tile);
        }

        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_vector_tile_downloader(Handle);
            DLLUtil.CheckDllError(result);
        }
    }
}
