using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// PropertyDrawersとセットで使用する
namespace PLATEAU.Util
{

    [CustomPropertyDrawer(typeof(ConditionalShowAttribute))]
    public class ConditionalShowDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalShowAttribute attr = (ConditionalShowAttribute)attribute;
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(attr.conditionField);
            if (conditionProperty == null)
                Debug.LogError($"Condition field '{attr.conditionField}' not found");
            else if (conditionProperty.boolValue)
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalShowAttribute attr = (ConditionalShowAttribute)attribute;
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(attr.conditionField);

            return (conditionProperty != null && conditionProperty.boolValue) ? EditorGUI.GetPropertyHeight(property) : 0f;
        }
    }


    [CustomPropertyDrawer(typeof(ConditionalShowBoolAttribute))]
    public class ConditionalShowBoolDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalShowBoolAttribute attr = (ConditionalShowBoolAttribute)attribute;
            if (attr.show)
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalShowBoolAttribute attr = (ConditionalShowBoolAttribute)attribute;
            return attr.show ? EditorGUI.GetPropertyHeight(property) : 0f;
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }

}
