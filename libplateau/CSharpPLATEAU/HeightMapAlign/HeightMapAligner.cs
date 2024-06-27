using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.HeightMapAlign
{
    public class HeightMapAligner : PInvokeDisposable
    {
        public static HeightMapAligner Create(double heightOffset)
        {
            var result = NativeMethods.height_map_aligner_create(out var createdPtr, heightOffset);
            DLLUtil.CheckDllError(result);
            return new HeightMapAligner(createdPtr);
        }
        
        public HeightMapAligner(IntPtr handle) : base(handle)
        {
        }

        public void AddHeightmapFrame(UInt16[] heightMapData, int heightMapWidth, int heightMapHeight, float minX, float maxX, float minY, float maxY, float minHeight, float maxHeight)
        {
            if (heightMapData.Length != heightMapWidth * heightMapHeight)
            {
                throw new ArgumentException($"{nameof(heightMapData)}のサイズとWidth,Heightの積が一致しません。");
            }
            var apiResult = NativeMethods.height_map_aligner_add_heightmap_frame(Handle, heightMapData, heightMapData.Length, heightMapWidth, heightMapHeight, minX, maxX, minY, maxY, minHeight, maxHeight);
            DLLUtil.CheckDllError(apiResult);
        }
        
        public void Align(Model model)
        {
            
            var apiResult = NativeMethods.height_map_aligner_align(Handle, model.Handle);
            DLLUtil.CheckDllError(apiResult);
        }
        
        

        protected override void DisposeNative()
        {
            NativeMethods.height_map_aligner_destroy(Handle);
        }
        
        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_create(
                out IntPtr createdPtr,
                [In] double heightOffset
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_destroy(
                IntPtr createdPtr
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_add_heightmap_frame(
                [In] IntPtr alignerPtr,
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
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_align(
                [In] IntPtr alignerPtr,
                [In, Out] IntPtr modelPtr
            );
        }

        
    }
}
