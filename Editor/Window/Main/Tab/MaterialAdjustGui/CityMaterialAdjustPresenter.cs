using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Linq;
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
        private ConvertGranularity granularity;
        private bool doChangeGranularity;
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
                new ElementGroup("", 0,
                    new HeaderElementGroup("", "分類に応じたマテリアル分けを行います。", HeaderType.Subtitle),
                    new ElementGroup("MAGuiBeforeSearch",0, // *** 検索前のUI ***
                        new ObjectSelectGui(OnTargetChanged), // 対象オブジェクト選択
                        new HeaderElementGroup("commonConf", "共通設定", HeaderType.Header,
                            new DestroyOrPreserveSrcGui(OnPreserveOrDestroyChanged) // 元のオブジェクトを削除するか残すか
                        ),
                        new HeaderElementGroup("", "分割/結合", HeaderType.Header,
                            new ConvertGranularityGui(OnMAGranularityChanged, OnDoChangeGranularityChanged) // 分割結合に関する一般設定
                        ),
                        new HeaderElementGroup("", "マテリアル分け", HeaderType.Header,
                            new ToggleLeftElement("doMaterialAdjust", "マテリアルを条件指定で変更する", false, OnDoMaterialAdjustChanged),
                            new ElementGroup("materialConf", 0,
                                new MaterialCriterionGui(OnCriterionChanged), // マテリアル分け基準選択
                                new AttributeKeyGui(parentEditorWindow, ()=>SelectedObjects, OnAttrKeyChanged), // 属性情報キー選択
                                new ButtonElement("", "検索", new Vector4(0,0,15,0), OnSearchButtonPushed) // 検索ボタン
                            )
                        )
                    ),
                    new ElementGroup("MAGuiAfterSearch",0, // *** 検索後のUI ***
                        new MaterialConfGui(() => CurrentSearcher?.MaterialAdjustConf), // マテリアル設定,
                        new ButtonElement("execMaterialAdjustButton", "マテリアル分けを実行", ExecMaterialAdjust)
                    ),
                    new ButtonElement("execGranularityConvertButton", "粒度変更を実行", ExecGranularityConvert)
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
            Views.Get<ButtonElement>().IsVisible = !doMaterialAdjust; // マテリアル分けをせず粒度変更のみのとき表示するボタン
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

        private void OnMAGranularityChanged(ConvertGranularity granularity)
        {
            this.granularity = granularity;
        }

        private void OnDoChangeGranularityChanged(bool doChangeGranularityArg)
        {
            this.doChangeGranularity = doChangeGranularityArg;
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
            var button = Views.Get<ButtonElement>("execMaterialAdjustButton");
            button.SetProcessing("マテリアル分け中...");
            parentEditorWindow.Repaint();
            try
            {
                ExecMaterialAdjustAsync().ContinueWith(_ => button.RecoverFromProcessing()).ContinueWithErrorCatch();
            }
            catch (Exception e)
            {
                Debug.LogError("マテリアル分けに失敗しました。\n" + e);
            }
        }

        private async Task ExecMaterialAdjustAsync()
        {
            var conf = GenerateConf();
            // 分割結合
            if (doChangeGranularity)
            {
                var result = await ExecGranularityConvertAsync();
                // 次のマテリアル分けの設定値を差し替え
                conf.DoDestroySrcObjs = true;
                conf.TargetTransforms = result.GeneratedRootTransforms;
            }

            // マテリアル分け
            await ExecMaterialAdjustAsync(conf);
        }

        private async Task ExecMaterialAdjustAsync(MAExecutorConf conf)
        {
            await Task.Delay(100); // ボタン押下時のGUIの更新を反映させるために1フレーム以上待つ必要があります。
            await Task.Yield();
            
            IMAExecutorV2 maExecutor = selectedCriterion switch
            {
                MaterialCriterion.ByType => new MAExecutorV2ByType(),
                MaterialCriterion.ByAttribute => new MAExecutorV2ByAttr(),
                _ => throw new Exception("Unknown Criterion")
            };
            // ここで実行します。
            var result = await maExecutor.ExecAsync(conf);

            // 完了後、GUIの選択オブジェクトを完了後に差し替えます
            if (DoDestroySrcObjs)
            {
                clearSearchOnTargetChange = false;
                Views.Get<ObjectSelectGui>().ForceSet(result);
                clearSearchOnTargetChange = true;
                if (SelectedObjects.Count == 0) IsSearched = false;
            }
        }

        /// <summary>
        /// マテリアル分けをせず、分割結合のみ行う場合に呼びます。
        /// </summary>
        private void ExecGranularityConvert()
        {
            var button = Views.Get<ButtonElement>("execGranularityConvertButton");
            button.SetProcessing("粒度変更中...");
            parentEditorWindow.Repaint();
            try
            {
                // ここで実行します。
                ExecGranularityConvertAsync().ContinueWith((_) => button.RecoverFromProcessing()).ContinueWithErrorCatch();
            }
            catch (Exception e)
            {
                Debug.LogError("粒度変更に失敗しました。\n" + e);
            }
        }
        

        private async Task<PlaceToSceneResult> ExecGranularityConvertAsync()
        {
            await Task.Delay(100); // ボタン押下時のGUIの更新を反映させるために1フレーム以上待つ必要があります。
            await Task.Yield();
            
            var conf = GenerateConf();
            if (conf.TargetTransforms.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
            }
            return await new CityGranularityConverter().ConvertAsync(new GranularityConvertOptionUnity(new GranularityConvertOption(granularity, 1), conf.TargetTransforms, conf.DoDestroySrcObjs));
        }

        /// <summary>
        /// GUIをもとに設定値インスタンスを生成します。
        /// </summary>
        private MAExecutorConf GenerateConf()
        {
            
            return selectedCriterion switch
            {
                MaterialCriterion.None => throw new Exception("分割結合のみのときはCityGranularityConverterを使ってください"),
                
                MaterialCriterion.ByType => new MAExecutorConf(
                    CurrentSearcher.MaterialAdjustConf,
                    SelectedObjects,
                    DoDestroySrcObjs,
                    true
                ),
                    
                MaterialCriterion.ByAttribute => new MAExecutorConfByAttr(
                    CurrentSearcher.MaterialAdjustConf,
                    SelectedObjects,
                    DoDestroySrcObjs,
                    true,
                    attrKey
                ),
                    
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }
}