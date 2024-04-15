using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts;
using PLATEAU.PolygonMesh;
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
        private readonly SearchButton searchButton;
        public MaterialAdjustByCriterion CurrentAdjuster => materialCriterionGui.CurrentAdjuster;
        private MaterialCriterion SelectedCriterion => materialCriterionGui.SelectedCriterion;
        private MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        private GameObject[] SelectedObjs => objectSelectGui.SelectedObjs;

        public bool IsSearched
        {
            get => CurrentAdjuster.IsSearched;
            set => CurrentAdjuster.IsSearched = value;
        }

        private bool doDestroySrcObjs;
        
        

        public CityMaterialAdjustGUI(EditorWindow parentEditorWindow)
        {
            objectSelectGui = new ObjectSelectGui(this, parentEditorWindow);
            searchButton = new SearchButton(this, parentEditorWindow);
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("分類に応じたマテリアル分けを行います。");

            objectSelectGui.Draw(); // 選択オブジェクトの表示
            materialCriterionGui.Draw(); // マテリアル分け基準の選択

            // 属性情報キーの入力
            if (SelectedCriterion == MaterialCriterion.ByAttribute)
            {
                using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
                {
                    EditorGUIUtility.labelWidth = 100;
                    attrKey = EditorGUILayout.TextField("属性情報キー", attrKey);
                }
            }

            // 検索ボタンの描画
            searchButton.Draw(CurrentAdjuster);

            if (!IsSearched) return;

            // 検索後にのみ以下を表示します。
            
            // 全般的な設定
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                meshGranularity = GranularityGUI.Draw("粒度", meshGranularity);
                destroyOrPreserveSrcGUI.Draw();
                doDestroySrcObjs = destroyOrPreserveSrcGUI.Current ==
                                               DestroyOrPreserveSrcGUI.PreserveOrDestroy.Destroy;
            }
            
            // 各分類キーごとのマテリアル設定 
            MaterialConfGui.Draw(CurrentAdjuster.MaterialAdjustConf);

            PlateauEditorStyle.Separator(0);

            // 実行ボタン
            if (PlateauEditorStyle.MainButton("実行"))
            {
                var executorConf = SelectedCriterion switch
                {
                    MaterialCriterion.ByType => new AdjustExecutorConf(
                        CurrentAdjuster.MaterialAdjustConf,
                        objectSelectGui.SelectedObjs,
                        meshGranularity,
                        doDestroySrcObjs),
                    
                    MaterialCriterion.ByAttribute => new AdjustExecutorConfByAttr(
                        CurrentAdjuster.MaterialAdjustConf,
                        objectSelectGui.SelectedObjs,
                        meshGranularity,
                        doDestroySrcObjs,
                        attrKey),
                    
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                CurrentAdjuster.AdjustExecutor.Exec(executorConf).ContinueWithErrorCatch(); // ここで実行します。
            }
            
        }

        /// <summary>
        /// 現在のGUIの設定で検索条件を生成します。
        /// </summary>
        public SearchArg GenerateSearchArg()
        {
            SearchArg searchArg = SelectedCriterion switch
            {
                MaterialCriterion.ByType => new SearchArg(SelectedObjs),
                MaterialCriterion.ByAttribute => new SearchArgByArr(SelectedObjs, attrKey),
                _ => throw new ArgumentOutOfRangeException()
            };
            return searchArg;
        }

        public void UpdateObjectSelection()
        {
            objectSelectGui.UpdateSelection();
        }

        public void Dispose()
        {
            objectSelectGui.Dispose();
        }
    }
}