using System;
using PLATEAU.Interop;

namespace PLATEAU.Network
{
    public class Client : PInvokeDisposable
    {
        public Client(IntPtr handle) : base(handle)
        {
        }

        public static Client Create()
        {
            var ptr = DLLUtil.PtrOfNewInstance(NativeMethods.plateau_create_client);
            return new Client(ptr);
        }

        public NativeVectorDatasetMetadataGroup GetDatasetMetadataGroup()
        {
            var metadataGroups = NativeVectorDatasetMetadataGroup.Create();
            var result = NativeMethods.plateau_client_get_metadata(
                Handle, metadataGroups.Handle);
            DLLUtil.CheckDllError(result);
            return metadataGroups;
        }

        public string Url
        {
            get
            {
                return DLLUtil.GetNativeStringByValue(Handle,
                    NativeMethods.plateau_client_get_api_server_url_size,
                    NativeMethods.plateau_client_get_api_server_url);
            }
            set
            {
                var result = NativeMethods.plateau_client_set_api_server_url(Handle, value);
                DLLUtil.CheckDllError(result);
            }
        }

        protected override void DisposeNative()
        {
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_client);
        }
    }
}
