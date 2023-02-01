using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    public enum GeometryType : ulong
    {
        GT_Unknown = 1ul << 0,
        GT_Roof = 1ul << 1,
        GT_Wall = 1ul << 2,
        GT_Ground = 1ul << 3,
        GT_Closure = 1ul << 4,
        GT_Floor = 1ul << 5,
        GT_InteriorWall = 1ul << 6,
        GT_Ceiling = 1ul << 7,
        GT_OuterCeiling = 1ul << 8,
        GT_OuterFloor = 1ul << 9,
        GT_Tin = 1ul << 10,
    }

    /// <summary>
    /// 建築物の形状と見た目の情報を保持します。
    /// <see cref="CityObject"/> が<see cref="Geometry"/>を保持します。
    /// <see cref="Geometry"/> は <see cref="Polygon"/> , 子<see cref="Geometry"/> を保持します。
    /// </summary>
    public class Geometry : AppearanceTarget
    {
        private Geometry[] cachedChildGeometries; // キャッシュの初期状態は null とするので null許容型にします。
        private Polygon[] cachedPolygons;
        public Geometry(IntPtr handle) : base(handle)
        {
        }

        public GeometryType Type => DLLUtil.GetNativeValue<GeometryType>(Handle, NativeMethods.plateau_geometry_get_type);

        /// <summary> 子の <see cref="Geometry"/> の数を返します。 </summary>
        public int ChildGeometryCount => DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_geometry_get_geometries_count);
        
        /// <summary> インデックス指定で子の <see cref="Geometry"/> を1つ返します。 </summary>
        public Geometry GetChildGeometry(int index)
        {
            var geom = DLLUtil.ArrayCache(ref this.cachedChildGeometries, index, ChildGeometryCount, () =>
            {
                IntPtr childHandle = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                    NativeMethods.plateau_geometry_get_child_geometry);
                return new Geometry(childHandle);
            });
            return geom;
        }

        /// <summary> 子の <see cref="Geometry"/> をforeachやLinqで回したい時に利用できます。 </summary>
        IEnumerable<Geometry> ChildGeometries
        {
            get
            {
                int cnt = ChildGeometryCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetChildGeometry(i);
                }
            }
        }

        /// <summary>
        /// 子孫の <see cref="Geometry"/> を再帰ですべてイテレートします。自分自身を含みます。
        /// イテレートの順番は DFS(深さ優先探索)です。
        /// </summary>
        public IEnumerable<Geometry> GeometryDescendantsDFS
        {
            get
            {
                var results = this.ChildGeometriesDfsRecursive(this);
                foreach (var r in results)
                {
                    yield return r;
                }
            }
        }

        private IEnumerable<Geometry> ChildGeometriesDfsRecursive(Geometry geom)
        {
            yield return geom;
            foreach (var child in geom.ChildGeometries)
            {
                var results = ChildGeometriesDfsRecursive(child);
                foreach (var r in results)
                {
                    yield return r;
                }
            }
        }

        /// <summary> <see cref="Polygon"/> の数を返します。 </summary>
        public int PolygonCount => DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_geometry_get_polygons_count);
        
        /// <summary> インデックス指定で <see cref="Polygon"/> を1つ返します。 </summary>
        public Polygon GetPolygon(int index)
        {
            var poly = DLLUtil.ArrayCache(ref this.cachedPolygons, index, PolygonCount, () =>
            {
                var polyHandle = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                    NativeMethods.plateau_geometry_get_polygon);
                return new Polygon(polyHandle);
            });
            return poly;
        }
        
        /// <summary>
        /// <see cref="Polygon"/> をforeachやLinqで回したい時に利用できます。
        /// </summary>
        public IEnumerable<Polygon> Polygons
        {
            get
            {
                int cnt = PolygonCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetPolygon(i);
                }
            }
        }

        /// <summary>
        /// LOD (Level Of Detail) を取得します。
        /// LOD は 0 がもっとも簡略化された形状であり、数字が上がるほど形状が詳細であることを意味します。
        /// </summary>
        public int LOD
        {
            get
            {
                int lod = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_geometry_get_lod);
                return lod;
            }
        }

        /// <summary> デバッグ用に自身に関する情報を文字列で返します。 </summary>
        public override string ToString()
        {
            return $"[ Geometry : (id: {ID}) , (type: {Type}) , {ChildGeometryCount} child geometries , {PolygonCount} polygons , {LineStringCount} line strings , (attributesMap: {AttributesMap}) ]";
        }
        
        
        /// <summary>
        /// LineString の数を返します。
        /// TODO LineString の取得は未実装です。 GMLファイルが LineString を含むケースが今のところ見当たらないため
        /// </summary>
        public int LineStringCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_geometry_get_line_string_count);
                return count;
            }
        }

        /// <summary>
        /// SRSName を取得します。
        /// SRSNameは典型的には次のようなURL形式の文字列になるはずです。
        /// 例: "http://www.opengis.net/def/crs/EPSG/0/6697"
        /// </summary>
        public string SRSName
        {
            get
            {
                string srsName = DLLUtil.GetNativeString(Handle,
                    NativeMethods.plateau_geometry_get_srs_name);
                return srsName;
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_type(
                [In] IntPtr geometryHandle,
                out GeometryType type);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_geometries_count(
                [In] IntPtr geometryHandle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_polygons_count(
                [In] IntPtr geometryHandle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_child_geometry(
                [In] IntPtr geometryHandle,
                out IntPtr childGeomHandle,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_polygon(
                [In] IntPtr geometryHandle,
                out IntPtr polygonHandle,
                int index
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_lod(
                [In] IntPtr geometryHandle,
                out int outLod);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_srs_name(
                [In] IntPtr geometryHandle,
                out IntPtr outNameStrPtr,
                out int outStrLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_get_line_string_count(
                [In] IntPtr handle,
                out int outCount);
        }
    }
}
