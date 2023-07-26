using Newtonsoft.Json;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;
using static PLATEAU.CityInfo.CityObjectList;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「属性情報」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityAttributeGUI : IEditorDrawable
    {
        private static readonly int THIS_TAB_INDEX = 3;
        private static readonly string GIZMO_GAMEOBJECT_NAME = "PLATEAUCityObjectGroup_GizmoGameObject";
        private bool isActive = false;
        private PlateauWindowGUI parentGUI;
        private UnityEditor.EditorWindow parentEditorWindow;
        private CityObject parent;
        private CityObject child;
        private string parentJson;
        private string childJson;
        private string targetObjectName;
        private string errorMessage;
        private Vector2 scrollParent;
        private Vector2 scrollChild;
        private CityObjectGizmoDrawer gizmoDrawer;

        public CityAttributeGUI(UnityEditor.EditorWindow parentEditorWindow, PlateauWindowGUI parent)
        {
            this.parentGUI = parent;
            this.parentEditorWindow = parentEditorWindow;
        }
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("クリックした地物の情報を表示します。");

            if(!string.IsNullOrEmpty(errorMessage))
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.MultiLineLabelWithBox(errorMessage);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(targetObjectName))
                {
                    PlateauEditorStyle.Heading("対象オブジェクト", null);

                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        EditorGUILayout.LabelField(targetObjectName);
                    }
                }

                if (!string.IsNullOrEmpty(parentJson) || !string.IsNullOrEmpty(childJson))
                {
                    PlateauEditorStyle.Heading("属性情報", null);
                }

                if (!string.IsNullOrEmpty(parentJson))
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        EditorGUILayout.LabelField(parent.GmlID);
                        scrollParent = EditorGUILayout.BeginScrollView(scrollParent, GUILayout.MaxHeight(400));
                        EditorGUILayout.TextArea(parentJson);
                        EditorGUILayout.EndScrollView();
                    }
                }

                if (!string.IsNullOrEmpty(childJson))
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        EditorGUILayout.LabelField(child.GmlID);
                        scrollChild = EditorGUILayout.BeginScrollView(scrollChild, GUILayout.MaxHeight(400));
                        EditorGUILayout.TextArea(childJson);
                        EditorGUILayout.EndScrollView();
                    }
                }
            }

            if(!isActive)
            {
                parentGUI.OnTabChange += OnParentTabChanged;
                SceneView.duringSceneGui += OnSceneGUI;
                isActive = true;
            }
        }

        void OnSceneGUI(SceneView scene)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100000.0f))
                {
                    if (hit.transform.TryGetComponent<PLATEAUCityObjectGroup>(out var cog))
                    {
                        errorMessage = null;
                        parent = cog.GetPrimaryCityObject(hit);
                        child = cog.GetAtomicCityObject(hit);

                        if(parent == null || child == null)
                        {
                            errorMessage = $"{hit.transform.gameObject.name}:\r\n地物がクリックされましたが、属性情報が見つかりませんでした。\r\nインポート時に属性情報を含める設定になっているか確認してください。";
                        }
                        else
                        {
                            //Json表示
                            parentJson = JsonConvert.SerializeObject(parent, Formatting.Indented);
                            childJson = JsonConvert.SerializeObject(child, Formatting.Indented);
                            
                            targetObjectName = hit.transform.root.name;

                            //最小値物
                            if (!string.IsNullOrEmpty(cog.CityObjects.outsideParent))
                            {
                                Transform parentTrans = (hit.transform.parent.gameObject.name == cog.CityObjects.outsideParent ) ? hit.transform.parent : GameObject.Find(cog.CityObjects.outsideParent)?.transform;
                                if (parentTrans != null)
                                {
                                    GetGizmoDrawer().ShowParentSelection(parentTrans, parent.IndexInMesh, GetIdAndAttributeString(parent));
                                }
                                else
                                    Debug.Log($"parent is null");
                            }
                            else
                            {
                                GetGizmoDrawer().ShowParentSelection(hit.transform, parent.IndexInMesh, GetIdAndAttributeString(parent));
                            }
                            GetGizmoDrawer().ShowChildSelection(hit.transform, child.IndexInMesh, GetIdAndAttributeString(child));
                        }
                    }
                    else
                    {
                        errorMessage = $"{hit.transform.gameObject.name}:\r\n地物ではありません";
                    }
                }
                else
                {
                    Clear();
                    errorMessage = "クリック箇所のレイキャストがコライダーにヒットしませんでした。\r\nコライダーがセットされているか確認してください。";  
                }

                scene.Repaint();
                parentEditorWindow.Repaint();
            }
        }

        private void OnParentTabChanged(int index)
        {
            //タブ変更時に削除
            if (index!= THIS_TAB_INDEX)
            {
                Clear();
                parentEditorWindow.Repaint();
                SceneView.duringSceneGui -= OnSceneGUI;
                parentGUI.OnTabChange -= OnParentTabChanged;
                isActive = false;
            }
        }

        private string GetIdAndAttributeString(CityObject obj)
        {
            var id = obj.GmlID;
            var attr = obj.AttributesMap.DebugString(0);
            if(attr.Length > 50)
            {
                attr = attr.Substring(0, 50);
            }
            return $"{id}\n{attr}";
        }

        private void Clear()
        {
            parentJson = childJson = targetObjectName = errorMessage = null;
            parent = child = null;
            DestroyGizmoDrawer();
        }

        private CityObjectGizmoDrawer GetGizmoDrawer()
        {
            if (gizmoDrawer == null)
            {
                gizmoDrawer = GameObject.Find(GIZMO_GAMEOBJECT_NAME)?.GetComponent<CityObjectGizmoDrawer>();
                if (gizmoDrawer == null)
                {
                    var obj = new GameObject(GIZMO_GAMEOBJECT_NAME);
                    gizmoDrawer = obj.AddComponent<CityObjectGizmoDrawer>();
                }
            }
            return gizmoDrawer;
        }

        private void DestroyGizmoDrawer()
        {
            if (gizmoDrawer != null)
            {
                GameObject.DestroyImmediate(gizmoDrawer.gameObject);
            }
        }
    }
}
