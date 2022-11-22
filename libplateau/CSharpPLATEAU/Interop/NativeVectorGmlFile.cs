﻿using System;
using PLATEAU.Udx;

namespace PLATEAU.Interop
{
    public class NativeVectorGmlFile : PInvokeDisposable
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

        public GmlFile At(int index)
        {
            ThrowIfDisposed();
            var gmlFilePtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_gml_file_get_pointer);
            return new GmlFile(gmlFilePtr);
        }

        public int Length
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
    }
}
