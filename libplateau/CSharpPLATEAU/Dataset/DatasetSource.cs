using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.Network;

namespace PLATEAU.Dataset
{
    public class DatasetSource : PInvokeDisposable
    {
        private DatasetSource(IntPtr handle) : base(handle)
        {
        }
        
        /// <summary>
        /// <see cref="DatasetSource"/> を生成します。
        /// </summary>
        /// <param name="isServer">データの場所は true ならサーバー、falseならローカルです。</param>
        /// <param name="localSourcePath">ローカルモードでのみ利用します。インポート元のパスを渡します。</param>
        /// <param name="serverDatasetID">
        /// サーバーモードでのみ利用します。データセットのIDを渡します。
        /// そのIDとは、APIサーバーにデータセットの一覧を問い合わせたときに得られるID文字列です。例: 東京23区のデータセットのIDは "23ku"
        /// </param>
        /// <param name="serverUrl">サーバーモードでのみ利用します。サーバーのURLです。</param>
        private static DatasetSource Create(bool isServer, string localSourcePath, string serverDatasetID, string serverUrl)
        {
            return Create(new DatasetSourceConfig(isServer, localSourcePath, serverDatasetID, serverUrl, ""));
        }
        
        
        public static DatasetSource Create(DatasetSourceConfig config)
        {
            return config.IsServer ?
                CreateServer(config.ServerDatasetID, config.ServerUrl, config.ServerToken) :
                CreateLocal(config.LocalSourcePath);
        }

        /// <summary>
         /// ローカルPCのデータセットを指す <see cref="DatasetSource"/> を作ります。
         /// </summary>
        public static DatasetSource CreateLocal(string path)
         {
             var pathUtf8 = DLLUtil.StrToUtf8Bytes(path);
             var result = NativeMethods.plateau_create_dataset_source_local(out var datasetSourcePtr, pathUtf8);
             DLLUtil.CheckDllError(result);
             return new DatasetSource(datasetSourcePtr);
         }

         /// <summary>
         /// リモートPCのデータセットを指す <see cref="DatasetSource"/> を作ります。
         /// </summary>
         public static DatasetSource CreateServer(string datasetID, string serverUrl, string apiToken)
         {
             Client client = Client.Create(serverUrl, apiToken);
             var result = NativeMethods.plateau_create_dataset_source_server(
                 out var ptr, datasetID, client.Handle);
             DLLUtil.CheckDllError(result);
             return new DatasetSource(ptr);
         }

         public static DatasetSource CreateForMockServer(string datasetID)
         {
             return CreateServer(datasetID, NetworkConfig.MockServerUrl, "");
         }

         public DatasetAccessor Accessor
         {
             get
             {
                 ThrowIfDisposed();
                 var result = NativeMethods.plateau_dataset_source_get_accessor(
                     Handle, out var accessorPtr);
                 DLLUtil.CheckDllError(result);
                 return new DatasetAccessor(accessorPtr);
             }
         }

        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_dataset_source(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_dataset_source_local(
                out IntPtr outDatasetSource,
                [In] byte[] sourcePathUtf8);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_dataset_source(
                [In] IntPtr datasetSourcePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_source_get_accessor(
                [In] IntPtr datasetSourcePtr,
                out IntPtr accessorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_dataset_source_server(
                out IntPtr newDatasetSourcePtr,
                [In] string datasetID,
                [In] IntPtr clientPtr);
        }
    }
}
