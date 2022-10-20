using PLATEAU.Interop;
using PLATEAU.PolygonMesh;
using System;

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

    public class GltfWriter : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public bool Write(string destination, Model model, GltfWriteOptions options)
        {
            string texturePath = options.TextureDirectoryPath;
            GltfFileFormat format = options.GltfFileFormat;
            var result = NativeMethods.plateau_gltf_writer_write(this.handle, out var flg, destination, model.Handle, texturePath, format);
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
            var result = NativeMethods.plateau_delete_gltf_writer(this.handle);
            DLLUtil.CheckDllError(result);
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }
    }
}
