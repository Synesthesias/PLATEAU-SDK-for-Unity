using System;
using PLATEAU.Editor.Window.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    internal class ObjectSelectGui
    {
        public GameObject[] SelectedObjs { get; private set; } = Array.Empty<GameObject>();
        private Vector2 scrollSelected;
        private EditorWindow parentEditorWindow;
        private CityMaterialAdjustGUI materialAdjustGUI;
        
        public ObjectSelectGui(CityMaterialAdjustGUI materialAdjustGUI, EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;
            this.materialAdjustGUI = materialAdjustGUI;
            OnSelectionChanged();
        }

        public void Draw()
        {
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in SelectedObjs)
                {
                    if (obj == null)
                    {
                        EditorGUILayout.LabelField("(削除されたゲームオブジェクト)");
                    }
                    else
                    {
                        EditorGUILayout.LabelField(obj.name);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }
        
        public void OnSelectionChanged()
        {
            if (materialAdjustGUI.CurrentAdjuster.IsSearched) return; // 「検索」ボタンを押したら対象は変更できないようにします。
            UpdateSelection();
            
        }

        public void UpdateSelection()
        {
            SelectedObjs = Selection.gameObjects;
            parentEditorWindow.Repaint();
        }
        
        public void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Array.Clear(SelectedObjs, 0, SelectedObjs.Length);
        }
        
    }
}