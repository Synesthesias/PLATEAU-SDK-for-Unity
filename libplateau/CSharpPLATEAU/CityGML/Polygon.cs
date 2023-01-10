using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// 建築物の形状におけるポリゴンです。
    /// Vertices , Indices を保持します。
    /// ただし、GMLファイルのパース時に <see cref="CitygmlParserParams.Tessellate"/> を false に設定した時に限り、
    /// Vertices, Indices の代わりに <see cref="ExteriorRing"/> , <see cref="InteriorRings"/> を保持することがあります。
    /// Vertex と Ring を両方保持する場合もあれば、片方だけの場合もあります。
    /// <see cref="Polygon"/> は <see cref="Geometry"/> によって保持されます。
    /// </summary>
    public class Polygon : AppearanceTarget
    {
        private LinearRing cachedExteriorRing;
        private LinearRing[] cachedInteriorRings; // キャッシュの初期状態は null とするので null許容型にします。
        
        internal Polygon(IntPtr handle) : base(handle)
        {
        }

        /// <summary> ポリゴンとしての頂点を持っているか </summary>
        public bool DoHaveVertices => VertexCount > 0;
        /// <summary> 多角形の形状情報としての <see cref="LinearRing"/>(Exterior または Interior)を持っているか </summary>
        public bool DoHaveRings => ExteriorRing.VertexCount > 0 || InteriorRingCount > 0;

        /// <summary>
        /// 頂点数を返します。
        /// </summary>
        public int VertexCount
        {
            get
            {
                int vertCount = DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_polygon_get_vertex_count);
                return vertCount;
            }
        }

        /// <summary>
        /// 頂点番号を受け取り、その頂点の座標を返します。
        /// </summary>
        public PlateauVector3d GetVertex(int index)
        {
            if (index >= VertexCount)
            {
                throw new ArgumentOutOfRangeException($"{nameof(index)}",
                    $"index is out of range.  index={index}, VertexCount={VertexCount}");
            }
            var vert = DLLUtil.GetNativeValue<PlateauVector3d>(Handle, index,
                NativeMethods.plateau_polygon_get_vertex);
            return vert;
        }

        public int IndicesCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_polygon_get_indices_count);
                return count;
            }
        }

        private int GetIndexOfIndices(int indexOfIndicesList)
        {
            int ret = DLLUtil.GetNativeValue<int>(Handle, indexOfIndicesList,
                NativeMethods.plateau_polygon_get_index_of_indices);
            return ret;
        }

        public IEnumerable<int> Indices
        {
            get
            {
                int cnt = IndicesCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetIndexOfIndices(i);
                }
            }
        }

        /// <summary>
        /// 建物の外周の形状です。
        /// GMLファイルのパース時の設定で <see cref="CitygmlParserParams.Tessellate"/> が true の場合、この情報は保持されません。
        /// false の場合、 <see cref="ExteriorRing"/>, <see cref="InteriorRings"/>が保持される場合があります。
        /// </summary>
        public LinearRing ExteriorRing
        {
            get
            {
                if (this.cachedExteriorRing != null) return this.cachedExteriorRing;
                IntPtr ringHandle = DLLUtil.GetNativeValue<IntPtr>(Handle,
                    NativeMethods.plateau_polygon_get_exterior_ring);
                this.cachedExteriorRing = new LinearRing(ringHandle);
                return this.cachedExteriorRing;
            }
        }

        /// <summary>
        /// 建物の形状の多角形表現のうち、内側にある多角形の数です。
        /// </summary>
        public int InteriorRingCount =>
            DLLUtil.GetNativeValue<int>(Handle,
                NativeMethods.plateau_polygon_get_interior_ring_count);

        /// <summary>
        /// 建物の形状の多角形表現のうち、内側にある多角形の1つをインデックス指定で返します。
        /// </summary>
        private LinearRing GetInteriorRing(int index)
        {
            var ring = DLLUtil.ArrayCache(ref this.cachedInteriorRings, index, InteriorRingCount, () =>
            {
                IntPtr ringHandle = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                    NativeMethods.plateau_polygon_get_interior_ring);
                return new LinearRing(ringHandle);
            });
            return ring;
        }

        /// <summary>
        /// InteriorRing を foreach や Linq で回したい時に利用できます。
        /// </summary>
        public IEnumerable<LinearRing> InteriorRings
        {
            get
            {
                int cnt = InteriorRingCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetInteriorRing(i);
                }
            }
        }

        public override string ToString()
        {
            return $"Polygon : ID={ID}, VertexCount={VertexCount}";
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_vertex_count(
                [In] IntPtr polygonHandle,
                out int outVertCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_vertex(
                [In] IntPtr handle,
                out PlateauVector3d outVertex,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_indices_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_index_of_indices(
                [In] IntPtr handle,
                out int outIndex,
                int indexOfIndicesList);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_exterior_ring(
                [In] IntPtr handle,
                out IntPtr ringHandle);
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_interior_ring_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_polygon_get_interior_ring(
                [In] IntPtr handle,
                out IntPtr outRingHandle,
                int index);
        }
    }
}
