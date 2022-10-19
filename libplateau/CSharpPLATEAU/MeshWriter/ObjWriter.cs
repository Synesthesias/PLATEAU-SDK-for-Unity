using System;
using PLATEAU.Interop;
using System.Threading;
using PLATEAU.PolygonMesh;

namespace PLATEAU.MeshWriter
{
    public class ObjWriter : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public bool Write(string destination, Model model)
        {
            var result = NativeMethods.plateau_obj_writer_write(this.handle, out var flg, destination, model.Handle);
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
            var result = NativeMethods.plateau_delete_obj_writer(this.handle);
            DLLUtil.CheckDllError(result);
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }
    }
}
