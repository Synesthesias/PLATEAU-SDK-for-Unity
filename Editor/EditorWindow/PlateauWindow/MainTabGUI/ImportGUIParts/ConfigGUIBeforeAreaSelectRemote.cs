using System.Linq;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// リモートインポートGUIのうち、範囲選択前に表示する部分です。
    /// </summary>
    internal class ConfigGUIBeforeAreaSelectRemote : IConfigGUIBeforeAreaSelect
    {
        private readonly ConfigBeforeAreaSelect confBeforeAreaSelect;
        private readonly ServerDatasetFetchGUI serverDatasetFetchGUI;
        private int selectedDatasetGroupIndex;
        private int selectedDatasetIndex;
        public ServerDatasetFetchGUI.LoadStatusEnum DatasetFetchStatus => this.serverDatasetFetchGUI.LoadStatus;

        public ConfigGUIBeforeAreaSelectRemote(UnityEditor.EditorWindow parentEditorWindow)
        {
            serverDatasetFetchGUI = new ServerDatasetFetchGUI(parentEditorWindow);
            confBeforeAreaSelect = new ConfigBeforeAreaSelect();
        }

        public ConfigBeforeAreaSelect Draw()
        {
            EditorGUILayout.Space(15);
            this.serverDatasetFetchGUI.Draw();


            if (this.serverDatasetFetchGUI.LoadStatus != ServerDatasetFetchGUI.LoadStatusEnum.Success)
            {
                return confBeforeAreaSelect;
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

            this.confBeforeAreaSelect.DatasetSourceConfig ??= new DatasetSourceConfigRemote("", "", "");
            var sourceConf = (DatasetSourceConfigRemote)this.confBeforeAreaSelect.DatasetSourceConfig;
            sourceConf.ServerDatasetID = dataset.ID;
            sourceConf.ServerUrl = this.serverDatasetFetchGUI.ServerUrl;
            sourceConf.ServerToken = this.serverDatasetFetchGUI.ServerToken;

            confBeforeAreaSelect.CoordinateZoneID = CoordinateZonePopup.Draw(confBeforeAreaSelect.CoordinateZoneID);
            return confBeforeAreaSelect;
        }
    }
}