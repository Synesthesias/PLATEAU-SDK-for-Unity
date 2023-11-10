using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;
using Material = UnityEngine.Material;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityMaterialAdjustGUI : IEditorDrawable
    {
        private UnityEditor.EditorWindow parentEditorWindow;
        private GameObject[] selectedObjs = new GameObject[0];
        private Vector2 scrollSelected;
        private int selectedType;
        private string[] typeOptions = { "地物型"/*, "属性情報"*/ };
        private string attrKey = "";

        private CityMaterialAdjuster adjuster;

        public CityMaterialAdjustGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public void Dispose()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Array.Clear(selectedObjs, 0, selectedObjs.Length);
        }

        private void OnSelectionChanged()
        {
            //選択アイテムのフィルタリング処理
            //Selected = Selection.gameObjects.Where(x => x.GetComponent<PLATEAUCityObjectGroup>() != null).ToArray<GameObject>();
            selectedObjs = Selection.gameObjects;
            parentEditorWindow.Repaint();
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollSelected = EditorGUILayout.BeginScrollView(scrollSelected, GUILayout.MaxHeight(100));
                foreach (GameObject obj in selectedObjs)
                {
                    EditorGUILayout.LabelField(obj.name);
                }
                EditorGUILayout.EndScrollView();
            }

            PlateauEditorStyle.Heading("マテリアル分類", null);

            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 50;
                this.selectedType = EditorGUILayout.Popup("分類", this.selectedType, typeOptions);
            }

            if(selectedType == 1)
            {
                using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
                {
                    EditorGUIUtility.labelWidth = 100;
                    attrKey = EditorGUILayout.TextField("属性情報キー", attrKey);
                }
            }

            using (PlateauEditorStyle.VerticalScopeWithPadding(0, 0, 15, 0))
            {
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    if (PlateauEditorStyle.MiniButton("検索", 150))
                    {
                        using var progressBar = new ProgressBar("検索中です...");
                        progressBar.Display(0.4f);
                        adjuster = new CityMaterialAdjuster(selectedObjs);
                        parentEditorWindow.Repaint();
                    }
                });
            }

            if (adjuster == null) return;
            
            // 検索後にのみ以下を表示します

            var conf = adjuster.MaterialAdjustConf;
            int displayIndex = 1;
            foreach (var (typeNode, typeConf) in conf)
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.CategoryTitle(
                        $"地物型{displayIndex} : {typeNode.GetDisplayName()}");
                    typeConf.ChangeMaterial = EditorGUILayout.ToggleLeft("マテリアルを変更する", typeConf.ChangeMaterial);
                    if (typeConf.ChangeMaterial)
                    {
                        typeConf.Material = (Material)EditorGUILayout.ObjectField("マテリアル",
                            typeConf.Material, typeof(Material), false);
                    }
                }
                displayIndex++;
            }

            PlateauEditorStyle.Separator(0);
            
                if (PlateauEditorStyle.MainButton("実行"))
                {
                    adjuster.Exec().ContinueWithErrorCatch();
                }
        }

    }
}
