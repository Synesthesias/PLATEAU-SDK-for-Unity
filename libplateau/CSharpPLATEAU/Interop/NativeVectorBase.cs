using System;
using System.Collections;
using System.Collections.Generic;

namespace PLATEAU.Interop
{
    public abstract class NativeVectorBase<T> : PInvokeDisposable, IEnumerable<T>
    {
        protected NativeVectorBase(IntPtr handle) : base(handle)
        {
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
