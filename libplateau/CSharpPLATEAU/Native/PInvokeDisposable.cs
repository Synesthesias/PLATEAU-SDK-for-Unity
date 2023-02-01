using System;

namespace PLATEAU.Native
{
    /// <summary>
    /// 廃棄時に Native側で delete する必要があるものの基底クラスです。
    /// Native側で delete する処理として、 DisposeNative() がサブクラスで実装されていることを前提とします。
    /// 廃棄タイミングは GC処理時 または using(var a){} ブロックを抜ける時 または Dispose() を呼んだときです。
    /// </summary>
    // TODO PInvokeDisposableで置き換え可能なラッパークラスを置き換える。具体的には GeoReference(済), GltfWriter, ObjWriter, Client.
    public abstract class PInvokeDisposable : IDisposable
    {
        public IntPtr Handle { get; }
        private bool isDisposed;

        protected abstract void DisposeNative();

        protected PInvokeDisposable(IntPtr handle)
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

        protected void ThrowIfDisposed()
        {
            if (this.isDisposed) throw new ObjectDisposedException("Object is disposed.");
        }

        ~PInvokeDisposable()
        {
            Dispose();
        }
    }
}
