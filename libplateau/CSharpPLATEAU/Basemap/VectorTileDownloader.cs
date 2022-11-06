using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Util;

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

        public static VectorTileDownloader Create(string destinationPath, Extent extent)
        {
            var result = NativeMethods.plateau_create_vector_tile_downloader(
                out var pointer, destinationPath, extent
            );
            DLLUtil.CheckDllError(result);
            return new VectorTileDownloader(pointer);
        }

        public int TileCount => DLLUtil.GetNativeValue<int>(Handle,
            NativeMethods.plateau_vector_tile_downloader_get_tile_count);

        /// <summary>
        /// 地理院地図タイルをダウンロードして pngファイルに保存します。
        /// </summary>
        public void Download(int index, out TileCoordinate tileCoordinate, out string imagePath)
        {
            var result = NativeMethods.plateau_vector_tile_downloader_download(
                Handle, index, out tileCoordinate, out int sizeOfImagePath
            );
            DLLUtil.CheckDllError(result);
            IntPtr pathStrPtr = Marshal.AllocCoTaskMem(sizeOfImagePath);
            var result2 = NativeMethods.plateau_vector_tile_downloader_last_image_path(pathStrPtr);
            DLLUtil.CheckDllError(result2);
            string pathStr = DLLUtil.ReadUtf8Str(pathStrPtr, sizeOfImagePath);
            Marshal.FreeCoTaskMem(pathStrPtr);
            imagePath = pathStr;
        }

        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_vector_tile_downloader(Handle);
            DLLUtil.CheckDllError(result);
        }
    }
}
