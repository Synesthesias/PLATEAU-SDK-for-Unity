using Newtonsoft.Json;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAttributeGUI : IEditorDrawable
    {
        private static readonly int THIS_TAB_INDEX = 3;

        private PlateauWindowGUI parentGUI;
        UnityEditor.EditorWindow parentWindow;

        private bool isActive = false;

        private CityObject.CityObjectParam parent;
        private CityObject.CityObjectChildParam child;

        private string parentJson;
        private string childJson;

        private Vector2 scrollParent;
        private Vector2 scrollChild;

        public CityAttributeGUI(UnityEditor.EditorWindow parentEditorWindow, PlateauWindowGUI parent)
        {
            this.parentGUI = parent;
            this.parentWindow = parentEditorWindow;
        }
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("クリックした地物の情報を表示します。");
            PlateauEditorStyle.Heading("対象オブジェクト", null);

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                scrollParent = EditorGUILayout.BeginScrollView(scrollParent, GUILayout.MaxHeight(400));
                EditorGUILayout.TextArea(parentJson);
                EditorGUILayout.EndScrollView();
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                scrollChild = EditorGUILayout.BeginScrollView(scrollChild, GUILayout.MaxHeight(400));
                EditorGUILayout.TextArea(childJson);
                EditorGUILayout.EndScrollView();
            }

            if(!isActive)
            {
                SceneView.duringSceneGui += OnSceneGUI;
                isActive = true;
            }
        }

        void OnSceneGUI(SceneView scene)
        {
            #region Raycastテスト

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Debug.Log("Mouse Down");

                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100000.0f))
                {
                    if (hit.transform.TryGetComponent<PLATEAUCityObjectGroup>(out var cog))
                    {
                        var obj = cog.GetCityObject(hit);
                        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        Debug.Log(json);

                        parent = obj.cityObjects[0];         
                        child = parent.children[0];
                        parent.children.Clear();

                        parentJson = JsonConvert.SerializeObject(parent, Formatting.Indented);
                        childJson = JsonConvert.SerializeObject(child, Formatting.Indented);

                        parentWindow.Repaint();

                        cog.ShowSelected(child.IndexInMesh);
                    }
                }
                else
                    Debug.Log("no hit");

            }

            #endregion Raycastテスト

            if(parentGUI.tabIndex != THIS_TAB_INDEX)
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                isActive = false;
            }
        }
    }
}
