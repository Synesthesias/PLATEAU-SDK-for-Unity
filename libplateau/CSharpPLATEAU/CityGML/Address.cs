using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// 住所情報を保持します。
    /// <see cref="CityObject"/> が <see cref="Address"/> を保持します。
    /// </summary>
    public class Address
    {
        private readonly IntPtr handle;

        public Address(IntPtr handle)
        {
            this.handle = handle;
        }
        
        public string Country =>
            DLLUtil.GetNativeString(
                this.handle,
                NativeMethods.plateau_address_get_country
            );
        
        public string Locality =>
            DLLUtil.GetNativeString(
                this.handle,
                NativeMethods.plateau_address_get_locality
            );
        
        public string PostalCode =>
            DLLUtil.GetNativeString(
                this.handle,
                NativeMethods.plateau_address_get_postal_code
            );
        
        public string ThoroughFareName =>
            DLLUtil.GetNativeString(
                this.handle,
                NativeMethods.plateau_address_get_thoroughfare_name
            );
        
        public string ThoroughFareNumber =>
            DLLUtil.GetNativeString(
                this.handle,
                NativeMethods.plateau_address_get_thoroughfare_number
            );

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_address_get_country(
                [In] IntPtr addressHandle,
                out IntPtr outCountryNamePtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_address_get_locality(
                [In] IntPtr addressHandle,
                out IntPtr outStrPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_address_get_postal_code(
                [In] IntPtr addressHandle,
                out IntPtr outStrPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_address_get_thoroughfare_name(
                [In] IntPtr addressHandle,
                out IntPtr outStrPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_address_get_thoroughfare_number(
                [In] IntPtr addressHandle,
                out IntPtr outStrPtr,
                out int strLength);
        }
    }
}
