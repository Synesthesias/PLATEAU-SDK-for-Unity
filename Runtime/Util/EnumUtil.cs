using System;
using System.Collections.Generic;

namespace PLATEAU.Util
{
    public static class EnumUtil
    {
        /// <summary>
        /// フラグ式のEnumを受け取り、そのフラグが立っている各Enumタイプに分解します。
        /// </summary>
        public static IEnumerable<TEnum> EachFlags<TEnum>(TEnum input) where TEnum : Enum
        {
            foreach (TEnum value in Enum.GetValues(input.GetType()))
            {
                if (Convert.ToUInt64(value) == 0) continue; // noneは除きます
                if (input.HasFlag(value)) yield return value;
            }
        }
    }
}
