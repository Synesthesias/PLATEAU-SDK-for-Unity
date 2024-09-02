using System;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.Heightmap
{
    /// <summary>
    /// ハイトマップから<see cref="PolygonMesh.Mesh"/>を生成します。
    /// </summary>
    public class HeightmapMeshGenerator
    {
        /// <summary>
        /// ハイトマップから<see cref="PolygonMesh.Mesh"/>を生成します。
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="zScale"></param>
        /// <param name="heightmapData"></param>
        /// <param name="coordinate"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="minUV"></param>
        /// <param name="maxUV"></param>
        /// <param name="invertMesh"></param>
        public void Generate(
            ref Mesh mesh, int textureWidth, int textureHeight, float zScale,
            UInt16[] heightmapData, CoordinateSystem coordinate,
            PlateauVector3d min, PlateauVector3d max, PlateauVector2f minUV, PlateauVector2f maxUV,
            bool invertMesh)
        {
            var result = NativeMethods.plateau_heightmap_mesh_generator_generate(
                textureWidth,
                textureHeight,
                zScale,
                heightmapData,
                coordinate,
                min,
                max,
                minUV.X,
                minUV.Y,
                maxUV.X,
                maxUV.Y,
                invertMesh,
                mesh.Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_heightmap_mesh_generator_generate(
                [In] int textureWidth,
                [In] int textureHeight,
                [In] float zScale,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt16[] heightmapData,
                [In] CoordinateSystem coordinate,
                [In] PlateauVector3d min,
                [In] PlateauVector3d max,
                [In] float minUVX,
                [In] float minUVY,
                [In] float maxUVX,
                [In] float maxUVY,
                [In][MarshalAs(UnmanagedType.I1)] bool invertMesh,
                [In, Out] IntPtr outMeshHandle);
        }
    }
}
