using System;
using PLATEAU.Interop;

namespace PLATEAU
{
    /// <summary>
    /// 廃棄時に Native側で delete する必要があるものの基底クラスです。
    /// Native側で delete する処理として、 DisposeNative() がサブクラスで実装されていることを前提に、
    /// GC処理時または using(var a){} ブロックを抜ける時に廃棄されます。
    /// </summary>
    public abstract class PInvokeDisposable : IDisposable
    {
        public IntPtr Handle { get; }
        private bool isDisposed;

        protected abstract void DisposeNative();

        public PInvokeDisposable(IntPtr handle)
        {
            Handle = handle;
        }
        
        public void Dispose()
        {
            if (this.isDisposed) return;
            DisposeNative();
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }

        ~PInvokeDisposable()
        {
            Dispose();
        }
    }
}
