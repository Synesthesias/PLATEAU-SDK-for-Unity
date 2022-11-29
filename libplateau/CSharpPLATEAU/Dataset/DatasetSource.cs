using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.Dataset
{
    public class DatasetSource : PInvokeDisposable
    {
        public DatasetSource(IntPtr handle) : base(handle)
        {
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
         public static DatasetSource CreateServer(string datasetID)
         {
             var result = NativeMethods.plateau_create_dataset_source_server(
                 out var ptr, datasetID);
             DLLUtil.CheckDllError(result);
             return new DatasetSource(ptr);
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
    }
}
