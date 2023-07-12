using Newtonsoft.Json;
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

        void OnSceneGUI()
        {
            //RaycastÉeÉXÉg
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100000.0f))
                {
                    if(hit.transform.TryGetComponent<PLATEAUCityObjectGroup>(out var cog))
                    {
                        var obj = cog.GetCityObject(hit);
                        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        Debug.Log(json);
                    }
                }
                else
                    Debug.Log("no hit");
            }
        }
    }
}
