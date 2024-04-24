using System;
using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts;
using UnityEditor;
using UnityEngine.UIElements;

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

        public bool IsSearched
        {
            get => CurrentSearcher.IsSearched;
            set => CurrentSearcher.IsSearched = value;
        }


        public CityMaterialAdjustGUI(EditorWindow parentEditorWindow)
        {
            Guis =
                new ElementGroup("",
                    new HeaderElementGroup("", "分類に応じたマテリアル分けを行います。", HeaderType.Subtitle),
                    new ElementGroup("MAGuiBeforeSearch", // *** 検索前のUI ***
                        new ObjectSelectGui(this, parentEditorWindow), // 対象オブジェクト選択
                        new HeaderElementGroup("commonConf", "共通設定", HeaderType.Header,
                            new DestroyOrPreserveSrcGui(), // 元のオブジェクトを削除するか残すか
                            new NameSelectGui("処理後の親オブジェクト名", "Converted") // 名前入力
                        ),
                        new HeaderElementGroup("", "分割/結合", HeaderType.Header,
                            new MAGranularityGui() // 分割結合に関する一般設定
                        ),
                        new HeaderElementGroup("", "マテリアル分け", HeaderType.Header,
                            new ToggleLeftElement("doMaterialAdjust", "マテリアルを条件指定で変更する", false),
                            new ElementGroup("materialConf", 
                                new MaterialCriterionGui(), // マテリアル分け基準選択
                                new AttributeKeyGui(), // 属性情報キー選択
                                new ToggleLeftElement("skipNotChangingMaterial", "マテリアル変更対象外のものは分割結合をスキップ", true),
                                new MASearchButton(this, parentEditorWindow) // 検索ボタン
                            )
                        )
                    ),
                    new ElementGroup("MAGuiAfterSearch", // *** 検索後のUI ***
                        new MaterialConfGui(this), // マテリアル設定
                        new MAExecButton(this) // 実行ボタン
                    )
                );
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        public void Draw()
        {

            Guis.Draw();

            // 不要な設定項目を隠します
            Guis.Get<AttributeKeyGui>().IsVisible = SelectedCriterion == MaterialCriterion.ByAttribute;
            Guis.Get("MAGuiAfterSearch").IsVisible = IsSearched;
            Guis.Get("materialConf").IsVisible = Guis.Get<ToggleLeftElement>("doMaterialAdjust").Value;
        }

        /// <summary>
        /// 現在のGUIの設定で検索条件を生成します。
        /// </summary>
        public SearchArg GenerateSearchArg()
        {
            SearchArg searchArg = SelectedCriterion switch
            {
                MaterialCriterion.ByType => new SearchArg(Guis.Get<ObjectSelectGui>().SelectedTransforms),
                MaterialCriterion.ByAttribute => new SearchArgByArr(Guis.Get<ObjectSelectGui>().SelectedTransforms, Guis.Get<AttributeKeyGui>().AttrKey),
                _ => throw new ArgumentOutOfRangeException()
            };
            return searchArg;
        }

        public void UpdateObjectSelection()
        {
            Guis.Get<ObjectSelectGui>().UpdateSelection();
        }

        public void Dispose()
        {
            Guis.Dispose();
        }
        
        public void OnTabUnselect()
        {
        }
    }
}