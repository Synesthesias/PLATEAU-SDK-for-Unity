using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Graph;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{
    public class ARnPartsDrawer<T> : PropertyDrawer
    {
        private static readonly string typeName = typeof(T).Name;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var id = property.FindPropertyRelative("debugId");
            // 編集不可にする
            EditorGUI.BeginDisabledGroup(true);
            if (id == null)
            {
                EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"undefined");
            }
            else
            {
                EditorGUI.LongField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"{label.text}[{typeName}]", (long)(id?.ulongValue));
            }
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    //[CustomPropertyDrawer(typeof(ARnParts<RVertex>))]
    //public class RVertexDrawer : ARnPartsDrawer<RVertex>
    //{
    //}

    //[CustomPropertyDrawer(typeof(ARnParts<REdge>))]
    //public class REdgeDrawer : ARnPartsDrawer<REdge>
    //{
    //}

    //[CustomPropertyDrawer(typeof(ARnParts<RPolygon>))]
    //public class RPolygonDrawer : ARnPartsDrawer<RPolygon>
    //{
    //}
}