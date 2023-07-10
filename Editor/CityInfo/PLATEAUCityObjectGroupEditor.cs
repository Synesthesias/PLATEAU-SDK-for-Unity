using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Geometries;
using PLATEAU.Native;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU
{
    [CustomEditor(typeof(PLATEAUCityObjectGroup))]
    public class PLATEAUCityObjectGroupEditor : UnityEditor.Editor
    {
        Vector2 scroll;

        public void OnEnable()
        {
            UnityEditorInternal.ComponentUtility.MoveComponentUp(target as UnityEngine.Component);
        }

        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUCityObjectGroup;
            if (cog == null) return;
            var json = cog.SerializedCityObjects;

            PlateauEditorStyle.Heading("ëÆê´èÓïÒ", null);
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
