using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts;
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
        private string[] typeOptions = { "地物型" /*, "属性情報"*/ };
        private string attrKey = "";
        private readonly DestroyOrPreserveSrcGUI destroyOrPreserveSrcGUI = new();

        private CityMaterialAdjuster adjuster; // 「検索」ボタンを押すまでこれはnullになります。

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
            if (adjuster != null) return; // 「検索」ボタンを押したら対象は変更できないようにします。
            selectedObjs = Selection.gameObjects;
            parentEditorWindow.Repaint();
        }

        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");

            DisplaySelectedObjects();
            DisplayClassificationChoice();

            if (selectedType == 1)
            {
                using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
                {
                    EditorGUIUtility.labelWidth = 100;
                    attrKey = EditorGUILayout.TextField("属性情報キー", attrKey);
                }
            }

            DisplayCityObjTypeSearchButton();

            if (adjuster == null) return;

            // 検索後にのみ以下を表示します

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                adjuster.granularity = GranularityGUI.Draw("粒度", adjuster.granularity);
                destroyOrPreserveSrcGUI.Draw();
                adjuster.DoDestroySrcObjects = destroyOrPreserveSrcGUI.Current ==
                                               DestroyOrPreserveSrcGUI.PreserveOrDestroy.Destroy;
            }

            DisplayCityObjectTypeMaterialConfGUI();


            PlateauEditorStyle.Separator(0);

            if (PlateauEditorStyle.MainButton("実行"))
            {
                adjuster.Exec().ContinueWithErrorCatch(); // ここで実行します。
            }
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
                this.selectedType = EditorGUILayout.Popup("分類", this.selectedType, typeOptions);
            }

            ;
        }

        private void DisplayCityObjTypeSearchButton()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(0, 0, 15, 0))
            {
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    if (adjuster == null)
                    {
                        if (PlateauEditorStyle.MiniButton("検索", 150))
                        {
                            using var progressBar = new ProgressBar("検索中です...");
                            progressBar.Display(0.4f);
                            adjuster = new CityMaterialAdjuster(selectedObjs); // ここで検索します。
                            if (adjuster.MaterialAdjustConf.Length <= 0)
                            {
                                Dialogue.Display("地物型が見つかりませんでした。\n属性情報を含む都市オブジェクトかその親を選択してください。", "OK");
                                adjuster = null;
                            }

                            parentEditorWindow.Repaint();
                        }
                    }
                    else
                    {
                        if (PlateauEditorStyle.MiniButton("再選択", 150))
                        {
                            adjuster = null;
                            OnSelectionChanged();
                        }
                    }
                });
            }
        }

        private void DisplayCityObjectTypeMaterialConfGUI()
        {
            var conf = adjuster.MaterialAdjustConf;
            int displayIndex = 1;

            // 存在する地物型を列挙します 
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
        }
    }
}