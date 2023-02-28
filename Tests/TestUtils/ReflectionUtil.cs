using System;
using System.Reflection;
using UnityEngine;

namespace PLATEAU.Tests.TestUtils
{

    /// <summary>
    /// テストのとき public でない対象をテストするために、
    /// リフレクションで 非public のものにアクセスするためのツールを提供します。
    /// </summary>
    public static class ReflectionUtil
    {

        /// <summary>
        /// targetType の private static フィールドの値を取得します。
        /// </summary>
        public static TField GetPrivateStaticFieldVal<TField>(Type targetType, string fieldName)
        {
            var fieldInfo = GetPrivateStaticFieldInfo(targetType, fieldName);
            if (fieldInfo == null)
            {
                Debug.LogError($"Field '{fieldName}' is not found.");
                return default;
            }
            return (TField)fieldInfo.GetValue(null);
        }

        /// <summary>
        /// targetType の private static フィールドの値を newFiledValue にします。
        /// </summary>
        public static void SetPrivateStaticFieldVal<TField>(TField newFieldValue, Type targetType, string fieldName)
        {
            var fieldInfo = GetPrivateStaticFieldInfo(targetType, fieldName);
            if (fieldInfo == null)
            {
                Debug.LogError($"Field '{fieldName}' is not found.");
                return;
            }
            fieldInfo.SetValue(null, newFieldValue);
        }


        /// <summary>
        /// privateフィールドの値を取得します。
        /// 型 <paramref name="targetType"/> である <paramref name="targetObj"/>の
        /// フィールド <paramref name="fieldName"/> の値を取得します。
        /// </summary>
        public static TField GetPrivateFieldVal<TField>(Type targetType, object targetObj, string fieldName)
        {
            FieldInfo info = GetPrivateFieldInfo(targetType, fieldName);
            return (TField)info.GetValue(targetObj);
        }

        public static void InvokePrivateMethod(Type targetType, object targetObj, string methodName, params object[] parameters)
        {
            var method = targetType.GetMethod(methodName,
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new NullReferenceException("method is not found.");
            method.Invoke(targetObj, parameters);
        }

        /// <summary>
        /// privateフィールドの値を更新します。(非static)
        /// 型 <paramref name="targetType"/> である <see cref="targetObj"/> の
        /// フィールド <paramref name="fieldName"/> の値を <paramref name="newFieldValue"/> にします。
        /// </summary>
        public static void SetPrivateFieldVal<TField>(Type targetType, object targetObj, string fieldName, TField newFieldValue)
        {
            FieldInfo info = GetPrivateFieldInfo(targetType, fieldName);
            info.SetValue(targetObj, newFieldValue);
        }

        private static FieldInfo GetPrivateStaticFieldInfo(Type targetType, string fieldName)
        {
            return targetType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static FieldInfo GetPrivateFieldInfo(Type targetType, string fieldName)
        {
            return targetType.GetField(fieldName,
                BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}