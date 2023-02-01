using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// 多角形による形状表現です。
    /// 頂点座標のリストを保持します。
    /// <see cref="Polygon"/> が <see cref="LinearRing"/> を保持します。（ただし条件付きで一部のみです。）
    /// </summary>
    public class LinearRing : Object
    {
        internal LinearRing(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// 頂点数を取得します。
        /// 注意:
        /// GMLファイルのパース時の設定が tessellate = false のときに限り
        /// 意味のある値を返します。
        /// tessellate = true でパースした場合、この値は 0 になります。
        /// </summary>
        public int VertexCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_linear_ring_get_vertex_count);
                return count;
            }
        }

        /// <summary>
        /// <paramref name="index"/> 番目の頂点座標を取得します。
        /// </summary>
        public PlateauVector3d GetVertex(int index)
        {
            if (index >= VertexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Ring has {VertexCount} vertices, but you tried to read index #{index} that is out of range."
                );
            }
            var vert3d = DLLUtil.GetNativeValue<PlateauVector3d>(Handle, index,
                NativeMethods.plateau_linear_ring_get_vertex);
            return vert3d;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_linear_ring_get_vertex_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_linear_ring_get_vertex(
                [In] IntPtr handle,
                out PlateauVector3d outVert3d,
                int index);
            
        }
    }
}
