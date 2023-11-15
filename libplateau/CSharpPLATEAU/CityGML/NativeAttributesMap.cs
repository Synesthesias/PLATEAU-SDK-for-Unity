using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

// key と value のペアに短縮名を付けます。
using AttrPair = System.Collections.Generic.KeyValuePair<string, PLATEAU.CityGML.NativeAttributeValue>;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// 属性の値の想定形式です。
    /// 形式が String, Double, Integer, Date, Uri, Measure である場合、内部的にはデータは string です。
    /// AttributeSet である場合、内部的にはデータは <see cref="NativeAttributesMap"/> への参照です。
    /// </summary>
    public enum AttributeType
    {
        String,
        Double,
        Integer,
        Date,
        Uri,
        Measure,
        AttributeSet
    }


    /// <summary>
    /// 属性の辞書です。
    /// <see cref="IReadOnlyDictionary{TKey,TValue}"/> を実装します。
    /// string をキーとし、 <see cref="NativeAttributeValue"/> が値になります。
    /// this[key] で <see cref="NativeAttributeValue"/> が返ります。
    /// </summary>
    public class NativeAttributesMap : IReadOnlyDictionary<string, NativeAttributeValue>
    {
        private readonly IntPtr handle;
        private string[] cachedKeys; // キャッシュの初期状態は null とするので null許容型にします。

        internal NativeAttributesMap(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary> 属性の数を返します。 </summary>
        public int Count
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_attributes_map_get_keys_count);
                return count;
            }
        }

        /// <summary>
        /// 属性のキーをすべて返します。
        /// 結果はキャッシュされるので2回目以降は速いです。
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                if (this.cachedKeys != null) return this.cachedKeys;
                
                this.cachedKeys = DLLUtil.GetNativeStringArrayByPtr(
                    this.handle,
                    NativeMethods.plateau_attributes_map_get_keys_count,
                    NativeMethods.plateau_attributes_map_get_keys);
                
                return this.cachedKeys;
            }
        }

        /// <summary>
        /// (key, value) のペアのうち value (<see cref="NativeAttributeValue"/>) をすべて返します。
        /// </summary>
        public IEnumerable<NativeAttributeValue> Values
        {
            get
            {
                var values = Keys.Select(key => this[key]);
                return values;
            }
        }

        /// <summary>
        /// 属性のキーから値を返します。
        /// <paramref name="key"/> が存在しない場合は <see cref="KeyNotFoundException"/> を投げます。
        /// </summary>
        public NativeAttributeValue this[string key]
        {
            get
            {
                APIResult result = NativeMethods.plateau_attributes_map_get_attribute_value(
                    this.handle, DLLUtil.StrToUtf8Bytes(key), out IntPtr valueHandle);
                // キーが存在しないエラー
                if (result == APIResult.ErrorValueNotFound)
                {
                    throw new KeyNotFoundException($"key {key} is not found in AttributesMap.");
                }

                // その他のエラー(Exception)
                DLLUtil.CheckDllError(result);
                return new NativeAttributeValue(valueHandle);
            }
        }

        /// <summary>
        /// 属性に <paramref name="key"/> が含まれていれば true,
        /// <paramref name="key"/> がなければ false を返します。
        /// </summary>
        public bool ContainsKey(string key)
        {
            APIResult result =
                NativeMethods.plateau_attributes_map_do_contains_key(this.handle, DLLUtil.StrToUtf8Bytes(key), out bool doContainsKey);
            DLLUtil.CheckDllError(result);
            return doContainsKey;
        }

        /// <summary>
        /// 属性辞書の中に <paramref name="key"/> が存在すればその値を <paramref name="value"/> に代入して true を返します。
        /// <paramref name="key"/> が存在しなければ <paramref name="value"/> に null を代入して false を返します。
        /// </summary>
        public bool TryGetValue(string key, out NativeAttributeValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// <paramref name="key"/> に対応する値 <see cref="NativeAttributeValue"/> を返します。
        /// なければ null を返します。
        /// </summary>
        public NativeAttributeValue GetValueOrNull(string key)
        {
            if (ContainsKey(key)) return this[key];
            return null;
        }

        public IEnumerator<AttrPair> GetEnumerator()
        {
            return new NativeAttributesMapEnumerator(this);
        }
        
        

        /// <summary>
        /// 属性の中身を、見やすくフォーマットした文字列にして返します。
        /// 子の属性も再帰的に含みます。
        /// </summary>
        public override string ToString()
        {
            return DLLUtil.GetNativeStringByValue(
                this.handle,
                NativeMethods.plateau_attributes_map_to_string_size,
                NativeMethods.plateau_attributes_map_to_string);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// インナークラスです。
        /// <see cref="NativeAttributesMap"/> に関する <see cref="IEnumerator"/> であり、
        /// foreachで回せるようにするための機能です。
        /// </summary>
        private class NativeAttributesMapEnumerator : IEnumerator<AttrPair>
        {
            private readonly NativeAttributesMap map;
            private int index;

            public NativeAttributesMapEnumerator(NativeAttributesMap map)
            {
                this.map = map;
                Reset();
            }

            public bool MoveNext()
            {
                this.index++;
                return this.index < this.map.Count;
            }

            public void Reset()
            {
                this.index = -1;
            }

            AttrPair IEnumerator<AttrPair>.Current
            {
                get
                {
                    object current = Current;
                    if (current == null) throw new NullReferenceException();
                    return (AttrPair)current;
                }
            }

            public object Current
            {
                get
                {
                    if (this.map.cachedKeys == null)
                    {
                        this.map.cachedKeys = this.map.Keys.ToArray();
                    }
                    string key = this.map.cachedKeys[this.index];
                    return new KeyValuePair<string, NativeAttributeValue>(key, this.map[key]);
                }
            }
            
            public void Dispose()
            {
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_attributes_map_get_keys_count(
                [In] IntPtr attributesMap,
                out int count);


            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_attributes_map_get_keys(
                [In] IntPtr attributesMap,
                [In, Out] IntPtr[] keyHandles,
                [Out] int[] outKeySizes);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_attributes_map_get_attribute_value(
                [In] IntPtr attributesMap,
                [In] byte[] keyUtf8,
                [Out] out IntPtr attrValuePtr);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_attributes_map_do_contains_key(
                [In] IntPtr attributesMap,
                [In] byte[] keyUtf8,
                out bool doContainsKey);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_attributes_map_to_string_size(
                [In] IntPtr attributesMap,
                out int size);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_attributes_map_to_string(
                [In] IntPtr attributesMap,
                [In, Out] IntPtr outStrPtrUtf8);
        }
    }
}
