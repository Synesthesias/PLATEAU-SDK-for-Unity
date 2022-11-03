using System.Collections.Generic;
using System.Text;

namespace PLATEAU.Util
{
    public static class DebugUtil
    {
        /// <summary>
        /// 複数オブジェクトの ToString をまとめて行い、結合した string を返します。
        /// </summary>
        public static string EnumerableToString<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null) return "";
            var sb = new StringBuilder();
            foreach (T t in enumerable)
            {
                sb.Append(t);
                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}
