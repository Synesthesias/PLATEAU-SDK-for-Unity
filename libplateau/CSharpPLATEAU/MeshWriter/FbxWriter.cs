using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.PolygonMesh;

namespace PLATEAU.MeshWriter
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FbxWriteOptions
    {
        public FbxFileFormat FileFormat;

        public FbxWriteOptions(FbxFileFormat fileFormat)
        {
            this.FileFormat = fileFormat;
        }
    }
    
    public enum FbxFileFormat : UInt32
    {
        Binary, Ascii
    }
    
    public static class FbxWriter
    {
        public static bool Write(string fbxPath, Model model, FbxWriteOptions options)
        {
            var fbxPathUtf8 = DLLUtil.StrToUtf8Bytes(fbxPath);
            var result = NativeMethods.plateau_fbx_writer_write(fbxPathUtf8, model.Handle, options, out bool isSucceed);
            DLLUtil.CheckDllError(result);
            return isSucceed;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_fbx_writer_write(
                [In] byte[] fbxFilePathUtf8,
                [In] IntPtr modelPtr,
                FbxWriteOptions options,
                out bool outIsSucceed);
        }
    }
}
