using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
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
using UnityEngine.UIElements;
using ProgressBar = PLATEAU.Util.ProgressBar;
using static PLATEAU.Editor.Window.Common.SceneTileChooserGui;

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
           
        private UniqueParentTransformList SelectedObjects { get; set; } // ここでsetしてもGUIに反映されないので注意です。set時にはForceSetTargetObjectsを使ってください。

        private PreserveOrDestroy preserveOrDestroy;
        private bool DoDestroySrcObjs => preserveOrDestroy == PreserveOrDestroy.Destroy;
        private ConvertGranularity granularity;
        private bool doChangeGranularity;
        private bool doMaterialAdjust;
        private MaterialCriterion selectedCriterion;
        private string attrKey;
        private readonly EditorWindow parentEditorWindow;
        private TileMaterialAdjustGui tileConvert;

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
        /// シーンオブジェクト/動的タイルの現在選択されているタイプを返します。
        /// </summary>
        private ChooserType CurrentSceneTileSelectType => Views.Get<ObjectSelectGui>()?.SelectedType ?? ChooserType.SceneObject;

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
            tileConvert = new TileMaterialAdjustGui(parentEditorWindow);

            // UIの内容とコールバックをここで定義します。
            Views =
                new ElementGroup("", 0,
                    new HeaderElementGroup("", "分類に応じたマテリアル分けを行います。", HeaderType.Subtitle),
                    new ElementGroup("MAGuiBeforeSearch",0, // *** 検索前のUI ***
                        new ObjectSelectGui(OnTargetChanged, new SceneTileChooserGui(onSceneTileSelectionChanged), tileConvert), // 対象オブジェクト選択
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
                                new AttributeKeyGui(parentEditorWindow, GetSelectedObjectsAsync, OnAttrKeyChanged), // 属性情報キー選択
                                new ButtonElement("", "検索", new Vector4(0,0,15,0), () => OnSearchButtonPushedAsync().ContinueWithErrorCatch()) // 検索ボタン
                            )
                        )
                    ),
                    new ElementGroup("MAGuiAfterSearch", 0, // *** 検索後のUI ***
                        new MaterialConfGui(() => CurrentSearcher?.MaterialAdjustConf), // マテリアル設定,
                        new ButtonElement("execMaterialAdjustButton", "マテリアル分けを実行", () => ExecMaterialAdjustAsync().ContinueWithErrorCatch())
                    ),
                    new ButtonElement("execGranularityConvertButton", "粒度変更を実行", () => ExecGranularityConvertAsync().ContinueWithErrorCatch())
                );
            this.parentEditorWindow = parentEditorWindow;

            ResetGui();
        }
        
        public VisualElement CreateGui()
        {
            return new IMGUIContainer(Draw);
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        private void Draw() => Views.Draw();

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

        /// <summary>変更通知（元のオブジェクトを削除するか残すか）</summary>
        private void OnPreserveOrDestroyChanged(PreserveOrDestroy pod)
        {
            preserveOrDestroy = pod;
        }

        /// <summary>変更通知（分割結合チェック）</summary>
        private void OnMAGranularityChanged(ConvertGranularity granularityArg)
        {
            this.granularity = granularityArg;
        }

        /// <summary>変更通知（分割結合粒度変更）</summary>
        private void OnDoChangeGranularityChanged(bool doChangeGranularityArg)
        {
            this.doChangeGranularity = doChangeGranularityArg;

            ShowGranularityConvertButton(doChangeGranularity && !doMaterialAdjust);
        }

        /// <summary>変更通知（マテリアル分けチェック）</summary>
        private void OnDoMaterialAdjustChanged(bool doMaterialAdjustArg)
        {
            doMaterialAdjust = doMaterialAdjustArg;
            ShowGuiAfterSearch(doMaterialAdjust && IsSearched);
            ShowMaterialConf(doMaterialAdjust);
            ShowGranularityConvertButton(doChangeGranularity && !doMaterialAdjust);
        }

        /// <summary>変更通知（マテリアル分け分類変更）</summary>
        private void OnCriterionChanged(MaterialCriterion criterion)
        {
            selectedCriterion = criterion;

            ShowAttributeKey(selectedCriterion == MaterialCriterion.ByAttribute);
            ShowGuiAfterSearch(false);
        }

        /// <summary>変更通知（マテリアル分け属性情報キー変更）</summary>
        private void OnAttrKeyChanged(string key)
        {
            attrKey = key;
        }

        private void onSceneTileSelectionChanged(SceneTileChooserGui.ChooserType selectedType)
        {
            ResetGui();
        }

        /// <summary>表示・非表示 （マテリアル分け　地物・属性検索）</summary>
        private void ShowMaterialConf(bool show)
        {
            if (Views?.Get("materialConf") != null)
                Views.Get("materialConf").IsVisible = show;

            if (Views?.Get<MaterialConfGui>() != null)
                Views.Get<MaterialConfGui>().IsVisible = show;
        }

        /// <summary>表示・非表示 （マテリアル分け 属性情報キー）</summary>
        private void ShowAttributeKey(bool show)
        {
            if (Views?.Get<AttributeKeyGui>() != null)
                Views.Get<AttributeKeyGui>().IsVisible = show;
        }

        /// <summary>表示・非表示 （検索後のUI）</summary>
        private void ShowGuiAfterSearch(bool show)
        {
            if (Views?.Get("MAGuiAfterSearch") != null)
                Views.Get("MAGuiAfterSearch").IsVisible = show;
        }

        /// <summary>表示・非表示 （マテリアル分けをせず粒度変更のみのとき表示するボタン）</summary>
        private void ShowGranularityConvertButton(bool show)
        {
            if (Views?.Get("execGranularityConvertButton") != null)
                Views.Get("execGranularityConvertButton").IsVisible = show;
        }

        /// <summary>
        /// UIを初期状態にリセットします。
        /// </summary>
        private void ResetGui()
        {
            ShowGuiAfterSearch(false);
            ShowMaterialConf(false);
            ShowAttributeKey(false);
            ShowGranularityConvertButton(false);

            tileConvert?.Reset();
            Views?.Reset();
            IsSearched = false;
        }

        /// <summary>
        /// 変換処理終了時の後処理を行います。
        /// </summary>
        private void PostConvert()
        {
            ResetGui();
        }

        private async Task<UniqueParentTransformList> GetSelectedObjectsAsync()
        {
            if (CurrentSceneTileSelectType == ChooserType.DynamicTile) // 動的タイルを選択しているとき
            {
                return await tileConvert.GetSelectionAsTransformList();
            }
            return await Task.FromResult(SelectedObjects);
        }

        private async Task OnSearchButtonPushedAsync()
        {
            using var progressBar = new ProgressBar("検索中です...");
            progressBar.Display(0.4f);

            ShowGuiAfterSearch(false);

            if (CurrentSceneTileSelectType == ChooserType.DynamicTile) // 動的タイルを選択しているとき
            {
                SelectedObjects = await tileConvert.GetSelectionAsTransformList();
                await Task.Yield();
            }

            progressBar.Display(0.6f);

            SearchArg searchArg = selectedCriterion switch
            {
                MaterialCriterion.ByType => new SearchArg(SelectedObjects),
                MaterialCriterion.ByAttribute => new SearchArgByArr(SelectedObjects, attrKey),
                _ => throw new ArgumentOutOfRangeException()
            };
            bool searchSucceed = CurrentSearcher.Search(searchArg);
            IsSearched = searchSucceed;

            progressBar.Display(0.8f);

            ShowGuiAfterSearch(searchSucceed);
            parentEditorWindow.Repaint();
        }

        public void Dispose()
        {
            Views.Dispose();
        }
        
        public void OnTabUnselect()
        {
        }

        private async Task ExecMaterialAdjustAsync()
        {
            var button = Views.Get<ButtonElement>("execMaterialAdjustButton");
            button.SetProcessing("マテリアル分け中...");
            parentEditorWindow.Repaint();
            try
            {
                // ここで実行します。
                await ExecMaterialAdjustPreProAsync();
                button.RecoverFromProcessing();
                PostConvert();
            }
            catch (Exception e)
            {
                Debug.LogError("マテリアル分けに失敗しました。\n" + e);
            }
        }

        private async Task<UniqueParentTransformList> ExecMaterialAdjustPreProAsync()
        {
            var conf = GenerateConf();
            // 分割結合
            if (doChangeGranularity)
            {
                var result = await ExecGranularityConvertMainAsync();
                // 次のマテリアル分けの設定値を差し替え
                conf.DoDestroySrcObjs = true;
                conf.TargetTransforms = result.GeneratedRootTransforms;
            }

            // マテリアル分け
            return await ExecMaterialAdjustMainAsync(conf);
        }

        private async Task<UniqueParentTransformList> ExecMaterialAdjustMainAsync(MAExecutorConf conf)
        {
            await Task.Delay(100); // ボタン押下時のGUIの更新を反映させるために1フレーム以上待つ必要があります。
            await Task.Yield();

            UniqueParentTransformList result;
            IMAExecutorV2 maExecutor = selectedCriterion switch
            {
                MaterialCriterion.ByType => new MAExecutorV2ByType(),
                MaterialCriterion.ByAttribute => new MAExecutorV2ByAttr(),
                _ => throw new Exception("Unknown Criterion")
            };
            if (CurrentSceneTileSelectType == ChooserType.DynamicTile) // 動的タイルを選択しているとき
            {
                result = await tileConvert.ExecMaterialAdjustAsync(GenerateConf(), maExecutor);

                if (DoDestroySrcObjs)
                {
                    if (tileConvert.Selected.Count == 0) IsSearched = false;
                }
            }
            else
            {
                // ここで実行します。
                result = await maExecutor.ExecAsync(conf);

                // 完了後、GUIで表示される「選択オブジェクト」を完了後に差し替えます
                if (DoDestroySrcObjs)
                {
                    if (SelectedObjects.Count == 0) IsSearched = false;
                }

                Selection.objects = result.Get.Where(trans => trans != null).Select(trans => (UnityEngine.Object)trans.gameObject).ToArray();
                ForceSetTargetObjects(result);
            }

            return result;
        }
        

        /// <summary>
        /// マテリアル分けをせず、分割結合のみ行う場合に呼びます。
        /// </summary>
        private async Task ExecGranularityConvertAsync()
        {
            var button = Views.Get<ButtonElement>("execGranularityConvertButton");
            button.SetProcessing("粒度変更中...");
            parentEditorWindow.Repaint();

            try
            {
                // ここで実行します。
                await ExecGranularityConvertMainAsync();
                button.RecoverFromProcessing();
                PostConvert();
            }
            catch (Exception e)
            {
                Debug.LogError("粒度変更に失敗しました。\n" + e);
            }
        }
        

        private async Task<GranularityConvertResult> ExecGranularityConvertMainAsync()
        {
            await Task.Delay(100); // ボタン押下時のGUIの更新を反映させるために1フレーム以上待つ必要があります。
            await Task.Yield();

            if (CurrentSceneTileSelectType == ChooserType.DynamicTile) // 動的タイルを選択しているとき
            {
                return await tileConvert.ExecGranularityConvertAsync(GenerateConf(), granularity);
            }

            var conf = GenerateConf();
            if (conf.TargetTransforms.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return GranularityConvertResult.Fail();
            }
            
            // ここで実行します。
            var result = await new CityGranularityConverter().ConvertAsync(new GranularityConvertOptionUnity(new GranularityConvertOption(granularity, 1), conf.TargetTransforms, conf.DoDestroySrcObjs));
            
            // 後処理
            Selection.objects = result.GeneratedRootTransforms.Get.Select(trans => (UnityEngine.Object)trans.gameObject).ToArray();
            ForceSetTargetObjects(result.GeneratedRootTransforms);
            return result;
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
        
        /// <summary>
        /// 変更時チェックをスキップしながら、対象オブジェクトの指定を引数のものに置き換えます。
        /// </summary>
        private void ForceSetTargetObjects(UniqueParentTransformList target)
        {
            clearSearchOnTargetChange = false;
            Views.Get<ObjectSelectGui>().ForceSet(target);
            clearSearchOnTargetChange = true;
        }

    }
}