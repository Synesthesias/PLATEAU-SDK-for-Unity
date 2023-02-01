using System;
using System.IO;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Network
{
    public class Client
    {
        public IntPtr Handle { get; }

        private Client(IntPtr handle)
        {
            Handle = handle;
        }

        public static Client Create(string serverUrl, string apiToken)
        {
            var result = NativeMethods.plateau_create_client(out var ptr, serverUrl, apiToken);
            DLLUtil.CheckDllError(result);
            return new Client(ptr);
        }

        public static Client CreateForMockServer()
        {
            var result = NativeMethods.plateau_create_client_for_mock_server(out var ptr);
            DLLUtil.CheckDllError(result);
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
            get =>
                DLLUtil.GetNativeStringByValue(Handle,
                    NativeMethods.plateau_client_get_api_server_url_size,
                    NativeMethods.plateau_client_get_api_server_url);
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

        public void Dispose()
        {
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_client);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_create_client(
                out IntPtr newClientPtr,
                [In] string serverUrl,
                [In] string apiToken);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_client_for_mock_server(
                out IntPtr newClientPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_client(
                [In] IntPtr ptr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_client_get_metadata(
                [In] IntPtr clientPtr,
                [In, Out] IntPtr refNativeArrayDatasetMetadataGroupPtr);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_client_set_api_server_url(
                [In] IntPtr clientPtr,
                [In] string url);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_client_get_api_server_url_size(
                [In] IntPtr clientPtr,
                out int outUrlSize);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_client_get_api_server_url(
                [In] IntPtr clientPtr,
                [In,Out] IntPtr outStrPtr );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_client_download(
                [In] IntPtr clientPtr,
                [In] byte[] destinationDirectoryUtf8,
                [In] byte[] urlUtf8,
                [In,Out] IntPtr refNativeStringPtr);
        }
    }
}
