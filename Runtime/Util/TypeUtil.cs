using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PLATEAU.Util
{
    public static class TypeUtil
    {
        /// <summary>
        /// FieldInfo or PropertyInfoどっちかの場合に値を設定する
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetValue(MemberInfo m, object obj, object v)
        {
            if (m is FieldInfo f)
                f.SetValue(obj, v);
            else if (m is PropertyInfo p)
                p.SetValue(obj, v);
            else
                throw new InvalidOperationException($"SetValue MemberInfo {m.Name} is nor FieldInfo or PropertyInfo");
        }

        /// <summary>
        /// FieldInfo or PropertyInfoどっちかの場合に値を取得する
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static object GetValue(MemberInfo m, object obj)
        {
            if (m is FieldInfo f)
                return f.GetValue(obj);
            if (m is PropertyInfo p)
                return p.GetValue(obj);
            throw new InvalidOperationException($"GetValue MemberInfo {m.Name} is nor FieldInfo or PropertyInfo");
        }

        /// <summary>
        /// FieldInfo or PropertyInfoどっちかの場合にMemberTypeを取得する
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Type GetMemberType(MemberInfo m)
        {
            if (m is FieldInfo f)
                return f.FieldType;
            if (m is PropertyInfo p)
                return p.PropertyType;
            throw new InvalidOperationException($"GetValue MemberInfo {m?.Name} is nor FieldInfo or PropertyInfo");
        }


        private class Work
        {
            public HashSet<object> visited = new HashSet<object>();
        }

        /// <summary>
        /// 単純型かどうか(子メンバーを持たないかどうか)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string);
        }

        /// <summary>
        /// あんま正しく動かない可能性あるので注意
        /// flagsで指定される. objのtargetType型のフィールド/プロパティを子要素含めてすべて取得する.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="memberInfo"></param>
        /// <param name="targetType"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static IEnumerable<Tuple<FieldInfo, object>> GetAllMembersRecursively(object obj, FieldInfo memberInfo, Type targetType, BindingFlags flags, Work work)
        {
            if (obj == null)
                yield break;

            // すでにこのオブジェクト探索済みなら無視(参照がループしているような場合対応
            if (work.visited.Contains(obj))
                yield break;

            work.visited.Add(obj);
            var type = obj.GetType();
            // 見つかったらそれを返す
            if (type.IsSubclassOf(targetType) || type == targetType)
            {
                yield return new Tuple<FieldInfo, object>(memberInfo, obj);
            }
            // プリミティブ型は子を持たないので打ち切る
            else if (type.IsSimpleType())
            {
                yield break;
            }
            // 配列型の場合はそれぞれの要素を返す
            else if (type.IsArray)
            {
                foreach (var e in (Array)obj)
                {
                    foreach (var elm in GetAllMembersRecursively(e, memberInfo, targetType, flags, work))
                        yield return elm;
                }
            }
            // それ以外の
            else
            {

                var fieldInfos = type.GetFields(flags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var v = fieldInfo.GetValue(obj);
                    foreach (var elm in GetAllMembersRecursively(v, fieldInfo, targetType, flags, work))
                        yield return elm;
                }
            }
        }

        public static IEnumerable<Tuple<FieldInfo, object>> GetAllMembersRecursively(object obj, Type targetType, BindingFlags flags)
        {
            return GetAllMembersRecursively(obj, null, targetType, flags, new Work());
        }

        public static IEnumerable<Tuple<FieldInfo, T>> GetAllMembersRecursively<T>(object obj, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            return GetAllMembersRecursively(obj, typeof(T), flags).Select(item => new Tuple<FieldInfo, T>(item.Item1, (T)(item.Item2)));
        }

        /// <summary>
        /// 自動生成されたプロパティが内部で持っているフィールドのFieldInfoを取得する
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static FieldInfo GetPropertyBackingField(Type type, PropertyInfo propertyInfo)
        {
            // https://learn.microsoft.com/en-us/ef/core/modeling/backing-field?tabs=data-annotations
            // type = propertyInfo.ReflectedTypeでも良さそうだけど確実じゃないので外部から渡す形式にする
            // #TODO : すべてのコンパイラがk_BackingFieldという名前にする保証があるのか？
            return type?.GetField($"<{propertyInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
