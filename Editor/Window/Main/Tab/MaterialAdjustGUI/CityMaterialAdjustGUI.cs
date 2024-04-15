using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityMaterialAdjustGUI : IEditorDrawable
    {
        private readonly ObjectSelectGui objectSelectGui;
        
        private readonly DestroyOrPreserveSrcGUI destroyOrPreserveSrcGUI = new();
        private string attrKey = "";

        private readonly MaterialCriterionGui materialCriterionGui = new MaterialCriterionGui();
        public MaterialAdjustByCriterion CurrentAdjuster => materialCriterionGui.CurrentAdjuster;
        private MaterialCriterion SelectedCriterion => materialCriterionGui.SelectedCriterion;
        private MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        private GameObject[] SelectedObjs => objectSelectGui.SelectedObjs;
        private bool doDestroySrcObjs;
        private readonly EditorWindow parentEditorWindow;
        
        

        public CityMaterialAdjustGUI(EditorWindow parentEditorWindow)
        {
            objectSelectGui = new ObjectSelectGui(this, parentEditorWindow);
            this.parentEditorWindow = parentEditorWindow;
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");

            objectSelectGui.Draw();
            materialCriterionGui.Draw();

            if (SelectedCriterion == MaterialCriterion.ByAttribute)
            {
                using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
                {
                    EditorGUIUtility.labelWidth = 100;
                    attrKey = EditorGUILayout.TextField("属性情報キー", attrKey);
                }
            }

            DisplayCityObjTypeSearchButton();

            if (!CurrentAdjuster.IsSearched) return;

            // 検索後にのみ以下を表示します
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                meshGranularity = GranularityGUI.Draw("粒度", meshGranularity);
                destroyOrPreserveSrcGUI.Draw();
                doDestroySrcObjs = destroyOrPreserveSrcGUI.Current ==
                                               DestroyOrPreserveSrcGUI.PreserveOrDestroy.Destroy;
            }
            MaterialConfGui.Draw(CurrentAdjuster.MaterialAdjustConf);

            PlateauEditorStyle.Separator(0);

            if (PlateauEditorStyle.MainButton("実行"))
            {
                var executorConf = new AdjustExecutorConf(
                    CurrentAdjuster.MaterialAdjustConf,
                    objectSelectGui.SelectedObjs,
                    meshGranularity,
                    doDestroySrcObjs);
                CurrentAdjuster.AdjustExecutor.Exec(executorConf).ContinueWithErrorCatch(); // ここで実行します。
            }
            
        }

        /// <summary>
        /// 検索ボタンを表示します。
        /// </summary>
        private void DisplayCityObjTypeSearchButton()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(0, 0, 15, 0))
            {
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    if (!CurrentAdjuster.IsSearched)
                    {
                        if (PlateauEditorStyle.MiniButton("検索", 150))
                        {
                            using var progressBar = new ProgressBar("検索中です...");
                            progressBar.Display(0.4f);
                            SearchArg searchArg = SelectedCriterion switch
                            {
                                MaterialCriterion.ByType => new SearchArg(SelectedObjs),
                                MaterialCriterion.ByAttribute => new SearchArgByArr(SelectedObjs, attrKey),
                                _ => throw new ArgumentOutOfRangeException()
                            };
                            bool searchSucceed = CurrentAdjuster.Search(searchArg);
                            CurrentAdjuster.IsSearched = searchSucceed;
                            parentEditorWindow.Repaint();
                        }
                    }
                    else
                    {
                        if (PlateauEditorStyle.MiniButton("再選択", 150))
                        {
                            CurrentAdjuster.IsSearched = false;
                            objectSelectGui.OnSelectionChanged();
                        }
                    }
                });
            }
        }

        public void Dispose()
        {
            objectSelectGui.Dispose();
        }
    }
}