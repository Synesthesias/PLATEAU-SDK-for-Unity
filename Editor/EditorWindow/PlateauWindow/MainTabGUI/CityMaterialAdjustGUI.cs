using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityMaterialAdjustGUI : IEditorDrawable
    {
        private UnityEditor.EditorWindow parentEditorWindow;
        private GameObject[] Selected = new GameObject[0];
        private Vector2 scrollSelected;
        private int selectedType;
        private string[] typeOptions = { "属性情報", "地物型" };
        private string attrKey = "";
        private bool changeMat1 = false;
        private bool changeMat2 = false;
        private Material mat1 = null;
        private Material mat2 = null;
        private bool isSearchTaskRunning = false;
        private bool isExecTaskRunning = false;

        private void OnSelectionChanged()
        {
            //Selected = Selection.gameObjects.Where(x => x.GetComponent<PLATEAUCityObjectGroup>() != null).ToArray<GameObject>();
            Selected = Selection.gameObjects;
            parentEditorWindow.Repaint();
        }

        public void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        public CityMaterialAdjustGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in Selected)
                {
                    EditorGUILayout.LabelField(obj.name);
                }
                EditorGUILayout.EndScrollView();
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.Heading("マテリアル分類", null);

                using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 8))
                {
                    EditorGUIUtility.labelWidth = 50;
                    this.selectedType = EditorGUILayout.Popup("分類", this.selectedType, typeOptions);
                }

                EditorGUIUtility.labelWidth = 100;
                attrKey = EditorGUILayout.TextField("属性情報キー", attrKey);
                

                using (PlateauEditorStyle.VerticalScopeWithPadding(0, 0, 15, 0))
                {
                    PlateauEditorStyle.CenterAlignHorizontal(() =>
                    {
                        using (new EditorGUI.DisabledScope(isSearchTaskRunning))
                        {
                            if (PlateauEditorStyle.MiniButton(isSearchTaskRunning ? "処理中..." : "検索", 150))
                            {
                                //isSearchTaskRunning = true;
                                //検索処理
                            }
                        }
                    });
                }
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.CategoryTitle("属性情報①");
                using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 0))
                {
                    changeMat2 = EditorGUILayout.ToggleLeft("マテリアルを変更する", changeMat1);
                    mat1 = (Material)EditorGUILayout.ObjectField("マテリアル",
                                    mat1, typeof(Material), false);
                }    
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                PlateauEditorStyle.CategoryTitle("属性情報②");
                using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 0))
                {
                    changeMat2 = EditorGUILayout.ToggleLeft("マテリアルを変更する", changeMat2);
                    mat2 = (Material)EditorGUILayout.ObjectField("マテリアル",
                                    mat2, typeof(Material), false);
                }
            }

            PlateauEditorStyle.Separator(0);

            using (new EditorGUI.DisabledScope(isExecTaskRunning))
            {
                if (PlateauEditorStyle.MainButton(isExecTaskRunning ? "処理中..." : "実行"))
                {
                    //isExecTaskRunning = true;
                    //実行処理
                }
            }
        }

    }
}
