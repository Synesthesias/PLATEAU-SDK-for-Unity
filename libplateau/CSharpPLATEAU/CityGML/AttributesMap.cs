using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PLATEAU.CityGML.Util;

// key と value のペアに短縮名を付けます。
using AttrPair = System.Collections.Generic.KeyValuePair<string, PLATEAU.CityGML.AttributeValue>;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// 属性の辞書です。
    /// <see cref="IReadOnlyDictionary{TKey,TValue}"/> を実装します。
    /// string をキーとし、 <see cref="AttributeValue"/> が値になります。
    /// this[key] で <see cref="AttributeValue"/> が返ります。
    /// </summary>
    public class AttributesMap : IReadOnlyDictionary<string, AttributeValue>
    {
        private readonly IntPtr handle;
        private string[] cachedKeys; // キャッシュの初期状態は null とするので null許容型にします。

        internal AttributesMap(IntPtr handle)
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
        /// (key, value) のペアのうち value (<see cref="AttributeValue"/>) をすべて返します。
        /// </summary>
        public IEnumerable<AttributeValue> Values
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
        /// 例外を投げてほしくない場合は代わりに <see cref="TryGetValue"/> メソッドを利用してください。
        /// </summary>
        public AttributeValue this[string key]
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
                return new AttributeValue(valueHandle);
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
        public bool TryGetValue(string key, out AttributeValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }

            value = null;
            return false;
        }

        public IEnumerator<AttrPair> GetEnumerator()
        {
            return new AttributesMapEnumerator(this);
        }

        /// <summary>
        /// key と value と型 のセットをすべて文字にします。
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[ ");
            sb.Append(String.Join(", ", Keys.Select(key => $"{{ ({this[key].Type}) {key} , {this[key].AsString} }}")));
            sb.Append(" ]");
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// インナークラスです。
        /// <see cref="AttributesMap"/> に関する <see cref="IEnumerator"/> であり、
        /// foreachで回せるようにするための機能です。
        /// </summary>
        private class AttributesMapEnumerator : IEnumerator<AttrPair>
        {
            private readonly AttributesMap map;
            private int index;

            public AttributesMapEnumerator(AttributesMap map)
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

            AttrPair IEnumerator<AttrPair>.Current => (AttrPair)Current;

            public object Current
            {
                get
                {
                    if (this.map.cachedKeys == null)
                    {
                        this.map.cachedKeys = this.map.Keys.ToArray();
                    }
                    string key = this.map.cachedKeys[this.index];
                    return new KeyValuePair<string, AttributeValue>(key, this.map[key]);
                }
            }
            
            public void Dispose()
            {
            }
        }
    }
}