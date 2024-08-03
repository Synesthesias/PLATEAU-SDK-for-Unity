using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    // #TODO : 一時的に消す
    //[CustomPropertyDrawer(typeof(RnModel))]
    public class RnModelDrawer : PropertyDrawer
    {
        [field: SerializeField]
        private ulong DebugLaneId { get; set; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            var model = property.boxedValue as RnModel;
            var linksProp = property.FindPropertyRelative("links");
            if (linksProp != null)
                EditorGUILayout.LabelField($"Model {linksProp?.arraySize}");
        }

        public void Draw(RnModel model)
        {
            EditorGUILayout.LongField((long)DebugLaneId, "Lane ID");

        }

    }
}