using System.Linq;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// インポート画面で「サーバー」が選択されたときのGUIです。
    /// </summary>
    internal class CityImportRemoteGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private readonly CityLoadConfig config = new CityLoadConfig();
        private readonly GUIBeforeAreaSelect guiBeforeAreaSelect;
        private ImportGUIAfterAreaSelect guiAfterAreaSelect;

        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;


        public CityImportRemoteGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.guiBeforeAreaSelect = new GUIBeforeAreaSelect(parentEditorWindow, config, this);
            progressGUI = new ProgressDisplayGUI(parentEditorWindow);
        }

        public void Draw()
        {
            this.guiBeforeAreaSelect.Draw();

            if (this.guiBeforeAreaSelect.IsAreaSelectComplete)
            {
                this.guiAfterAreaSelect.Draw();
            }

            progressGUI.Draw();
        }


        public void ReceiveResult(AreaSelectResult result)
        {
            this.config.InitWithAreaSelectResult(result);
            this.guiAfterAreaSelect = new ImportGUIAfterAreaSelect(this.config, result.PackageToLodDict, progressGUI);
        }

        // テストで使う用です。
        internal ServerDatasetFetchGUI.LoadStatusEnum DatasetFetchStatus => guiBeforeAreaSelect.DatasetFetchStatus;


        /// <summary>
        /// リモートインポートGUIのうち、範囲選択前に表示する部分です。
        /// </summary>
        private class GUIBeforeAreaSelect : IEditorDrawable
        {
            private readonly ServerDatasetFetchGUI serverDatasetFetchGUI;
            private int selectedDatasetGroupIndex;
            private int selectedDatasetIndex;
            private readonly CityLoadConfig cityLoadConfig;
            private readonly IAreaSelectResultReceiver areaSelectResultReceiver;
            public bool IsAreaSelectComplete { get; private set; }
            public ServerDatasetFetchGUI.LoadStatusEnum DatasetFetchStatus => this.serverDatasetFetchGUI.LoadStatus;

            public GUIBeforeAreaSelect(UnityEditor.EditorWindow parentEditorWindow, CityLoadConfig config,
                IAreaSelectResultReceiver areaSelectResultReceiver)
            {
                this.serverDatasetFetchGUI = new ServerDatasetFetchGUI(parentEditorWindow);
                this.cityLoadConfig = config;
                this.areaSelectResultReceiver = areaSelectResultReceiver;
            }

            public void Draw()
            {
                EditorGUILayout.Space(15);
                this.serverDatasetFetchGUI.Draw();


                if (this.serverDatasetFetchGUI.LoadStatus != ServerDatasetFetchGUI.LoadStatusEnum.Success)
                {
                    return;
                }

                // どのようなデータセットが利用可能であるか、サーバーからの返答があるまでは以下は実行されません。

                PlateauEditorStyle.Heading("データセットの選択", "num1.png");

                var datasetGroups = this.serverDatasetFetchGUI.DatasetGroups;
                using (var groupChangeCheck = new EditorGUI.ChangeCheckScope())
                {
                    var datasetGroupTitles = datasetGroups.Select(dg => dg.Title).ToArray();
                    this.selectedDatasetGroupIndex =
                        EditorGUILayout.Popup("都道府県", this.selectedDatasetGroupIndex, datasetGroupTitles);
                    if (groupChangeCheck.changed)
                    {
                        this.selectedDatasetIndex = 0;
                    }
                }

                var datasetGroup = datasetGroups.At(this.selectedDatasetGroupIndex);
                var datasets = datasetGroup.Datasets;
                var datasetTitles = datasets.Select(d => d.Title).ToArray();
                this.selectedDatasetIndex = EditorGUILayout.Popup("データセット", this.selectedDatasetIndex, datasetTitles);
                var dataset = datasets.At(this.selectedDatasetIndex);
                PlateauEditorStyle.MultiLineLabelWithBox(
                    $"タイトル: {dataset.Title}\n説明    : {dataset.Description}\n種別: {dataset.PackageFlags.ToJapaneseName()}");

                this.cityLoadConfig.DatasetSourceConfig ??= new DatasetSourceConfig(true, "", "", "", "");
                var sourceConf = this.cityLoadConfig.DatasetSourceConfig;
                sourceConf.ServerDatasetID = dataset.ID;
                sourceConf.ServerUrl = this.serverDatasetFetchGUI.ServerUrl;
                sourceConf.ServerToken = this.serverDatasetFetchGUI.ServerToken;

                CoordinateZonePopup.DrawAndSet(this.cityLoadConfig);

                PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");

                IsAreaSelectComplete = AreaSelectButton.Draw(this.cityLoadConfig.AreaMeshCodes, sourceConf,
                    this.areaSelectResultReceiver, this.cityLoadConfig.CoordinateZoneID);
            }
        }
    }
}