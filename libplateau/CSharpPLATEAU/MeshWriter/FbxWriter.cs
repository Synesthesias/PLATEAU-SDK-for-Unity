using PLATEAU.Interop;
using PLATEAU.PolygonMesh;

namespace PLATEAU.MeshWriter
{
    public static class FbxWriter
    {
        public static bool Write(string fbxPath, Model model, FbxWriteOptions options)
        {
            var fbxPathUtf8 = DLLUtil.StrToUtf8Bytes(fbxPath);
            var result = NativeMethods.plateau_fbx_writer_write(fbxPathUtf8, model.Handle, options, out bool isSucceed);
            DLLUtil.CheckDllError(result);
            return isSucceed;
        }
    }
}
