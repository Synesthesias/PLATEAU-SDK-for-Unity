using System;
using System.IO;
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
        
        public string Download(string destDirPath, string url)
        {
            byte[] destUtf8 = DLLUtil.StrToUtf8Bytes(destDirPath);
            byte[] urlUtf8 = DLLUtil.StrToUtf8Bytes(url);
            var downloadedPathNative = NativeString.Create();
            var result = NativeMethods.plateau_client_download(
                Handle, destUtf8, urlUtf8, downloadedPathNative.Handle);
            DLLUtil.CheckDllError(result);
            string downloadedPath = downloadedPathNative.ToString();
            downloadedPathNative.Dispose();
            if (!File.Exists(downloadedPath)) throw new FileLoadException("Failed to download file.");
            if (new FileInfo(downloadedPath).Length == 0) throw new FileLoadException("Downloaded file size is zero. Maybe client.Url is wrong.");
            return downloadedPath;
        }

        protected override void DisposeNative()
        {
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_client);
        }
    }
}
