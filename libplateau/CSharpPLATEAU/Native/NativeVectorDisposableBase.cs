using System;
using System.Collections;
using System.Collections.Generic;

namespace PLATEAU.Native
{
    /// <summary>
    /// <seealso cref="NativeVectorBase{T}"/> の自動で NativeDispose する版です。
    /// </summary>
    public abstract class NativeVectorDisposableBase<T> : PInvokeDisposable, INativeVector<T>
    {
        protected NativeVectorDisposableBase(IntPtr handle) : base(handle)
        {
        }
        
        // この実装は NativeVectorBase　と重複しています。
        // C# 8.0 以降の機能であるインターフェイスのデフォルト実装を使えば重複せず書けそうですが、
        // 今のところそのC#バージョンは利用していません。

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
