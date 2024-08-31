using System;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.HeightMapAlign
{
    public class HeightMapAligner : PInvokeDisposable
    {
        private const float MaxEdgeLength = 4; // 高さ合わせのためメッシュを細分化するときの、最大の辺の長さ。だいたい4mくらいが良さそう。
        private const int AlphaExpandWidthCartesian = 2; // 逆高さ合わせのアルファの平滑化処理において、不透明部分を広げる幅（直交座標系）
        private const int AlphaAverageWidthCartesian = 2; // 逆高さ合わせのアルファの平滑化処理において、周りの平均を取る幅（直交座標系）
        private const float AlignInvertHeightOffset = -0.2f; // 逆高さ合わせで、土地を対象と比べてどの高さに合わせるか（直交座標系）
        private const float SkipThresholdOfMapLandDistance = 0.8f; // 逆高さ合わせで、土地との距離がこの値以上の箇所は高さ合わせしない（直交座標系）
        
        public static HeightMapAligner Create(double heightOffset, CoordinateSystem axis)
        {
            var result = NativeMethods.height_map_aligner_create(out var createdPtr, heightOffset, axis);
            DLLUtil.CheckDllError(result);
            return new HeightMapAligner(createdPtr);
        }
        
        public HeightMapAligner(IntPtr handle) : base(handle)
        {
        }

        public void AddHeightmapFrame(UInt16[] heightMapData, int heightMapWidth, int heightMapHeight, float minX, float maxX, float minY, float maxY, float minZ, float maxZ, CoordinateSystem axis)
        {
            if (heightMapData.Length != heightMapWidth * heightMapHeight)
            {
                throw new ArgumentException($"{nameof(heightMapData)}のサイズとWidth,Heightの積が一致しません。");
            }
            var apiResult = NativeMethods.height_map_aligner_add_heightmap_frame(Handle, heightMapData, heightMapData.Length, heightMapWidth, heightMapHeight, minX, maxX, minY, maxY, minZ, maxZ, axis);
            DLLUtil.CheckDllError(apiResult);
        }
        
        public void Align(Model model)
        {
            
            var apiResult = NativeMethods.height_map_aligner_align(Handle, model.Handle, MaxEdgeLength);
            DLLUtil.CheckDllError(apiResult);
        }

        public void AlignInvert(Model model)
        {
            var apiResult = NativeMethods.height_map_aligner_align_invert(Handle, model.Handle, AlphaExpandWidthCartesian, AlphaAverageWidthCartesian, AlignInvertHeightOffset, SkipThresholdOfMapLandDistance);
            DLLUtil.CheckDllError(apiResult);
        }

        public int HeightMapCount
        {
            get
            {
                var apiResult = NativeMethods.height_map_aligner_height_map_count(Handle, out int count);
                DLLUtil.CheckDllError(apiResult);
                return count;
            }
        }

        public UInt16[] GetHeightMapAt(int index)
        {
            var apiResult = NativeMethods.height_map_aligner_get_height_map_at(
                Handle, index, out IntPtr heightMapPtr, out int dataSize
            );
            DLLUtil.CheckDllError(apiResult);
            byte[] outData = DLLUtil.PtrToBytes(heightMapPtr, sizeof(UInt16) * dataSize);
            NativeMethods.release_heightmap_data(heightMapPtr);
            var copiedHeightMap = new UInt16[dataSize];
            int byteIndex = 0;
            for (int i = 0; i < dataSize; i++)
            {
                copiedHeightMap[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += sizeof(UInt16);
            }

            return copiedHeightMap;
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
                [In] double heightOffset,
                [In] CoordinateSystem axis
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
                [In] float minZ,
                [In] float maxZ,
                [In] CoordinateSystem axis
            );
            
                [DllImport(DLLUtil.DllName)]
                internal static extern APIResult height_map_aligner_align(
                    [In] IntPtr alignerPtr,
                    [In, Out] IntPtr modelPtr,
                    [In] float maxEdgeLength
                );
                
                [DllImport(DLLUtil.DllName)]
                internal static extern APIResult height_map_aligner_align_invert(
                    [In] IntPtr alignerPtr,
                    [In, Out] IntPtr modelPtr,
                    [In] int alphaExpandWidthCartesian,
                    [In] int alphaAverageWidthCartesian,
                    [In] float heightOffset,
                    [In] float skipThresholdOfMapLandDistance
                );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_height_map_count(
                [In] IntPtr alignerPtr,
                [Out] out int count
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult height_map_aligner_get_height_map_at(
                [In] IntPtr alignerPtr,
                [In] int index,
                [Out] out IntPtr heightMapPtr,
                [Out] out int dataSize
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult release_heightmap_data(
                [In] IntPtr data
            );
        }

        
    }
}
