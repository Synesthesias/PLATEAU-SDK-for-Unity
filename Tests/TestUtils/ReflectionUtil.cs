using System;
using System.Reflection;
using UnityEngine;

namespace PlateauUnitySDK.Tests.TestUtils {
    
    /// <summary>
    /// テストのとき public でない対象をテストするために、
    /// リフレクションで 非public のものにアクセスするためのツールを提供します。
    /// </summary>
    public static class ReflectionUtil {
        
        /// <summary>
        /// targetType の private static フィールドの値を取得します。
        /// </summary>
        public static TField GetPrivateStaticFieldVal<TField>(Type targetType, string fieldName) {
            var fieldInfo = GetPrivateStaticFieldInfo(targetType, fieldName);
            if (fieldInfo == null) {
                Debug.LogError($"Field '{fieldName}' is not found.");
                return default;
            }
            return (TField)fieldInfo.GetValue(null);
        }

        /// <summary>
        /// targetType の private static フィールドの値を newFiledValue にします。
        /// </summary>
        public static void SetPrivateStaticFieldVal<TField>(TField newFieldValue, Type targetType, string fieldName) {
            var fieldInfo = GetPrivateStaticFieldInfo(targetType, fieldName);
            if (fieldInfo == null) {
                Debug.LogError($"Field '{fieldName}' is not found.");
                return;
            }
            fieldInfo.SetValue(null, newFieldValue);
            return;
        }

        private static FieldInfo GetPrivateStaticFieldInfo(Type targetType, string fieldName) {
            return targetType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}