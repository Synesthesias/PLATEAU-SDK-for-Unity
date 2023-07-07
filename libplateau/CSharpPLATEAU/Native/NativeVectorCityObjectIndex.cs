using PLATEAU.Interop;
using PLATEAU.PolygonMesh;
using System;
using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    /// <summary>
    /// C++側の vector{int} を扱います。
    /// </summary>
    internal class NativeVectorCityObjectIndex : NativeVectorDisposableBase<CityObjectIndex>
    {
        private NativeVectorCityObjectIndex(IntPtr handle) : base(handle)
        {
        }

        public static NativeVectorCityObjectIndex Create()
        {
            return new NativeVectorCityObjectIndex(
                DLLUtil.PtrOfNewInstance(
                    NativeMethods.plateau_create_vector_city_object_index
                )
            );
        }

        protected override void DisposeNative()
        {
            ThrowIfDisposed();
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_vector_city_object_index);
        }

        public override CityObjectIndex At(int index)
        {
            ThrowIfDisposed();
            return DLLUtil.GetNativeValue<CityObjectIndex>(Handle, index,
                NativeMethods.plateau_vector_city_object_index_get_value);
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_city_object_index_count);
                return count;
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_city_object_index(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_city_object_index(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_city_object_index_get_value(
                [In] IntPtr vectorPtr,
                out CityObjectIndex outIndex,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_city_object_index_count(
                [In] IntPtr handle,
                out int outCount);
        }
    }
}
