using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Network
{
    public class NetworkConfig
    {

        public static string MockServerUrl
        {
            get
            {
                var urlNative = NativeString.Create();
                var result = NativeMethods.plateau_client_get_mock_server_url(urlNative.Handle);
                DLLUtil.CheckDllError(result);
                return urlNative.ToString();
            }
        }

        private static class NativeMethods
        {

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_client_get_mock_server_url(
                [In, Out] IntPtr nativeStrPtr);
            
        }
    }
}
