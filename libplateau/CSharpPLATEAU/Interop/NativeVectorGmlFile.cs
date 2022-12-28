﻿using System;
using System.Runtime.InteropServices;
using PLATEAU.Dataset;

namespace PLATEAU.Interop
{
    public class NativeVectorGmlFile : NativeVectorDisposableBase<GmlFile>
    {
        private NativeVectorGmlFile(IntPtr ptr) : base(ptr)
        {
        }

        public static NativeVectorGmlFile Create()
        {
            var result = NativeMethods.plateau_create_vector_gml_file(out var ptr);
            DLLUtil.CheckDllError(result);
            return new NativeVectorGmlFile(ptr);
        }

        public override GmlFile At(int index)
        {
            ThrowIfDisposed();
            var gmlFilePtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_gml_file_get_pointer);
            return new GmlFile(gmlFilePtr);
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_gml_file_count);
                return count;
            }
        }


        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_vector_gml_file(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_gml_file(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_gml_file(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_gml_file_get_pointer(
                [In] IntPtr vectorPtr,
                out IntPtr outGmlFilePtr,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_gml_file_count(
                [In] IntPtr handle,
                out int outCount);
        }
    }
}
