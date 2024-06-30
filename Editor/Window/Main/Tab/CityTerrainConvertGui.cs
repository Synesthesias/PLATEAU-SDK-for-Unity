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
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「地形変換」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityTerrainConvertGui : ITabContent
    {
        private PLATEAUInstancedCityModel targetModel;
        private int selectedSize = 1;
        private bool fillEdges = true;
        private static TerrainConvertOption.ImageOutput heightmapImageOutput = TerrainConvertOption.ImageOutput.None;
        private static readonly string[] SizeOptions = { "257 x 257", "513 x 513", "1025 x 1025", "2049 x 2049" };
        private static readonly int[] SizeValues = { 257, 513, 1025, 2049 };
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
                    new GeneralElement("", NotifyIfInvalidTarget),
                    new HeaderElementGroup("", "設定", HeaderType.Header),
                    new DestroyOrPreserveSrcGui(OnPreserveOrDestroyChanged),
                    new HeaderElementGroup("", "", HeaderType.Separator),
                    new FoldOutElement("detailConf", "詳細設定",
                        new ToggleLeftElement("", "地形をテレインに変換", true, OnConvertToTerrainChanged),
                        new ElementGroup("terrainConf",
                            new GeneralElement("", DrawHeightmapResolutionSelector),
                            new GeneralElement("convertToTerrainConf", DrawTerrainEdgeConf)
                        )
                    ),
                    new ToggleLeftElement("", "交通・区域モデルの高さを地形に合わせる", true, OnChangeAlignLand),
                    new HeaderElementGroup("", "", HeaderType.Separator),
                    new ButtonElement("execButton", ExecButtonTextNormal, ()=>Exec().ContinueWithErrorCatch())
                    );
        }




        public void Draw()
        {
            guis.Draw();

            // 不要な設定項目を隠す
            guis.Get<FoldOutElement>("detailConf").ChildElementGroup.Get("terrainConf").IsVisible = convertToTerrain;
            
            // 実行中なら実行ボタンを変更
            var execButton = guis.Get<ButtonElement>("execButton");
            execButton.ButtonText = isExecTaskRunning ? ExecButtonTextRunning : ExecButtonTextNormal;
            execButton.IsEnabled = !isExecTaskRunning;
        }

        private void DrawHeightmapResolutionSelector()
        {
            EditorGUIUtility.labelWidth = 50;
            this.selectedSize =
                PlateauEditorStyle.PopupWithLabelWidth(
                    "高さマップ解像度", this.selectedSize, SizeOptions, 90);
        }

        private void DrawTerrainEdgeConf()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(16, 0, 8, 16))
            {
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

        /// <summary> ここで実行します </summary>
        private async Task Exec()
        {
            if (!CanExecOrNotify()) return;
            isExecTaskRunning = true;
            int heightmapWidth = (int)SizeValues.GetValue(selectedSize);
            Selection.objects = new Object[] { };

            // Terrainに変換します
            if (convertToTerrain)
            {
                var converter = new CityTerrainConverter();
                var convertOption = new TerrainConvertOption(
                    FilterDemItems(new GameObject[]{targetModel.gameObject}),
                    heightmapWidth,
                    preserveOrDestroy == PreserveOrDestroy.Destroy,
                    fillEdges,
                    heightmapImageOutput
                );

                await converter.ConvertAsync(convertOption);
            }

            // 高さ合わせを実行します
            if (alignLand)
            {
                ExecAlignLand(heightmapWidth);
            }
            
            isExecTaskRunning = false;
            parentWindow.Repaint(); // テキストが「実行中...」に変わったボタンを元に戻します
        }

        private void NotifyIfInvalidTarget()
        {
            if (targetModel == null) return;
            if (!new ALTargetSearcher(targetModel).IsLandExist())
            {
                EditorGUILayout.HelpBox("変換対象の都市モデルに土地が含まれていません。", MessageType.Error);
            }
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

        private void ExecAlignLand(int heightmapWidth)
        {
            // 高さ合わせの対象を検索します
            var searcher = new ALTargetSearcher(targetModel);
            if (!searcher.IsValid())
            {
                Dialogue.Display("高さ合わせに適した対象が見つかりませんでした。", "OK");
                return;
            }
            // 高さ合わせを実行します
            var conf = searcher.ToConfig(preserveOrDestroy == PreserveOrDestroy.Destroy, heightmapWidth);
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
