using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts;
using PLATEAU.GranularityConvert;
using PLATEAU.Util.Async;
using System.Threading.Tasks;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「マテリアル分け」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityMaterialAdjustGUI : ITabContent
    {
        public ElementGroup Guis { get; }
        
        public MAKeySearcher CurrentSearcher => Guis?.Get<MaterialCriterionGui>()?.CurrentSearcher;
        public MaterialCriterion SelectedCriterion => Guis.Get<MaterialCriterionGui>().SelectedCriterion;

        /// <summary>
        /// マテリアル分けの選択画面を出すために、利用可能な分類項目を検索したかどうかです。
        /// falseにすることで検索をリセットできます。
        /// </summary>
        public bool IsSearched
        {
            get => CurrentSearcher.IsSearched;
            set
            {
                CurrentSearcher.IsSearched = value;
                if (value)
                {
                    Guis.Get<ObjectSelectGui>().LockChange = true;
                }
            }
        }


        public CityMaterialAdjustGUI(EditorWindow parentEditorWindow)
        {
            // UIの内容をここで定義します。
            Guis =
                new ElementGroup("",
                    new HeaderElementGroup("", "分類に応じたマテリアル分けを行います。", HeaderType.Subtitle),
                    new ElementGroup("MAGuiBeforeSearch", // *** 検索前のUI ***
                        new ObjectSelectGui(this, parentEditorWindow), // 対象オブジェクト選択
                        new HeaderElementGroup("commonConf", "共通設定", HeaderType.Header,
                            new DestroyOrPreserveSrcGui() // 元のオブジェクトを削除するか残すか
                        ),
                        new HeaderElementGroup("", "分割/結合", HeaderType.Header,
                            new MAGranularityGui() // 分割結合に関する一般設定
                        ),
                        new HeaderElementGroup("", "マテリアル分け", HeaderType.Header,
                            new ToggleLeftElement("doMaterialAdjust", "マテリアルを条件指定で変更する", false),
                            new ElementGroup("materialConf", 
                                new MaterialCriterionGui(), // マテリアル分け基準選択
                                new AttributeKeyGui(), // 属性情報キー選択
                                new MASearchButton(this, parentEditorWindow) // 検索ボタン
                            )
                        )
                    ),
                    new ElementGroup("MAGuiAfterSearch", // *** 検索後のUI ***
                        new MaterialConfGui(this), // マテリアル設定
                        new MAExecButton(this) // 実行ボタン
                    ),
                    new ButtonElement("", "粒度変更を実行", ExecGranularityConvert)
                );
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        public void Draw()
        {

            Guis.Draw();

            // 不要な設定項目を隠します
            bool doMaterialAdjust = Guis.Get<ToggleLeftElement>("doMaterialAdjust").Value;
            Guis.Get<AttributeKeyGui>().IsVisible = SelectedCriterion == MaterialCriterion.ByAttribute; // 属性情報による分類をするときのみ属性情報キー入力欄を表示
            Guis.Get<ButtonElement>().IsVisible = !doMaterialAdjust &&
                                                  Guis.Get<MAGranularityGui>().Granularity != MAGranularity.DoNotChange;// マテリアル分けをせず粒度変更のみのとき表示するボタン
            Guis.Get("MAGuiAfterSearch").IsVisible = IsSearched && doMaterialAdjust;
            Guis.Get("materialConf").IsVisible = Guis.Get<ToggleLeftElement>("doMaterialAdjust").Value;
        }

        /// <summary>
        /// 現在のGUIの設定で検索条件を生成します。
        /// </summary>
        public SearchArg GenerateSearchArg()
        {
            SearchArg searchArg = SelectedCriterion switch
            {
                MaterialCriterion.ByType => new SearchArg(Guis.Get<ObjectSelectGui>().UniqueSelected),
                MaterialCriterion.ByAttribute => new SearchArgByArr(Guis.Get<ObjectSelectGui>().UniqueSelected, Guis.Get<AttributeKeyGui>().AttrKey),
                _ => throw new ArgumentOutOfRangeException()
            };
            return searchArg;
        }

        public void NotifyTargetChange()
        {
            IsSearched = false;
        }

        public void Dispose()
        {
            Guis.Dispose();
        }
        
        public void OnTabUnselect()
        {
        }

        /// <summary>
        /// マテリアル分けをせず、分割結合のみ行う場合に呼びます。
        /// </summary>
        public void ExecGranularityConvert()
        {
            ExecGranularityConvertAsync().ContinueWithErrorCatch();
        }

        private async Task ExecGranularityConvertAsync()
        {
            await new CityGranularityConverter().ConvertProgressiveAsync(GenerateConf(), new MAConditionSimple());
        }

        /// <summary>
        /// GUIをもとに設定値インスタンスを生成します。
        /// </summary>
        private MAExecutorConf GenerateConf()
        {
            var granularity = Guis.Get<MAGranularityGui>().Granularity;
            
            return SelectedCriterion switch
            {
                // MaterialCriterion.None => new MAExecutorConf(
                //     null,
                //     Guis.Get<ObjectSelectGui>().UniqueSelected,
                //     granularity,
                //     Guis.Get<DestroyOrPreserveSrcGui>().DoDestroySrcObjs,
                //     false
                // ),
                MaterialCriterion.None => throw new Exception("分割結合のみのときはCityGranularityConverterを使ってください"),
                
                MaterialCriterion.ByType => new MAExecutorConf(
                    CurrentSearcher.MaterialAdjustConf,
                    Guis.Get<ObjectSelectGui>().UniqueSelected,
                    granularity,
                    Guis.Get<DestroyOrPreserveSrcGui>().DoDestroySrcObjs,
                    true
                ),
                    
                MaterialCriterion.ByAttribute => new MAExecutorConfByAttr(
                    CurrentSearcher.MaterialAdjustConf,
                    Guis.Get<ObjectSelectGui>().UniqueSelected,
                    granularity,
                    Guis.Get<DestroyOrPreserveSrcGui>().DoDestroySrcObjs,
                    true,
                    Guis.Get<AttributeKeyGui>().AttrKey
                ),
                    
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public MAExecutor GenerateExecutor()
        {
            var executorConf = GenerateConf();
            return SelectedCriterion switch
            {
                MaterialCriterion.None => MAExecutorFactory.CreateGranularityExecutor(executorConf),
                MaterialCriterion.ByType => MAExecutorFactory.CreateTypeExecutor(executorConf),
                MaterialCriterion.ByAttribute => MAExecutorFactory.CreateAttrExecutor((MAExecutorConfByAttr)executorConf),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}