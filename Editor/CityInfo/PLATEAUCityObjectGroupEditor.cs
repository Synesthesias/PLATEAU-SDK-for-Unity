using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.PolygonMesh;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PLATEAU.Editor.CityInfo
{
    [CustomEditor(typeof(PLATEAUCityObjectGroup))]
    public class PLATEAUCityObjectGroupEditor : UnityEditor.Editor
    {
        private readonly ScrollView scrollView = new (GUILayout.MaxHeight(400));

        public void OnEnable()
        {
            ComponentUtility.MoveComponentUp(target as Component);
        }

        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUCityObjectGroup;
            if (cog == null) return;
            
            PlateauEditorStyle.Heading("粒度", null);
            EditorGUILayout.LabelField(cog.Granularity.ToJapaneseString());
            
            SerializedProperty prop = serializedObject.FindProperty("serializedCityObjects");
            var json = prop.stringValue;

            PlateauEditorStyle.Heading("属性情報", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                scrollView.Draw(() =>
                {
                    EditorGUILayout.TextArea(json);
                });
            }
           
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}
