using System;
using PLATEAU.Dataset;

namespace PLATEAU.Interop
{
    public class NativeVectorMeshCode : NativeVectorDisposableBase<MeshCode>
    {
        private NativeVectorMeshCode(IntPtr ptr) : base(ptr)
        {
        }

        public static NativeVectorMeshCode Create()
        {
            var result = NativeMethods.plateau_create_vector_mesh_code(out var ptr);
            DLLUtil.CheckDllError(result);
            return new NativeVectorMeshCode(ptr);
        }

        public override MeshCode At(int index)
        {
            ThrowIfDisposed();
            var meshCode = DLLUtil.GetNativeValue<MeshCode>(Handle, index,
                NativeMethods.plateau_vector_mesh_code_get_value);
            return meshCode;
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_mesh_code_count);
                return count;
            }
        }


        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_vector_mesh_code(Handle);
            DLLUtil.CheckDllError(result);
        }
    }
}
