﻿using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIのPresenter層です。
    /// </summary>
    internal class CityMaterialAdjustPresenter : ITabContent
    {
        private ElementGroup Views { get; }
        
        // マテリアル分けの基準が属性情報の場合と地物型の場合で、2つのインスタンスを用意します。
        private readonly MAKeySearcher materialGuiByType = new MAKeySearcher(MaterialCriterion.ByType);
        private readonly MAKeySearcher materialGuiByAttr = new MAKeySearcher(MaterialCriterion.ByAttribute);
        
        
        private UniqueParentTransformList SelectedObjects { get; set; } // ここでsetしてもGUIに反映されないので注意

        private PreserveOrDestroy preserveOrDestroy;
        private bool DoDestroySrcObjs => preserveOrDestroy == PreserveOrDestroy.Destroy;
        private MAGranularity maGranularity;
        private bool doMaterialAdjust;
        private MaterialCriterion selectedCriterion;
        private string attrKey;
        private readonly EditorWindow parentEditorWindow;
        /// <summary>
        /// 選択中のマテリアル分け基準に応じた、マテリアル分けインスタンスを返します。
        /// </summary>
        private MAKeySearcher CurrentSearcher
        {
            get
            {
                return selectedCriterion switch
                {
                    MaterialCriterion.ByType => materialGuiByType,
                    MaterialCriterion.ByAttribute => materialGuiByAttr,
                    MaterialCriterion.None => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        /// マテリアル分けの選択画面を出すために、利用可能な分類項目を検索したかどうかです。
        /// falseにすることで検索をリセットできます。
        /// </summary>
        private bool IsSearched
        {
            get => CurrentSearcher?.IsSearched ?? false;
            set
            {
                CurrentSearcher.IsSearched = value;
                Views.Get<ObjectSelectGui>().LockChange = value;
            }
        }


        public CityMaterialAdjustPresenter(EditorWindow parentEditorWindow)
        {
            // UIの内容とコールバックをここで定義します。
            Views =
                new ElementGroup("",
                    new HeaderElementGroup("", "分類に応じたマテリアル分けを行います。", HeaderType.Subtitle),
                    new ElementGroup("MAGuiBeforeSearch", // *** 検索前のUI ***
                        new ObjectSelectGui(OnTargetChanged), // 対象オブジェクト選択
                        new HeaderElementGroup("commonConf", "共通設定", HeaderType.Header,
                            new DestroyOrPreserveSrcGui(OnPreserveOrDestroyChanged) // 元のオブジェクトを削除するか残すか
                        ),
                        new HeaderElementGroup("", "分割/結合", HeaderType.Header,
                            new MAGranularityGui(OnMAGranularityChanged) // 分割結合に関する一般設定
                        ),
                        new HeaderElementGroup("", "マテリアル分け", HeaderType.Header,
                            new ToggleLeftElement("doMaterialAdjust", "マテリアルを条件指定で変更する", false, OnDoMaterialAdjustChanged),
                            new ElementGroup("materialConf", 
                                new MaterialCriterionGui(OnCriterionChanged), // マテリアル分け基準選択
                                new AttributeKeyGui(parentEditorWindow, ()=>SelectedObjects, OnAttrKeyChanged), // 属性情報キー選択
                                new ButtonElement("", "検索", new Vector4(0,0,15,0), OnSearchButtonPushed) // 検索ボタン
                            )
                        )
                    ),
                    new ElementGroup("MAGuiAfterSearch", // *** 検索後のUI ***
                        new MaterialConfGui(() => CurrentSearcher?.MaterialAdjustConf), // マテリアル設定,
                        new ButtonElement("", "マテリアル分けを実行", ExecMaterialAdjust)
                    ),
                    new ButtonElement("", "粒度変更を実行", ExecGranularityConvert)
                );
            this.parentEditorWindow = parentEditorWindow;
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        public void Draw()
        {
            // 描画メイン
            Views.Draw();
            
            // 不要な設定項目を隠します
            Views.Get<AttributeKeyGui>().IsVisible = selectedCriterion == MaterialCriterion.ByAttribute; // 属性情報による分類をするときのみ属性情報キー入力欄を表示
            Views.Get<ButtonElement>().IsVisible = !doMaterialAdjust &&
                                                  maGranularity != MAGranularity.DoNotChange;// マテリアル分けをせず粒度変更のみのとき表示するボタン
            Views.Get("MAGuiAfterSearch").IsVisible = IsSearched && doMaterialAdjust;
            Views.Get("materialConf").IsVisible = doMaterialAdjust;
        }
        
        /// <summary> 対象が変わった時に検索結果をリセットするか </summary>
        private bool clearSearchOnTargetChange = true;
        
        /// <summary> 対象オブジェクトの選択を変えた時に呼ばれます。 </summary>
        private void OnTargetChanged(UniqueParentTransformList nextTarget)
        {
            if (clearSearchOnTargetChange)
            {
                if(CurrentSearcher != null) IsSearched = false;
            }
            SelectedObjects = nextTarget;
        }

        private void OnPreserveOrDestroyChanged(PreserveOrDestroy pod)
        {
            preserveOrDestroy = pod;
        }

        private void OnMAGranularityChanged(MAGranularity granularity)
        {
            maGranularity = granularity;
        }

        private void OnDoMaterialAdjustChanged(bool doMaterialAdjustArg)
        {
            doMaterialAdjust = doMaterialAdjustArg;
        }

        private void OnCriterionChanged(MaterialCriterion criterion)
        {
            selectedCriterion = criterion; 
        }
        
        private void OnAttrKeyChanged(string key)
        {
            attrKey = key;
        }

        private void OnSearchButtonPushed()
        {
            using var progressBar = new ProgressBar("検索中です...");
            progressBar.Display(0.4f);
            SearchArg searchArg = selectedCriterion switch
            {
                MaterialCriterion.ByType => new SearchArg(SelectedObjects),
                MaterialCriterion.ByAttribute => new SearchArgByArr(SelectedObjects, attrKey),
                _ => throw new ArgumentOutOfRangeException()
            };
            bool searchSucceed = CurrentSearcher.Search(searchArg);
            IsSearched = searchSucceed;
            parentEditorWindow.Repaint();
        }

        public void Dispose()
        {
            Views.Dispose();
        }
        
        public void OnTabUnselect()
        {
        }
        
        private void ExecMaterialAdjust()
        {
            ExecMaterialAdjustAsync().ContinueWithErrorCatch();
        }

        private async Task ExecMaterialAdjustAsync()
        {
            var executor = GenerateExecutor();
            // 実行メイン
            var result = await executor.Exec();
            
            // 完了後、GUIの選択オブジェクトを完了後に差し替えます
            if (DoDestroySrcObjs)
            {
                clearSearchOnTargetChange = false;
                Views.Get<ObjectSelectGui>().ForceSet(result);
                clearSearchOnTargetChange = true;
                if(SelectedObjects.Count == 0) IsSearched = false;
            }
        }

        /// <summary>
        /// マテリアル分けをせず、分割結合のみ行う場合に呼びます。
        /// </summary>
        private void ExecGranularityConvert()
        {
            ExecGranularityConvertAsync().ContinueWithErrorCatch();
        }

        private async Task ExecGranularityConvertAsync()
        {
            var conf = GenerateConf();
            if (conf.TargetTransforms.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
            }
            await new CityGranularityConverter().ConvertProgressiveAsync(conf, new MAConditionSimple());
        }

        /// <summary>
        /// GUIをもとに設定値インスタンスを生成します。
        /// </summary>
        private MAExecutorConf GenerateConf()
        {
            var granularity = maGranularity;
            
            return selectedCriterion switch
            {
                MaterialCriterion.None => throw new Exception("分割結合のみのときはCityGranularityConverterを使ってください"),
                
                MaterialCriterion.ByType => new MAExecutorConf(
                    CurrentSearcher.MaterialAdjustConf,
                    SelectedObjects,
                    granularity,
                    DoDestroySrcObjs,
                    true
                ),
                    
                MaterialCriterion.ByAttribute => new MAExecutorConfByAttr(
                    CurrentSearcher.MaterialAdjustConf,
                    SelectedObjects,
                    granularity,
                    DoDestroySrcObjs,
                    true,
                    attrKey
                ),
                    
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private MAExecutor GenerateExecutor()
        {
            var executorConf = GenerateConf();
            return selectedCriterion switch
            {
                MaterialCriterion.None => MAExecutorFactory.CreateGranularityExecutor(executorConf),
                MaterialCriterion.ByType => MAExecutorFactory.CreateTypeExecutor(executorConf),
                MaterialCriterion.ByAttribute => MAExecutorFactory.CreateAttrExecutor((MAExecutorConfByAttr)executorConf),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}