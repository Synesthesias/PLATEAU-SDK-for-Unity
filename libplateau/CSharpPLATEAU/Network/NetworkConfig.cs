using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Network
{
    public class NetworkConfig
    {
        public static string DefaultApiServerUrl
        {
            get
            {
                var urlNative = NativeString.Create();
                var result = NativeMethods.plateau_client_get_default_url(urlNative.Handle);
                DLLUtil.CheckDllError(result);
                return urlNative.ToString();
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_client_get_default_url(
                [In, Out] IntPtr nativeStrPtr);
        }
    }
}
