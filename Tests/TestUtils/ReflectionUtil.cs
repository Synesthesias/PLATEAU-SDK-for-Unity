using System;
using System.Reflection;
using UnityEngine;

namespace PlateauUnitySDK.Tests.TestUtils {
    public static class ReflectionUtil {
        public static TField GetPrivateStaticFieldVal<TField>(Type targetType, string fieldName) {
            var fieldInfo = GetPrivateStaticFieldInfo(targetType, fieldName);
            if (fieldInfo == null) {
                Debug.LogError($"Field '{fieldName}' is not found.");
                return default;
            }
            return (TField)fieldInfo.GetValue(null);
        }

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