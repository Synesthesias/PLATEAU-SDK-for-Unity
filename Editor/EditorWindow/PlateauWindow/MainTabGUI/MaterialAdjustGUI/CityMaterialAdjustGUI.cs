using System;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityMaterialAdjustGUI : IEditorDrawable
    {
        private UnityEditor.EditorWindow parentEditorWindow;
        private GameObject[] selectedObjs = new GameObject[0];
        private Vector2 scrollSelected;
        
        private string attrKey = "";
        private readonly DestroyOrPreserveSrcGUI destroyOrPreserveSrcGUI = new();
        private bool isTargetDetermined;
        
        // 分類基準が地物型か属性情報かでGUIと処理が変わるので、2つのGUIを用意します。
        private MaterialCriterionGuiBase CurrentGui => materialGuis[selectedCriterion];
        private int selectedCriterion;
        private readonly string[] criterionOptions = { "地物型" , "属性情報" };
        private MaterialCriterionGuiBase[] materialGuis = { new MaterialCriterionTypeGui(), new MaterialCriterionAttrGui() };

        public CityMaterialAdjustGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        public void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Array.Clear(selectedObjs, 0, selectedObjs.Length);
        }

        private void OnSelectionChanged()
        {
            if (isTargetDetermined) return; // 「検索」ボタンを押したら対象は変更できないようにします。
            selectedObjs = Selection.gameObjects;
            parentEditorWindow.Repaint();
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");

            DisplaySelectedObjects();
            DisplayClassificationChoice();
            
            CurrentGui.DrawBeforeTargetSelect();

            DisplayCityObjTypeSearchButton();

            if (!isTargetDetermined) return;

            // 検索後にのみ以下を表示します
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var granularity = GranularityGUI.Draw("粒度", CurrentGui.GetGranularity());
                destroyOrPreserveSrcGUI.Draw();
                bool doDestroySrcObjects = destroyOrPreserveSrcGUI.Current ==
                                               DestroyOrPreserveSrcGUI.PreserveOrDestroy.Destroy;
                CurrentGui.SetConfig(granularity, doDestroySrcObjects);
            }
            CurrentGui.DrawAfterTargetSelect();
            
        }

        private void DisplaySelectedObjects()
        {
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in selectedObjs)
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

        private void DisplayClassificationChoice()
        {
            PlateauEditorStyle.Heading("マテリアル分類", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 50;
                selectedCriterion = EditorGUILayout.Popup("分類", selectedCriterion, criterionOptions);
            }

            ;
        }

        private void DisplayCityObjTypeSearchButton()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(0, 0, 15, 0))
            {
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    if (!isTargetDetermined)
                    {
                        if (PlateauEditorStyle.MiniButton("検索", 150))
                        {
                            using var progressBar = new ProgressBar("検索中です...");
                            progressBar.Display(0.4f);
                            bool searchSucceed = CurrentGui.Search(selectedObjs);
                            isTargetDetermined = searchSucceed;
                            parentEditorWindow.Repaint();
                        }
                    }
                    else
                    {
                        if (PlateauEditorStyle.MiniButton("再選択", 150))
                        {
                            isTargetDetermined = false;
                            OnSelectionChanged();
                        }
                    }
                });
            }
        }

        
    }
}