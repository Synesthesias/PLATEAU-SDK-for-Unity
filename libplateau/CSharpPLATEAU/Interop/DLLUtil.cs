using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PLATEAU.Interop
{
    internal enum APIResult
    {
        Success,
        ErrorUnknown,
        ErrorValueNotFound,
        ErrorLoadingCityGml,
        ErrorIndexOutOfBounds,
        ErrorFileSystem,
        ErrorInvalidArgument,
        ErrorValueIsInvalid
    }
    
    /// <summary>
    /// DLLとデータをやりとりするためのユーティリティクラスです。
    /// </summary>
    internal static class DLLUtil
    {
        #if UNITY_IOS && !UNITY_EDITOR_OSX
            public const string DllName = "__Internal";
        #else
            public const string DllName = "plateau";
        #endif

        /// <summary>
        /// NativeMethods で頻出するメソッドの型を delegate で登録しておくことで、delegate で呼び出せるようにします。
        /// ただし、すべてのメソッドがこのような型をしているわけではないので、対応していないメソッドもあります。
        /// </summary>
        internal delegate APIResult GetterDelegate<T>(IntPtr handle, out T ret);
        internal delegate APIResult GetterDelegateInt<T>(IntPtr handle, out T ret, int i);
        internal delegate APIResult IntArrayGetDelegate(IntPtr handle, int[] ret);
        internal delegate APIResult StrPtrLengthDelegate(IntPtr handle,out IntPtr strPtr,out int strLength);

        internal delegate APIResult StrPtrLengthArrayDelegate(IntPtr handle, IntPtr[] strPointers, int[] strLengths);
        internal delegate APIResult StrValueArrayGetDelegate(IntPtr handle, IntPtr strPtrArrayPtr);

        internal delegate APIResult StrValueGetDelegate(IntPtr handle, IntPtr strPtr);
        internal delegate APIResult NativeCreateDelegate(out IntPtr newPtr);

        internal delegate APIResult NativeVoidDelegate(IntPtr ptr);

        /// <summary>
        /// ネイティブで new してそのポインタを返す関数を引数にとり、それを実行してエラーチェックしてからポインタを返します。
        /// </summary>
        internal static IntPtr PtrOfNewInstance(NativeCreateDelegate createFunc)
        {
            var result = createFunc(out var newPtr);
            CheckDllError(result);
            return newPtr;
        }

        /// <summary>
        /// IntPtrを1つ引数にとるメソッドを実行し、その後エラーチェックします。
        /// </summary>
        internal static void ExecNativeVoidFunc(IntPtr handle, NativeVoidDelegate nativeVoidFunc)
        {
            var result = nativeVoidFunc(handle);
            CheckDllError(result);
        }

        /// <summary>
        /// DLLから文字列のポインタの配列を受け取り、各ポインタから文字列を読んで string[] で返します。
        /// 次の2つの NativeMethods を引数で受け取り利用します。
        /// ・配列の要素数を得るメソッド
        /// ・文字列のポインタの配列と、各文字列のバイト数を int[] で得るメソッド
        /// </summary>
        internal static string[] GetNativeStringArrayByPtr(
            IntPtr handle,
            GetterDelegate<int> arrayLengthGetter,
            StrPtrLengthArrayDelegate strPtrAndLengthGetter)
        {
            int cnt = GetNativeValue(handle, arrayLengthGetter);
            int[] strLengths = new int[cnt];
            var strHandles = new IntPtr[cnt];
            var result = strPtrAndLengthGetter(handle, strHandles, strLengths);
            CheckDllError(result);
            string[] ret = ReadNativeStrPtrArray(strHandles, strLengths);
            return ret;
        }

        /// <summary>
        /// DLL内の文字列の配列のコピーを受け取ります。
        /// 次の3つの NativeMethods を引数で受け取り利用します。
        /// ・配列の要素数を取得するメソッド
        /// ・各文字列のバイト数を配列で取得するメソッド
        /// ・文字列の配列のコピーを受け取るメソッド
        /// </summary>
        internal static string[] GetNativeStringArrayByValue(
            IntPtr handle,
            GetterDelegate<int> arrayLengthGetter,
            IntArrayGetDelegate strLengthsGetter,
            StrValueArrayGetDelegate strArrayGetter)
        {
            int cnt = GetNativeValue(handle, arrayLengthGetter);
            int[] strSizes = new int[cnt];
            var result = strLengthsGetter(handle, strSizes);
            CheckDllError(result);
            var strPtrArrayPtr = AllocPtrArray(cnt, strSizes);
            result = strArrayGetter(handle, strPtrArrayPtr);
            CheckDllError(result);
            var ret = PtrToStringArray(strPtrArrayPtr, cnt, strSizes);
            FreePtrArray(strPtrArrayPtr, cnt);
            return ret;
        }

        /// <summary>
        /// DLLから文字列のコピーを受け取ります。
        /// </summary>
        /// <param name="handle">対象オブジェクトのポインタです。</param>
        /// <param name="strSizeGetter">文字列のバイト数を受け取るための NativeMethod です。</param>
        /// <param name="strGetter">文字列のコピーを受け取るための NativeMethod です。</param>
        internal static string GetNativeStringByValue(
            IntPtr handle,
            GetterDelegate<int> strSizeGetter,
            StrValueGetDelegate strGetter)
        {
            var result = strSizeGetter(handle, out int strSize);
            CheckDllError(result);
            IntPtr strPtr = Marshal.AllocCoTaskMem(strSize);
            var result2 = strGetter(handle, strPtr);
            CheckDllError(result2);
            string str = ReadUtf8Str(strPtr, strSize - 1);
            Marshal.FreeCoTaskMem(strPtr);
            return str;
        }
        
        // 下の3つのメソッドは、 DLL側で一時的に生成した「文字列の配列」の完全なコピーが欲しいという状況で利用できます。

        /// <summary>
        /// ポインタの配列を作ります。
        /// 配列内の各ポインタのメモリ領域を <paramref name="sizes"/>[i] の大きさで確保します。
        /// ポインタ配列 のアドレスを <see cref="IntPtr"/> で返します。
        /// ここで確保したメモリは必ず <see cref="FreePtrArray"/> で解放してください。
        /// そうでないとメモリリークになります。
        /// </summary>
        /// <param name="count">ポインタ配列の要素数です。</param>
        /// <param name="sizes"><see cref="sizes"/>[i] = i番目のポインタの確保バイト数 となるような int配列です。</param>
        private static IntPtr AllocPtrArray(int count, int[] sizes)
        {
            if (count > sizes.Length)
            {
                throw new ArgumentException("sizes.Length should not be smaller than count.");
            }
            
            var managedPtrArray = new IntPtr[count]; // ポインタの配列 (managed)
            for (int i = 0; i < count; i++)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(sizes[i]); // 配列内の各ポインタについてメモリ確保
                managedPtrArray[i] = ptr;
            }
            
            int sizeOfPtrArray = Marshal.SizeOf(typeof(IntPtr)) * count;
            var unmanagedPtrArray = Marshal.AllocCoTaskMem(sizeOfPtrArray); // ポインタの配列 (unmanaged)
            Marshal.Copy(managedPtrArray, 0, unmanagedPtrArray, count);
            return unmanagedPtrArray;
        }
        
        /// <summary>
        /// <see cref="AllocPtrArray"/> で確保したメモリを解放します。
        /// ポインタ配列内の各ポインタを解放し、続けてポインタ配列自体を解放します。
        /// </summary>
        /// <param name="ptrOfPtrArray">解放したいポインタ配列を指定します。</param>
        /// <param name="count">ポインタ配列の要素数を指定します。</param>
        private static unsafe void FreePtrArray(IntPtr ptrOfPtrArray, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ptrArray = (IntPtr*)ptrOfPtrArray;
                if (ptrArray == null) throw new NullReferenceException();
                var ptr = (ptrArray)[i];
                Marshal.FreeCoTaskMem(ptr);
            }
            Marshal.FreeCoTaskMem(ptrOfPtrArray);
        }

        /// <summary>
        /// ポインタ(stringの配列のポインタ)から string の配列を読み込みます。
        /// ポインタ <paramref name="ptrOfStringArray"/> は <see cref="AllocPtrArray"/> で確保したものと同じであることを前提とし、
        /// 引数 <paramref name="count"/>, <paramref name="sizes"/> には <see cref="AllocPtrArray"/> で渡したものと同じ値を渡してください。 
        /// </summary>
        private static unsafe string[] PtrToStringArray(IntPtr ptrOfStringArray, int count, int[] sizes)
        {
            string[] ret = new string[count];
            for (int i = 0; i < count; i++)
            {
                var stringArray = (IntPtr*)ptrOfStringArray;
                if (stringArray == null) throw new NullReferenceException();
                var stringPtr = stringArray[i];
                ret[i] = ReadUtf8Str(stringPtr, sizes[i] - 1);
            }
            return ret;
        }

        internal static byte[] PtrToBytes(IntPtr ptr, int length)
        {
            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return bytes;
        }

        
        /// <summary>
        /// <paramref name="result"/> が <see cref="APIResult.Success"/> でなかった場合に例外を投げます。
        /// </summary>
        internal static void CheckDllError(APIResult result)
        {
            if (result == APIResult.ErrorIndexOutOfBounds)
            {
                throw new IndexOutOfRangeException("Index is out of range.");
            }
            if (result != APIResult.Success)
            {
                throw new Exception($"Error in Lib Plateau DLL. APIResult = {result}");
            }
        }
        

        /// <summary>
        /// DLLから文字列のポインタと文字列のバイト数を受け取り、
        /// それをもとに文字列を読んで返します。
        /// </summary>
        /// <param name="handle"> 関数に渡すハンドルです。 </param>
        /// <param name="strPtrAndLengthGetter"> 文字列のポインタとバイト数を受け取るための関数を指定します。 </param>
        internal static string GetNativeString(
            IntPtr handle,
            StrPtrLengthDelegate strPtrAndLengthGetter
        )
        {
            APIResult result = strPtrAndLengthGetter(handle, out IntPtr strPtr, out int strLength);
            CheckDllError(result);
            return ReadUtf8Str(strPtr, strLength - 1);
        }

        /// <summary>
        /// ネイティブ関数から値を受け取り、エラーチェックしてから値を返します。
        /// NativeMethods を呼ぶたびに手動で <see cref="CheckDllError"/> を呼ぶのと同義ですが、それだと冗長なのでこのメソッドにまとめました。
        /// </summary>
        /// <param name="handle">ネイティブ関数に渡すハンドルです。</param>
        /// <param name="getterMethod"> NativeMethods のメソッドを指定します。</param>
        /// <typeparam name="T">戻り値の型です。</typeparam>
        /// <returns>ネイティブ関数から受け取った値を返します。</returns>
        internal static T GetNativeValue<T>(IntPtr handle, GetterDelegate<T> getterMethod)
        {
            APIResult result = getterMethod(handle, out T ret);
            CheckDllError(result);
            return ret;
        }

        /// <summary>
        /// <see cref="GetNativeValue{T}(System.IntPtr,GetterDelegate{T})"/>
        /// の亜種で、<paramref name="getterMethod"/> のint引数が1つ増えた版です。
        /// </summary>
        internal static T GetNativeValue<T>(IntPtr handle, int i, GetterDelegateInt<T> getterMethod)
        {
            APIResult result = getterMethod(handle, out T ret, i);
            CheckDllError(result);
            return ret;
        }

        /// <summary>
        /// T型の配列であるキャッシュから値を読んで返します。
        /// キャッシュに値がなければ、値を生成してキャッシュに記憶してから返します。
        /// </summary>
        /// 
        /// <param name="cache">
        /// キャッシュです。
        /// <paramref name="cache"/> そのものが null であれば、キャッシュを T[arrayLength] で初期化します。
        /// キャッシュ本体がnullではないが、キャッシュの要素 <paramref name="cache"/>[<paramref name="index"/>] がnullであるときは、
        /// <paramref name="generator"/> で値を生成しキャッシュに記憶して返します。
        /// </param>
        ///
        /// <param name="index">T型配列から値を読みたいインデックスです。</param>
        /// <param name="arrayLength">T型配列の要素数です。</param>
        /// <param name="generator">キャッシュに値がないとき、値を生成するための関数を指定します。</param>
        public static T ArrayCache<T>(ref T[] cache, int index, int arrayLength, Func<T> generator)
        {
            if (cache == null)
            {
                // キャッシュの初期化
                cache = new T[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    cache[i] = default(T);
                }
            }

            T item = cache[index];
            // キャッシュがヒットしたとき
            if (item != null)
            {
                return item;
            }
            // キャッシュにないとき
            item = generator();
            cache[index] = item;
            return item;
        }
        /// <summary>
        /// 文字列のポインタの配列から各ポインタの文字を読み、
        /// string[]で返します。
        /// </summary>
        private static string[] ReadNativeStrPtrArray(IntPtr[] strPointers, int[] strSizes)
        {
            if (strPointers.Length != strSizes.Length)
            {
                throw new ArgumentException(
                    $"Array length of arguments should be same. {nameof(strPointers)}.Length = {strPointers.Length}, {nameof(strSizes)}.Length = {strSizes.Length}");
            }

            int cnt = strPointers.Length;
            var ret = new string[cnt];
            for (int i = 0; i < cnt; i++)
            {
                // -1 は null終端文字の分です。
                string str = ReadUtf8Str(strPointers[i], strSizes[i] - 1) ?? "";

                ret[i] = str;
            }

            return ret;
        }

        public static string ReadUtf8Str(IntPtr strPtr, int strByteSize)
        {
            if (strByteSize < 0)
            {
                throw new ArgumentException($"{nameof(strByteSize)} should be non-negative number. Actual is {strByteSize} .");
            }
            if (strPtr == IntPtr.Zero) throw new NullReferenceException();
            var data = new List<byte>(strByteSize);
            for (int i = 0; i < strByteSize; i++)
            {
                byte b = Marshal.ReadByte(strPtr, i);
                if (b == 0)
                {
                    break;
                }
                data.Add(b);
            }

            return Encoding.UTF8.GetString(data.ToArray());
        }

        public static byte[] StrToUtf8Bytes(string str)
        {
            if (str == null)
            {
                return new byte[] { 0 }; // null終端文字のみ
            }
            var bytes = new List<byte>(Encoding.UTF8.GetBytes(str));
            bytes.Add(0); // null終端文字
            return bytes.ToArray();
        }

    }
}
