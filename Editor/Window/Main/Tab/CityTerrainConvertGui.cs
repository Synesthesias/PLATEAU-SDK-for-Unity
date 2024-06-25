using PLATEAU.CityAdjust.AlignLand;
using System;
using System.Threading.Tasks;
using PLATEAU.Editor.Window.Common;
using PLATEAU.TerrainConvert;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using PLATEAU.Util;
using System.Collections.Generic;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「地形変換」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityTerrainConvertGui : ITabContent
    {
        private PLATEAUInstancedCityModel targetModel;
        private int selectedSize = 4;
        private bool fillEdges = true;
        private static TerrainConvertOption.ImageOutput heightmapImageOutput = TerrainConvertOption.ImageOutput.None;
        private static readonly string[] SizeOptions = { "33 x 33", "65 x 65", "129 x 129", "257 x 257", "513 x 513", "1025 x 1025", "2049 x 2049", "4097 x 4097" };
        private static readonly int[] SizeValues = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };
        private PreserveOrDestroy preserveOrDestroy;
        private bool convertToTerrain;
        private bool alignLand;
        private EditorWindow parentWindow;

        private readonly ElementGroup guis;
        
        private bool isExecTaskRunning = false;
        private const string ExecButtonTextNormal = "実行";
        private const string ExecButtonTextRunning = "生成中...";

        public CityTerrainConvertGui(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentWindow = parentEditorWindow;
            guis =
                new ElementGroup("",
                    new HeaderElementGroup("", "地形モデルの変換を行います", HeaderType.Subtitle),
                    new ObjectFieldElement<PLATEAUInstancedCityModel>("", "変換対象", OnTargetModelChanged),
                    new HeaderElementGroup("", "設定", HeaderType.Header),
                    new DestroyOrPreserveSrcGui(OnPreserveOrDestroyChanged),
                    new HeaderElementGroup("", "", HeaderType.Separator),
                    new ToggleLeftElement("", "地形をテレインに変換", true, OnConvertToTerrainChanged),
                    new GeneralElement("convertToTerrainConf", DrawResolutionSelector),
                    new ToggleLeftElement("", "交通・区域モデルの高さを地形に合わせる", true, OnChangeAlignLand),
                    new HeaderElementGroup("", "", HeaderType.Separator),
                    new ButtonElement("execButton", ExecButtonTextNormal, ()=>Exec().ContinueWithErrorCatch())
                    );
        }




        public void Draw()
        {
            guis.Draw();

            // 不要な設定項目を隠す
            guis.Get("convertToTerrainConf").IsVisible = convertToTerrain;
            
            // 実行中なら実行ボタンを変更
            var execButton = guis.Get<ButtonElement>("execButton");
            execButton.ButtonText = isExecTaskRunning ? ExecButtonTextRunning : ExecButtonTextNormal;
            execButton.IsEnabled = !isExecTaskRunning;
        }

        private void DrawResolutionSelector()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
            {
                EditorGUIUtility.labelWidth = 50;
                this.selectedSize =
                    PlateauEditorStyle.PopupWithLabelWidth(
                        "解像度", this.selectedSize, SizeOptions, 90);
                
                fillEdges = EditorGUILayout.ToggleLeft("余白を端の高さに合わせる", fillEdges);
            }
        }

        /// <summary> GUIで変換対象の都市モデルが変更された時 </summary>
        private void OnTargetModelChanged(PLATEAUInstancedCityModel selectedModel)
        {
            targetModel = selectedModel;
        }

        private void OnPreserveOrDestroyChanged(PreserveOrDestroy pod)
        {
            preserveOrDestroy = pod;
        }

        private void OnConvertToTerrainChanged(bool value)
        {
            convertToTerrain = value;
        }

        private void OnChangeAlignLand(bool value)
        {
            alignLand = value;
        }

        //選択アイテムのフィルタリング処理 (子にTINReliefを含む）
        private GameObject[] FilterItemsContainDemChildren(GameObject[] selection)
        {
            var selectionList = new HashSet<GameObject>();
            foreach (var gameObj in selection)
            {
                var components = gameObj.transform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var grp in components)
                {
                    if (grp.CityObjects.rootCityObjects.Exists(x => x.type == CityGML.CityObjectType.COT_TINRelief))
                    {
                        selectionList.Add(gameObj); break;
                    }
                }
            }
            return selectionList.ToArray();
        }

        //選択アイテムのフィルタリング処理 (TINReliefのアイテム）
        private GameObject[] FilterDemItems(GameObject[] selection)
        {
            var selectionList = new HashSet<GameObject>();
            foreach (var gameObj in selection)
            {
                var components = gameObj.transform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var grp in components)
                {
                    if (grp.CityObjects.rootCityObjects.Exists(x => x.type == CityGML.CityObjectType.COT_TINRelief))
                        selectionList.Add(grp.gameObject);
                }
            }
            return selectionList.ToArray();
        }

        private async Task Exec()
        {
            if (!CanExecOrNotify()) return;
            isExecTaskRunning = true;

            if (convertToTerrain)
            {
                var converter = new CityTerrainConverter();
                var convertOption = new TerrainConvertOption(
                    FilterDemItems(new GameObject[]{targetModel.gameObject}),
                    (int)SizeValues.GetValue(selectedSize),
                    preserveOrDestroy == PreserveOrDestroy.Destroy,
                    fillEdges,
                    heightmapImageOutput
                );

                await converter.ConvertAsync(convertOption);
            }

            if (alignLand)
            {
                ExecAlignLand();
            }
            
            isExecTaskRunning = false;
            parentWindow.Repaint(); // テキストが「実行中...」に変わったボタンを元に戻します
        }

        /// <summary>
        /// 地形変換の実行可否をboolで返します。
        /// 不可の場合、その理由をダイアログで表示します。
        /// </summary>
        private bool CanExecOrNotify()
        {
            if (targetModel == null)
            {
                Dialogue.Display("変換対象の都市モデルが選択されていません。", "OK");
                return false;
            }
            if(!(alignLand || convertToTerrain))
            {
                Dialogue.Display("処理がすべてオフになっています。処理を選択してください。", "OK");
                return false;
            }
            return true;
        }

        public void OnTabUnselect()
        {
        }

        private void ExecAlignLand()
        {
            // 高さ合わせの対象を検索します
            var searcher = new ALTargetSearcher(targetModel);
            if (!searcher.IsValid())
            {
                Dialogue.Display("高さ合わせに適した対象が見つかりませんでした。", "OK");
                return;
            }
            // 高さ合わせを実行します
            var conf = searcher.ToConfig(preserveOrDestroy == PreserveOrDestroy.Destroy);
            ExecAlignLandInner().ContinueWithErrorCatch();

            // 非同期部分のインナーメソッド
            async Task ExecAlignLandInner()
            {
                using var progressDisplay = new ProgressDisplayDialogue();
                await new AlignLandExecutor().ExecAsync(conf, progressDisplay);
            }
        }
        

        public void Dispose()
        {
            guis.Dispose();
        }
    }
}
