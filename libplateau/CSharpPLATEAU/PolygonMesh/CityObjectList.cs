using System;
using System.Linq;
using PLATEAU.Native;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.PolygonMesh
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CityObjectIndex
    {
        public int PrimaryIndex;
        public int AtomicIndex;

        public static CityObjectIndex FromUV(PlateauVector2f uv)
        {
            return new CityObjectIndex
            {
                PrimaryIndex = (int)Math.Round(uv.X),
                AtomicIndex = (int)Math.Round(uv.Y)
            };
        }
    }


    public class CityObjectList
    {
        public IntPtr Handle { get; }

        internal CityObjectList(IntPtr handle)
        {
            Handle = handle;
        }

        public CityObjectIndex[] GetAllKeys()
        {
            var keys = NativeVectorCityObjectIndex.Create();
            var result = NativeMethods.plateau_city_object_list_get_all_keys(Handle, keys.Handle);
            DLLUtil.CheckDllError(result);
            return keys.ToArray();
        }

        public string GetPrimaryID(int index)
        {
            var result = NativeMethods.plateau_city_object_list_get_primary_id(Handle, out var strPtr, out int strLength, index);
            return result != APIResult.Success
                ? null
                : DLLUtil.ReadUtf8Str(strPtr, strLength);
        }

        public string GetAtomicID(CityObjectIndex index)
        {
            var result = NativeMethods.plateau_city_object_list_get_atomic_id(Handle, out var strPtr, out int strLength, index);
            return result != APIResult.Success
                ? null
                : DLLUtil.ReadUtf8Str(strPtr, strLength);
        }

        /// <summary>
        /// gml:idに対応する<see cref="CityObjectIndex"/>を取得します。
        /// 存在しない場合は(-1, -1)を返します。
        /// </summary>
        public CityObjectIndex GetCityObjectIndex(string gmlID)
        {
            var result = NativeMethods.plateau_city_object_list_get_city_gml_index(Handle, out var index, gmlID);
            DLLUtil.CheckDllError(result);
            return index;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_city_object_list(
                [In] IntPtr handle);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_list_get_primary_id(
                [In] IntPtr handle,
                out IntPtr outStrPtr,
                out int strLength,
                [In] int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_list_get_atomic_id(
                [In] IntPtr handle,
                out IntPtr outStrPtr,
                out int strLength,
                [In] CityObjectIndex index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_list_get_city_gml_index(
                [In] IntPtr handle,
                out CityObjectIndex index,
                [In] string gmlID);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_object_list_get_all_keys(
                [In] IntPtr handle,
                [In, Out] IntPtr keys);
        }
    }
}
