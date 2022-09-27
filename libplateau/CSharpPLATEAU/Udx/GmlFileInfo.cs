using System;
using PLATEAU.Interop;

namespace PLATEAU.Udx
{
    public class GmlFileInfo : PInvokeDisposable
    {
        public GmlFileInfo(IntPtr handle) : base(handle)
        {
            
        }

        public static GmlFileInfo Create(string path)
        {
            var apiResult = NativeMethods.plateau_create_gml_file_info(out IntPtr outPtr, path);
            DLLUtil.CheckDllError(apiResult);
            return new GmlFileInfo(outPtr);
        }

        public string Path => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_info_get_path);

        protected override void DisposeNative()
        {
            NativeMethods.plateau_delete_gml_file_info(Handle);
        }
    }
}
