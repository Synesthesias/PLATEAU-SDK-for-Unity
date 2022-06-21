using System;
using System.Collections.Generic;
using LibPLATEAU.NET.Util;

namespace LibPLATEAU.NET.CityGML
{
    /// <summary>
    /// <para>
    /// CityGMLにおける都市オブジェクトです。</para>
    /// <para>
    /// <see cref="CityModel"/> または 親<see cref="CityObject"/> が <see cref="CityObject"/> を保持します。</para>
    /// <para>
    /// この<see cref="CityObject"/> は <see cref="CityObjectType"/> , <see cref="LibPLATEAU.NET.CityGML.Address"/> , 子<see cref="CityObject"/> , <see cref="Geometry"/> を保持します。
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
    }
}