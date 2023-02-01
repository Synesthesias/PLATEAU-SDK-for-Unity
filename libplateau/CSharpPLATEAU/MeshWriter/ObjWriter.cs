using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.PolygonMesh;

namespace PLATEAU.MeshWriter
{
    public class ObjWriter : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public bool Write(string destination, Model model)
        {
            var result = NativeMethods.plateau_obj_writer_write(
                this.handle, out bool flg, DLLUtil.StrToUtf8Bytes(destination), model.Handle);
            DLLUtil.CheckDllError(result);
            return flg;
        }

        public ObjWriter()
        {
            APIResult result = NativeMethods.plateau_create_obj_writer(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outPtr;
        }

        ~ObjWriter()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.isDisposed) return;
            DLLUtil.ExecNativeVoidFunc(this.handle, NativeMethods.plateau_delete_obj_writer);
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_obj_writer(out IntPtr outHandle);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_obj_writer([In] IntPtr objWriter);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_obj_writer_write(
                [In] IntPtr handle,
                out bool flg,
                [In] byte[] objFilePathUtf8,
                [In] IntPtr modelPtr);
        }
    }
}
