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
        private bool autoDispose;

        protected abstract void DisposeNative();

        /// <summary>
        /// <paramref name="handle"/>のポインタ位置にC++の実体インスタンスがあるとしてC#と結びつけます。
        /// autoDisposeについては<see cref="PreventAutoDispose"/>のコメントを参照してください。
        /// </summary>
        protected PInvokeDisposable(IntPtr handle, bool autoDispose = true)
        {
            Handle = handle;
            this.autoDispose = autoDispose;
        }
        
        public void Dispose()
        {
            if (!this.autoDispose) return;
            if (this.isDisposed) return;
            DisposeNative();
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }
        
        /// <summary>
        /// 自身ではのC++リソース廃棄を行わないようにします。
        /// 用途は別のタイミングで廃棄したいとき――例えば自身を保持するコンテナクラスにメモリ管理を任せている時などに使います。
        /// </summary>
        public void PreventAutoDispose()
        {
            this.autoDispose = false;
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
