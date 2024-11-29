using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.Util
{
    public static class EnumEx
    {
        /// <summary>
        /// Enum.GetValues(typeof(T)).Cast<T>();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>() where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

    }
}