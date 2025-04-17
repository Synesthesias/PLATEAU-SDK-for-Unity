using System;
using System.Runtime.InteropServices;
using PLATEAU.Dataset;
using PLATEAU.Interop;

namespace PLATEAU.Native
{
    /// <summary>
    /// GridCodeのC++ Vectorです。
    /// 中身はVectorの廃棄時に削除されます。
    /// ダングリングを防ぐため、vectorへの追加・参照時はコピーを渡します。
    /// </summary>
    public class NativeVectorGridCode : NativeVectorDisposableBase<GridCode>
    {
        private NativeVectorGridCode(IntPtr ptr) : base(ptr)
        {
        }
        public static NativeVectorGridCode Create()
        {
            var result = NativeMethods.plateau_create_vector_grid_code(out var ptr);
            DLLUtil.CheckDllError(result);
            return new NativeVectorGridCode(ptr);
        }

        /// <summary>
        /// インデックスでアクセスし、そのコピーを返します。
        /// </summary>
        public override GridCode At(int index)
        {
            ThrowIfDisposed();
            var gridCodePtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_grid_code_get_value);
            return GridCode.CopyFrom(gridCodePtr); // 寿命管理のためコピーを渡します。元データはvector廃棄時に消します。
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_grid_code_count);
                return count;
            }
        }

        /// <summary>
        /// 追加します。ただし、無効なGridCodeの場合は何もしません。
        /// </summary>
        public void AddCopyOf(GridCode gridCode)
        {
            if (!gridCode.IsValid) return;
            // ダングリングを防ぐためコピーを追加します。
            var copied = GridCode.CopyFrom(gridCode.Handle);
            // vectorが廃棄されるまでGridCodeが廃棄されないようにします。
            copied.PreventAutoDispose();
            var result = NativeMethods.plateau_vector_grid_code_push_back_value(
                Handle, copied.Handle);
            DLLUtil.CheckDllError(result);
        }

        protected override void DisposeNative()
        {
            // ベクター内の各GridCodeを削除してからベクターを解放
            CleanupElements();
            var result = NativeMethods.plateau_delete_vector_grid_code(Handle);
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// ベクター内の各GridCodeオブジェクトを削除します。
        /// ベクター自体は削除しません。
        /// </summary>
        public void CleanupElements()
        {
            ThrowIfDisposed();
            var result = NativeMethods.plateau_cleanup_vector_grid_code(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_grid_code(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_grid_code(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_grid_code_get_value(
                [In] IntPtr vectorPtr,
                out IntPtr outGridCodePtr,
                int index);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_grid_code_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_grid_code_push_back_value(
                [In] IntPtr handle,
                [In] IntPtr gridCodePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_cleanup_vector_grid_code(
                [In] IntPtr handle);
        }
    }
}
