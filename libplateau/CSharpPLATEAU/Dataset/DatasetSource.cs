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
        /// <see cref="DatasetSource"/>を生成します。
        /// ローカルかサーバーかは、引数の型によって判別します。
        /// </summary>
        public static DatasetSource Create(IDatasetSourceConfig conf)
        {
            switch (conf)
            {
                case DatasetSourceConfigLocal localConf:
                    return CreateLocal(localConf);
                case DatasetSourceConfigRemote remoteConf:
                    return CreateServer(remoteConf);
                default:
                    throw new ArgumentOutOfRangeException(nameof(conf));
            }
        }


        /// <summary>
        /// ローカルPCのデータセットを指す <see cref="DatasetSource"/> を作ります。
        /// </summary>
        private static DatasetSource CreateLocal(DatasetSourceConfigLocal conf)
        {
            var pathUtf8 = DLLUtil.StrToUtf8Bytes(conf.LocalSourcePath);
            var result = NativeMethods.plateau_create_dataset_source_local(out var datasetSourcePtr, pathUtf8);
            DLLUtil.CheckDllError(result);
            return new DatasetSource(datasetSourcePtr);
        }

         /// <summary>
         /// リモートPCのデータセットを指す <see cref="DatasetSource"/> を作ります。
         /// </summary>
         private static DatasetSource CreateServer(DatasetSourceConfigRemote conf)
         {
             Client client = Client.Create(conf.ServerUrl, conf.ServerToken);
             var result = NativeMethods.plateau_create_dataset_source_server(
                 out var ptr, conf.ServerDatasetID, client.Handle);
             DLLUtil.CheckDllError(result);
             return new DatasetSource(ptr);
         }

         public static DatasetSource CreateForMockServer(string datasetID)
         {
             var conf = new DatasetSourceConfigRemote(datasetID, NetworkConfig.MockServerUrl, "");
             return CreateServer(conf);
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
