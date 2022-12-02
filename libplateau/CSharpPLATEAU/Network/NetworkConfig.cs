using PLATEAU.Interop;

namespace PLATEAU.Network
{
    public class NetworkConfig
    {
        public static string MockServerURL
        {
            get
            {
                var urlNative = NativeString.Create();
                var result = NativeMethods.plateau_network_config_mock_server_url(urlNative.Handle);
                DLLUtil.CheckDllError(result);
                return urlNative.ToString();
            }
        }
    }
}
