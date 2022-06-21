using System;
using LibPLATEAU.NET.Util;

namespace LibPLATEAU.NET.CityGML
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
    }
}