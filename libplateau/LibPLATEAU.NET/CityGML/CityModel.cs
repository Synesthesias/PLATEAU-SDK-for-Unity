using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using LibPLATEAU.NET.Util;

namespace LibPLATEAU.NET.CityGML
{
    /// <summary>
    /// GMLファイルをパースして得られる街のモデルです。
    /// 0個以上の <see cref="CityObject"/> を保持します。
    /// </summary>
    public sealed class CityModel : IDisposable
    {
        private int disposed;
        private CityObject[] rootCityObjects;　// get されるまでは null なので null許容型とします。

        /// <summary>
        /// セーフハンドルを取得します。
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// <see cref="CityModel"/> のトップレベルにある <see cref="CityObject"/> の一覧を返します。
        /// </summary>
        public ReadOnlyCollection<CityObject> RootCityObjects
        {
            get
            {
                if (this.rootCityObjects != null)
                {
                    return Array.AsReadOnly(this.rootCityObjects);
                }

                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_city_model_get_root_city_object_count);
                var cityObjectHandles = new IntPtr[count];
                APIResult result = NativeMethods.plateau_city_model_get_root_city_objects(this.Handle, cityObjectHandles, count);
                DLLUtil.CheckDllError(result);
                this.rootCityObjects = new CityObject[count];
                for (var i = 0; i < count; ++i)
                {
                    this.rootCityObjects[i] = new CityObject(cityObjectHandles[i]);
                }

                return Array.AsReadOnly(this.rootCityObjects);
            }
        }

        /// <summary>
        /// idから <see cref="CityObject"/> を返します。
        /// 該当idのものがない場合は <see cref="KeyNotFoundException"/> を投げます。
        /// </summary>
        public CityObject GetCityObjectById(string id)
        {
            var result = NativeMethods.plateau_city_model_get_city_object_by_id(
                Handle, out IntPtr cityObjectPtr, id);
            if (result == APIResult.ErrorValueNotFound)
            {
                throw new KeyNotFoundException($"id {id} is not found.");
            }
            DLLUtil.CheckDllError(result);
            return new CityObject(cityObjectPtr);
        }

        internal CityModel(IntPtr handle)
        {
            Handle = handle;
        }

        ~CityModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                NativeMethods.plateau_delete_city_model(this.Handle);
            }
            GC.SuppressFinalize(this);
        }
    }
}
