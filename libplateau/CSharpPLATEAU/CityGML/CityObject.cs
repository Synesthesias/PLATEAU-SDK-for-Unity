using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    [Flags]
    public enum CityObjectType : ulong
    {
        COT_GenericCityObject = 1ul,
        COT_Building = 1ul << 1,
        COT_Room = 1ul << 2,
        COT_BuildingInstallation = 1ul << 3,
        COT_BuildingFurniture = 1ul << 4,
        COT_Door = 1ul << 5,
        COT_Window = 1ul << 6,
        COT_CityFurniture = 1ul << 7,
        COT_Track = 1ul << 8,
        COT_Road = 1ul << 9,
        COT_Railway = 1ul << 10,
        COT_Square = 1ul << 11,
        COT_PlantCover = 1ul << 12,
        COT_SolitaryVegetationObject = 1ul << 13,
        COT_WaterBody = 1ul << 14,
        COT_ReliefFeature = 1ul << 15,
        COT_ReliefComponent = 1ul << 35,
        COT_TINRelief = 1ul << 36,
        COT_MassPointRelief = 1ul << 37,
        COT_BreaklineRelief = 1ul << 38,
        COT_RasterRelief = 1ul << 39,
        COT_LandUse = 1ul << 16,
        COT_Tunnel = 1ul << 17,
        COT_Bridge = 1ul << 18,
        COT_BridgeConstructionElement = 1ul << 19,
        COT_BridgeInstallation = 1ul << 20,
        COT_BridgePart = 1ul << 21,
        COT_BuildingPart = 1ul << 22,

        COT_WallSurface = 1ul << 23,
        COT_RoofSurface = 1ul << 24,
        COT_GroundSurface = 1ul << 25,
        COT_ClosureSurface = 1ul << 26,
        COT_FloorSurface = 1ul << 27,
        COT_InteriorWallSurface = 1ul << 28,
        COT_CeilingSurface = 1ul << 29,
        COT_CityObjectGroup = 1ul << 30,
        COT_OuterCeilingSurface = 1ul << 31,
        COT_OuterFloorSurface = 1ul << 32,


        // covers all supertypes of tran::_TransportationObject that are not Track, Road, Railway or Square...
        // there are to many for to few bits to explicitly enumerate them. However Track, Road, Railway or Square should be used most of the time
        COT_TransportationObject = 1ul << 33,

        // ADD Building model 
        COT_IntBuildingInstallation = 1ul << 34,

        COT_All = 0xFFFFFFFFFFFFFFFFul
    };

    /// <summary>
    /// <para>
    /// CityGMLにおける都市オブジェクトです。</para>
    /// <para>
    /// <see cref="CityModel"/> または 親<see cref="CityObject"/> が <see cref="CityObject"/> を保持します。</para>
    /// <para>
    /// この<see cref="CityObject"/> は <see cref="CityObjectType"/> , <see cref="CityGML.Address"/> , 子<see cref="CityObject"/> , <see cref="Geometry"/> を保持します。
    /// </para>
    /// </summary>
    public class CityObject : FeatureObject
    {
        private CityObjectType type = 0;
        private Address cachedAddress;
        private CityObject[] cachedChildCityObjects; // キャッシュの初期状態は null とするので null許容型にします。
        private Geometry[] cachedGeometries;

        internal CityObject(IntPtr handle) : base(handle)
        {
        }

        public CityObjectType Type
        {
            get
            {
                if (this.type != 0)
                {
                    return this.type;
                }

                this.type = DLLUtil.GetNativeValue<CityObjectType>(Handle,
                    NativeMethods.plateau_city_object_get_type);
                return this.type;
            }
        }

        /// <summary>
        /// ジオメトリの数を返します。
        /// </summary>
        public int GeometryCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_city_object_get_geometries_count);
                return count;
            }
        }

        public int ImplicitGeometryCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_city_object_get_implicit_geometry_count);
                return count;
            }
        }

        public Address Address
        {
            get
            {
                if (this.cachedAddress != null) return this.cachedAddress;
                var addressHandle = DLLUtil.GetNativeValue<IntPtr>(
                    Handle,
                    NativeMethods.plateau_city_object_get_address
                );
                this.cachedAddress = new Address(addressHandle);
                return this.cachedAddress;
            }
        }

        public int ChildCityObjectCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_city_object_get_child_city_object_count);
                return count;
            }
        }
        
        public CityObject GetChildCityObject(int index)
        {
            var ret = DLLUtil.ArrayCache(ref this.cachedChildCityObjects, index, ChildCityObjectCount, () =>
            {
                IntPtr childHandle = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                    NativeMethods.plateau_city_object_get_child_city_object);
                return new CityObject(childHandle);
            });

            return ret;
        }

        /// <summary>
        /// 子<see cref="CityObject"/> をforeachやLinqで回したい時に利用できます。
        /// </summary>
        public IEnumerable<CityObject> ChildCityObjects
        {
            get
            {
                int cnt = ChildCityObjectCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetChildCityObject(i);
                }
            }
        }
        
        public Geometry GetGeometry(int index)
        {
            var geom = DLLUtil.ArrayCache(ref this.cachedGeometries, index, GeometryCount, () =>
            {
                IntPtr geomHandle = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                    NativeMethods.plateau_city_object_get_geometry);
                return new Geometry(geomHandle);
            });
            return geom;
        }

        /// <summary>
        /// 各 <see cref="Geometry"/> を foreach で回したい時に利用できます。
        /// </summary>
        public IEnumerable<Geometry> Geometries
        {
            get
            {
                int cnt = GeometryCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetGeometry(i);
                }
            }
        }
        
        /// <summary>
        /// 子孫の <see cref="CityObject"/> をすべて再帰的にイテレートします。自分自身を含みます。
        /// イテレートの順番は DFS（深さ優先探索）です。
        /// </summary>
        public IEnumerable<CityObject> CityObjectDescendantsDFS
        {
            get
            {
                var results = IterateChildrenDfsRecursive(this);
                foreach (var r in results)
                {
                    yield return r;
                }
            }
        }
        
        private static IEnumerable<CityObject> IterateChildrenDfsRecursive(CityObject co)
        {
            yield return co;
            foreach (var child in co.ChildCityObjects)
            {
                var results = IterateChildrenDfsRecursive(child);
                foreach (var r in results)
                {
                    yield return r;
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_type(
                [In] IntPtr cityObjectHandle,
                out CityObjectType outCityObjType);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_geometries_count(
                [In] IntPtr cityObjectHandle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_address(
                [In] IntPtr cityObjectHandle,
                out IntPtr addressHandle);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_implicit_geometry_count(
                [In] IntPtr cityObjectHandle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_child_city_object_count(
                [In] IntPtr cityObjectHandle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_child_city_object(
                [In] IntPtr cityObjectHandle,
                out IntPtr outChildHandle,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_get_geometry(
                [In] IntPtr cityObjectHandle,
                out IntPtr outGeometryHandle,
                int index);
        }
    }
}
