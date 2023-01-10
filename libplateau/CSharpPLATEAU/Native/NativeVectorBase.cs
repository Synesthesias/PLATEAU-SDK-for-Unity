using System;
using System.Collections;
using System.Collections.Generic;

namespace PLATEAU.Native
{
    /// <summary>
    /// C++側の vector を扱う基底クラスです。
    /// 自動で Dispose したいときは <see cref="NativeVectorDisposableBase{T}"/> を利用してください。
    /// </summary>
    public abstract class NativeVectorBase<T> : INativeVector<T>
    {
        protected IntPtr Handle { get; }
        protected NativeVectorBase(IntPtr handle)
        {
            Handle = handle;
        }

        public abstract T At(int index);
        public abstract int Length { get; }

        public IEnumerator<T> GetEnumerator()
        {
            int len = Length;
            for (int i = 0; i < len; i++)
            {
                yield return At(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
