using PLATEAU.CityAdjust.AlignLand;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Common.Tile;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using PLATEAU.Editor.Window.Main.Tab.TerrainConvertGui;
using PLATEAU.TerrainConvert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.Window.Main.Tab
{
    internal class CityTerrainConvertParam
    {
        internal static TerrainConvertOption.ImageOutput HEIGHTMAP_IMAGE_OUTPUT = TerrainConvertOption.ImageOutput.None;
        internal static readonly string[] SizeOptions = { "257 x 257", "513 x 513", "1025 x 1025", "2049 x 2049" };
        internal static readonly int[] SizeValues = { 257, 513, 1025, 2049 };

        public int SelectedSize { get; set; } = 1;
        
        /// <summary>
        /// 「余白を端の高さに合わせる」のチェック
        /// </summary>
        public bool FillEdges { get; set; } = true;
        
        /// <summary>
        /// 「地形変換」のチェック（Unityのテレインではない）
        /// </summary>
        public bool ConvertToTerrain { get; set; }
        
        /// <summary>
        /// 「テレインに変換する」のチェック
        /// </summary>
        public bool EnableTerrainConversion { get; set; }
        
        /// <summary>
        /// 「ハイトマップ平滑化」のチェック
        /// </summary>
        public bool ApplyConvolutionFilterToHeightMap { get; set; }
        
        /// <summary>
        /// 「高さ合わせ」のチェック
        /// </summary>
        public bool AlignLand { get; set; }
        
        /// <summary>
        /// 「交通・区域モデルの高さを地形に合わせる」のチェック
        /// </summary>
        public bool AlignLandNormal { get; set; }
        
        /// <summary>
        /// 「土地をLOD3道路モデルに合わせる」のチェック
        /// </summary>
        public bool AlignLandInvert { get; set; }
        public PreserveOrDestroy PreserveOrDestroy { get; set; }

        public int GetHeightmapWidth()
        {
            return (int)CityTerrainConvertParam.SizeValues.GetValue(SelectedSize);
        }
    }

    /// <summary>
    /// PLATEAU SDK ウィンドウで「地形変換」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityTerrainConvertGui : ITabContent
    {
        private PLATEAUInstancedCityModel targetModel;
        private CityTerrainConvertParam localParam;
        private EditorWindow parentWindow;

        /// <summary>
        /// シーンオブジェクト/動的タイルの現在選択されているタイプを返します。
        /// </summary>
        private SceneTileChooserType CurrentSceneTileSelectType => guis.Get<SceneTileChooserElement>()?.SelectedType ?? SceneTileChooserType.SceneObject;

        private TileTerrainConvert tileTerrainConvert;
        private TileListElementData tileListData;

        private readonly ElementGroup guis;
        
        private bool isExecTaskRunning = false;
        private const string ExecButtonTextNormal = "実行";
        private const string ExecButtonTextRunning = "生成中...";

        public CityTerrainConvertGui(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.parentWindow = parentEditorWindow;
            localParam = new CityTerrainConvertParam();
            tileListData = new TileListElementData(parentEditorWindow);
            tileTerrainConvert = new TileTerrainConvert(localParam, tileListData, parentEditorWindow);
            tileListData.EnableTileHierarchy = true; // タイル選択時に子要素も選択可能にするか？
            
            guis =
                new ElementGroup("",0,
                    new HeaderElementGroup("", "地形モデルの平滑化と地物の地形への高さ合わせを行います。", HeaderType.Subtitle),
                    new SceneTileChooserElement("TargetSelect", OnTargetModelChanged, OnTargetTileManagerChanged, OnChooserTypeChanged),
                    new TileListElement(tileListData),
                    new GeneralElement("", () => EditorGUILayout.HelpBox("選択された3D都市モデル内の地形モデルの平滑化・Terrain化及び地形に埋まっている各地物形状（道路、都市計画決定情報等）の高さ補正が行われます。", MessageType.Info)),
                    new GeneralElement("", NotifyIfInvalidTarget),
                    new HeaderElementGroup("", "設定", HeaderType.Header),
                    new DestroyOrPreserveSrcGui(OnPreserveOrDestroyChanged),
                    new HeaderElementGroup("", "", HeaderType.Separator),
                    new FoldOutElement("detailConf", "詳細設定", false,
                        new ToggleLeftElement("", "地形変換", true, OnConvertToTerrainChanged),
                        new ElementGroup("terrainConf", 1, BoxStyle.VerticalStyleLevel2,
                            new GeneralElement("", DrawHeightmapResolutionSelector),
                            new ToggleLeftElement("", "テレインに変換する", false, OnEnableTerrainConversionChanged),
                            new ToggleLeftElement("", "ハイトマップ平滑化", true, OnApplyConvolutionFilterChanged),
                            new ToggleLeftElement("", "余白を端の高さに合わせる", true, OnFillEdgesChanged)
                        ),
                        new ToggleLeftElement("", "高さ合わせ", true, OnChangeAlignLand),
                        new ElementGroup("alignLandConf", 1, BoxStyle.VerticalStyleLevel2,
                            new ToggleLeftElement("", "交通・区域モデルの高さを地形に合わせる", true, OnChangeAlignLandNormal),
                            new ToggleLeftElement("", "土地をLOD3道路モデルに合わせる", true, OnAlignLandInvertChanged),
                            new GeneralElement("", WarnAlignLandToLod3RoadNotWorking)
                        )
                    ),
                    new HeaderElementGroup("", "", HeaderType.Separator),
                    new ButtonElement("execButton", ExecButtonTextNormal, ()=>Exec().ContinueWithErrorCatch())
                );

            tileTerrainConvert.SetConversionFinishCallback( () => guis.Get<SceneTileChooserElement>().Reset() );
        }

        public VisualElement CreateGui()
        {
            return new IMGUIContainer(Draw);
        }

        private void Draw()
        {
            guis.Draw();
        }

        private void UpdateGui()
        {
            // 不要な設定項目を隠す
            var detailConf = guis?.Get<FoldOutElement>("detailConf");
            if (detailConf != null)
            {
                detailConf.ChildElementGroup.Get("terrainConf").IsVisible = localParam.ConvertToTerrain;
                detailConf.ChildElementGroup.Get("alignLandConf").IsVisible = localParam.AlignLand;
            }

            // 実行中なら実行ボタンを変更
            var execButton = guis?.Get<ButtonElement>("execButton");
            if (execButton != null)
            {
                execButton.ButtonText = isExecTaskRunning ? ExecButtonTextRunning : ExecButtonTextNormal;
                execButton.IsEnabled = !isExecTaskRunning;
            }

            var tilelist = guis?.Get<TileListElement>();
            if (tilelist != null)
            {
                tilelist.IsVisible = CurrentSceneTileSelectType == SceneTileChooserType.DynamicTile;
            }
        }

        private void DrawHeightmapResolutionSelector()
        {
            localParam.SelectedSize =
                PlateauEditorStyle.PopupWithLabelWidth(
                    "ハイトマップ解像度", localParam.SelectedSize, CityTerrainConvertParam.SizeOptions, 110);
        }

        private void SetTaskRunning(bool running)
        {
            isExecTaskRunning = running;
            UpdateGui();
        }

        private void OnChooserTypeChanged(SceneTileChooserType chooserType)
        {
            parentWindow.Repaint();
            UpdateGui();
        }

        /// <summary> GUIで変換対象のタイルマネージャーが変更された時 </summary>
        private void OnTargetTileManagerChanged(PLATEAUTileManager tileManager)
        {
            tileListData.TileManager = tileManager;
            UpdateGui();
        }

        /// <summary> GUIで変換対象の都市モデルが変更された時 </summary>
        private void OnTargetModelChanged(PLATEAUInstancedCityModel selectedModel)
        {
            targetModel = selectedModel;
            UpdateGui();
        }

        private void OnPreserveOrDestroyChanged(PreserveOrDestroy pod)
        {
            localParam.PreserveOrDestroy = pod;
            UpdateGui();
        }

        private void OnApplyConvolutionFilterChanged(bool value)
        {
            localParam.ApplyConvolutionFilterToHeightMap = value;
            UpdateGui();
        }

        private void OnConvertToTerrainChanged(bool value)
        {
            localParam.ConvertToTerrain = value;
            UpdateGui();
        }

        private void OnEnableTerrainConversionChanged(bool value)
        {
            localParam.EnableTerrainConversion = value;
            UpdateGui();
        }

        private void OnChangeAlignLand(bool value)
        {
            localParam.AlignLand = value;
            UpdateGui();
        }

        private void OnFillEdgesChanged(bool value)
        {
            localParam.FillEdges = value;
            UpdateGui();
        }

        private void OnChangeAlignLandNormal(bool value)
        {
            localParam.AlignLandNormal = value;
            UpdateGui();
        }
        
        private void OnAlignLandInvertChanged(bool value)
        {
            localParam.AlignLandInvert = value;
            UpdateGui();
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

        /// <summary> ここで実行します。地形変換/高さ合わせのエントリーポイントです。 </summary>
        private async Task Exec()
        {
            switch (CurrentSceneTileSelectType)
            {
                case SceneTileChooserType.DynamicTile:
                    await ExecForTile();
                    break;
                case SceneTileChooserType.SceneObject:
                    await ExecForSceneObjects();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        ///<summary>タイルを対象に地形変換/高さ合わせを実行します。</summary>
        private async Task ExecForTile()
        {
            SetTaskRunning(true);
            try
            {
                await tileTerrainConvert.Exec();
            }
            finally
            {
                SetTaskRunning(false);
            }
            return;
        }

        /// <summary>
        /// シーン内のゲームオブジェクトを対象に地形変換/高さ合わせを実行します。
        /// </summary>
        private async Task ExecForSceneObjects()
        {
            if (!CanExecOrNotify()) return;
            SetTaskRunning(true);
            try
            {
                int heightmapWidth = (int)CityTerrainConvertParam.SizeValues.GetValue(localParam.SelectedSize);
                Selection.objects = new Object[] { };

                // Terrainに変換します
                if (localParam.ConvertToTerrain)
                {
                    var converter = new CityTerrainConverter();
                    var convertOption = new TerrainConvertOption(
                        FilterDemItems(new GameObject[] { targetModel.gameObject }),
                        heightmapWidth,
                        localParam.PreserveOrDestroy == PreserveOrDestroy.Destroy,
                        localParam.FillEdges,
                        localParam.ApplyConvolutionFilterToHeightMap,
                        localParam.EnableTerrainConversion,
                        CityTerrainConvertParam.HEIGHTMAP_IMAGE_OUTPUT
                    );

                    await converter.ConvertAsync(convertOption);
                }

                // 高さ合わせを実行します
                if (localParam.AlignLand || ((!localParam.AlignLandNormal) && (!localParam.AlignLandInvert)))
                {
                    await ExecAlignLand(heightmapWidth);
                }
            }
            finally
            {
                parentWindow.Repaint(); // テキストが「実行中...」に変わったボタンを元に戻します
                SetTaskRunning(false);
            }
        }
        
        private void WarnAlignLandToLod3RoadNotWorking()
        {
            const string WARNING_MESSAGE = "土地をLOD3道路に合わせる機能を利用するには、平滑化された土地モデルが必要です。\nそのため、地形変換をオンにしてTerrain化するか、\n変換対象にTerrainが存在するようにしてください。";
            if (CurrentSceneTileSelectType == SceneTileChooserType.DynamicTile)
            {
                if (tileTerrainConvert?.IsAlignLandToLod3RoadNotWorking ?? true)
                    EditorGUILayout.HelpBox(WARNING_MESSAGE, MessageType.Warning);
                return;
            }

            if (targetModel == null) return;
            if (localParam.AlignLandInvert && (targetModel.GetComponentInChildren<Terrain>() == null && targetModel.GetComponentInChildren<PLATEAUSmoothedDem>() == null) && (!localParam.ConvertToTerrain))
            {
                EditorGUILayout.HelpBox(WARNING_MESSAGE, MessageType.Warning);
            }
        }

        private void NotifyIfInvalidTarget()
        {
            if (CurrentSceneTileSelectType == SceneTileChooserType.DynamicTile)
            {
                if (!(tileTerrainConvert?.IsReliefSelected ?? false))
                    EditorGUILayout.HelpBox("変換対象のタイルに土地が含まれていません。", MessageType.Error);
                return;
            }


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
            if(!(localParam.AlignLand || localParam.ConvertToTerrain))
            {
                Dialogue.Display("処理がすべてオフになっています。処理を選択してください。", "OK");
                return false;
            }
            return true;
        }

        public void OnTabUnselect()
        {
        }

        private async Task ExecAlignLand(int heightmapWidth)
        {
            // 高さ合わせの対象を検索します
            var searcher = new ALTargetSearcher(targetModel);
            if (!searcher.IsValid())
            {
                Dialogue.Display("高さ合わせに適した対象が見つかりませんでした。", "OK");
                return;
            }
            // 高さ合わせを実行します
            var conf = searcher.ToConfig(localParam.PreserveOrDestroy == PreserveOrDestroy.Destroy, heightmapWidth, localParam.FillEdges, localParam.ApplyConvolutionFilterToHeightMap, localParam.AlignLandNormal, localParam.AlignLandInvert);
            await ExecAlignLandInner();

            // 非同期部分のインナーメソッド
            async Task ExecAlignLandInner()
            {
                using var progressDisplay = new ProgressDisplayDialogue();
                await new AlignLandExecutor().ExecAsync(conf, progressDisplay);
            }
        }
        

        public void Dispose()
        {
            tileListData?.Dispose();
            guis.Dispose();
            tileTerrainConvert?.Dispose();
        }
    }
}
