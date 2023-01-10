using PLATEAU.Interop;
using PLATEAU.PolygonMesh;
using System;
using System.Runtime.InteropServices;

namespace PLATEAU.MeshWriter
{
    public struct GltfWriteOptions
    {
        public GltfFileFormat GltfFileFormat;
        public string TextureDirectoryPath;

        public GltfWriteOptions(GltfFileFormat format, string path)
        {
            this.GltfFileFormat = format;
            this.TextureDirectoryPath = path;
        }
    }
    
    public enum GltfFileFormat
    {
        GLB,
        GLTF
    }

    public class GltfWriter : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public bool Write(string destination, Model model, GltfWriteOptions options)
        {
            string texturePath = options.TextureDirectoryPath;
            GltfFileFormat format = options.GltfFileFormat;
            var result = NativeMethods.plateau_gltf_writer_write(
                this.handle, out var flg, DLLUtil.StrToUtf8Bytes(destination), model.Handle,
                DLLUtil.StrToUtf8Bytes(texturePath), format);
            DLLUtil.CheckDllError(result);
            return flg;
        }

        public GltfWriter()
        {
            APIResult result = NativeMethods.plateau_create_gltf_writer(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outPtr;
        }

        ~GltfWriter()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.isDisposed) return;
            DLLUtil.ExecNativeVoidFunc(this.handle, NativeMethods.plateau_delete_gltf_writer);
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_gltf_writer(out IntPtr outHandle);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_gltf_writer([In] IntPtr gltfWriter);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_gltf_writer_write(
                [In] IntPtr handle,
                out bool flg,
                [In] byte[] gltfFilePathUtf8,
                [In] IntPtr modelPtr,
                [In] byte[] texPathUtf8,
                GltfFileFormat format);
        }
    }
}
