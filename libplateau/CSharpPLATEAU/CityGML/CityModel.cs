using PLATEAU.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using PLATEAU.Native;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// GMLファイルをパースして得られる街のモデルです。
    /// 0個以上の <see cref="CityObject"/> を保持します。
    /// </summary>
    
    // 注意 : NativeDLL へ P/Invoke を使って CityModel のアドレスを送るとき、
    // Native側では cityModelHandle->getCityModelPtr() を介さないと
    // 正しいアドレスになりません。
    public sealed class CityModel : IDisposable
    {
        private int disposed;
        private CityObject[] rootCityObjects;

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
        /// <see cref="CityModel"/>の<paramref name="type"/>タイプの全ての<see cref="CityObject"/>を返します。
        /// </summary>
        /// <param name="type">取得したい都市オブジェクトのタイプ</param>
        /// <returns><paramref name="type"/>タイプの全ての<see cref="CityObject"/></returns>
        public CityObject[] GetCityObjectsByType(CityObjectType type)
        {
            var result = NativeMethods.plateau_city_model_get_all_city_object_count_of_type(
                Handle, out int count, type);
            DLLUtil.CheckDllError(result);

            var cityObjectHandles = new IntPtr[count];
            result = NativeMethods.plateau_city_model_get_all_city_objects_of_type(
                Handle, cityObjectHandles, type, count);
            DLLUtil.CheckDllError(result);

            var cityObjects = new CityObject[count];
            for (int i = 0; i < count; i++)
            {
                cityObjects[i] = new CityObject(cityObjectHandles[i]);
            }

            return cityObjects;
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


        /// <summary>
        /// cityModel の中心点を返します。
        /// また GMLファイルから cityModel の Envelope を読み取れない場合は、
        /// 中心点が分からないので原点座標を返します。
        /// </summary>
        public PlateauVector3d GetCenterPoint(int coordinateZoneID)
        {
            var centerPoint = DLLUtil.GetNativeValue<PlateauVector3d>(
                Handle, coordinateZoneID,
                NativeMethods.plateau_geometry_utils_get_center_point);
            return centerPoint;
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
                var result = NativeMethods.plateau_delete_city_model(Handle);
                DLLUtil.CheckDllError(result);
            }
            GC.SuppressFinalize(this);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_city_model(
                [In] IntPtr cityModel);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_get_root_city_objects(
                [In] IntPtr cityModel,
                [In, Out] IntPtr[] cityObjects,
                int count);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_get_root_city_object_count(
                [In] IntPtr cityModel,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_get_all_city_object_count_of_type(
                [In] IntPtr cityModel,
                out int count,
                CityObjectType type);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_get_all_city_objects_of_type(
                [In] IntPtr cityModel,
                [In, Out] IntPtr[] cityObjects,
                CityObjectType type,
                int count);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_city_model_get_city_object_by_id(
                [In] IntPtr handle,
                out IntPtr cityObjectPtr,
                [In] string id);
            
            // ***************
            //  geometry_utils_c.cpp
            // ***************
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_utils_get_center_point(
                [In] IntPtr cityModelPtr,
                out PlateauVector3d outCenterPoint,
                int coordinateZoneID);
        }
    }
}
