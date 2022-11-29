using System.Linq;
using System.Threading.Tasks;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Interop;
using PLATEAU.Network;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityImportRemoteGUI : IEditorDrawable
    {
        private DatasetSource datasetSource;
        private DatasetAccessor accessor;
        private bool datasetGroupLoaded;
        private int selectedDatasetGroupIndex;
        private int selectedDatasetIndex;
        private NativeVectorDatasetMetadataGroup datasetGroups;
        private const string serverUrl = "https://9tkm2n.deta.dev";
        

        public CityImportRemoteGUI()
        {
            LoadDatasetAsync().ContinueWithErrorCatch();
        }
        
        public void Draw()
        {
            if (!this.datasetGroupLoaded)
            {
                EditorGUILayout.LabelField("サーバーに問い合わせ中です...");
                return;
            }
            PlateauEditorStyle.Heading("データセットの選択", "num1.png");
            using (var groupChangeCheck = new EditorGUI.ChangeCheckScope())
            {
                var datasetGroupTitles = this.datasetGroups.Select(dg => dg.Title).ToArray();
                this.selectedDatasetGroupIndex = EditorGUILayout.Popup("データセットグループ", this.selectedDatasetGroupIndex, datasetGroupTitles);
                if (groupChangeCheck.changed)
                {
                    this.selectedDatasetIndex = 0;
                }
            }
            
            var datasetGroup = this.datasetGroups.At(this.selectedDatasetGroupIndex);
            PlateauEditorStyle.MultiLineLabelWithBox($"ID        : {datasetGroup.ID}\nタイトル: {datasetGroup.Title}");
            var datasets = datasetGroup.Datasets;
            var datasetTitles = datasets.Select(d => d.Title).ToArray();
            this.selectedDatasetIndex = EditorGUILayout.Popup("データセット", this.selectedDatasetIndex, datasetTitles);
            var dataset = datasets.At(this.selectedDatasetIndex);
            PlateauEditorStyle.MultiLineLabelWithBox($"ID        : {dataset.ID}\nタイトル: {dataset.Title}\n説明    : {dataset.Description}");
            
        }

        private async Task LoadDatasetAsync()
        {
            this.datasetGroups = await Task.Run(() =>
            {
                using var client = Client.Create();
                client.Url = serverUrl;
                return client.GetDatasetMetadataGroup();
            });
            this.datasetGroupLoaded = true;
        }

    }
}
