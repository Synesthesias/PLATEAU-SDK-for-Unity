using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.PolygonMesh;

namespace PLATEAU.HeightMapAlign
{
    public class HeightMapAligner
    {
        public void Align(Model model, UInt16[] heightMapData, int heightMapWidth, int heightMapHeight, float minX, float maxX, float minY, float maxY, float minHeight, float maxHeight)
        {
            if (heightMapData.Length != heightMapWidth * heightMapHeight)
            {
                throw new ArgumentException($"{nameof(heightMapData)}のサイズとWidth,Heightの積が一致しません。");
            }
            var apiResult = NativeMethods.height_map_aligner_align(model.Handle, heightMapData, heightMapData.Length, heightMapWidth, heightMapHeight, minX, maxX, minY, maxY, minHeight, maxHeight);
            DLLUtil.CheckDllError(apiResult);
        }
        
        
        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_align(
                [In, Out] IntPtr modelPtr,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt16[] heightMapData,
                [In] int heightMapSize, // マーシャリングの都合上sizeを明示しますが、その値は width * height としてください。
                [In] int heightMapWidth,
                [In] int heightMapHeight,
                [In] float minX,
                [In] float maxX,
                [In] float minY,
                [In] float maxY,
                [In] float minHeight,
                [In] float maxHeight
            );
        }
    }
}
