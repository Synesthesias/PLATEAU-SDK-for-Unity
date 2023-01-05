using System;
using System.Runtime.InteropServices;
using PLATEAU.Dataset;
using PLATEAU.Interop;

namespace PLATEAU.Native
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

        public void Add(MeshCode meshCode)
        {
            var result = NativeMethods.plateau_vector_mesh_code_push_back_value(
                Handle, meshCode);
            DLLUtil.CheckDllError(result);
        }


        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_vector_mesh_code(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_mesh_code(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_mesh_code(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_mesh_code_get_value(
                [In] IntPtr vectorPtr,
                out MeshCode outMeshCode,
                int index);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_mesh_code_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_mesh_code_push_back_value(
                [In] IntPtr handle,
                [In] MeshCode meshCode);
        }
    }
}
