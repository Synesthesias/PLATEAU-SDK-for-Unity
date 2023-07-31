using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PLATEAU
{
    [CustomEditor(typeof(PLATEAUCityObjectGroup))]
    public class PLATEAUCityObjectGroupEditor : UnityEditor.Editor
    {
        Vector2 scroll;

        public void OnEnable()
        {
            ComponentUtility.MoveComponentUp(target as Component);
        }

        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUCityObjectGroup;
            if (cog == null) return;
            SerializedProperty prop = serializedObject.FindProperty("serializedCityObjects");
            var json = prop.stringValue;

            PlateauEditorStyle.Heading("属性情報", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(400));
                EditorGUILayout.TextArea(json);
                EditorGUILayout.EndScrollView();
            }
           
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}
