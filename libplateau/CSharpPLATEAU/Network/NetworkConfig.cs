using PLATEAU.Interop;

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
    }
}
