using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Interop;
using PLATEAU.Network;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityImportRemoteGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private DatasetSource datasetSource;
        private DatasetAccessor accessor;
        private bool datasetGroupLoaded;
        private int selectedDatasetGroupIndex;
        private int selectedDatasetIndex;
        private NativeVectorDatasetMetadataGroup datasetGroups;
        private readonly CityLoadConfig config = new CityLoadConfig();
        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;
        private const string serverUrl = "https://9tkm2n.deta.dev";
        

        public CityImportRemoteGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            LoadDatasetAsync().ContinueWithErrorCatch();
            progressGUI ??= new ProgressDisplayGUI(parentEditorWindow);
            progressGUI.ParentEditorWindow = parentEditorWindow;
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
                this.selectedDatasetGroupIndex = EditorGUILayout.Popup("都道府県", this.selectedDatasetGroupIndex, datasetGroupTitles);
                if (groupChangeCheck.changed)
                {
                    this.selectedDatasetIndex = 0;
                }
            }
            
            var datasetGroup = this.datasetGroups.At(this.selectedDatasetGroupIndex);
            var datasets = datasetGroup.Datasets;
            var datasetTitles = datasets.Select(d => d.Title).ToArray();
            this.selectedDatasetIndex = EditorGUILayout.Popup("データセット", this.selectedDatasetIndex, datasetTitles);
            var dataset = datasets.At(this.selectedDatasetIndex);
            PlateauEditorStyle.MultiLineLabelWithBox($"タイトル: {dataset.Title}\n説明    : {dataset.Description}\n最大LOD: {dataset.MaxLOD}");
            
            this.config.DatasetSourceConfig ??= new DatasetSourceConfig(true, "");
            var sourceConf = this.config.DatasetSourceConfig;
            sourceConf.DatasetIdOrSourcePath = dataset.ID;

            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes, sourceConf,
                    this, this.config.CoordinateZoneID);

            if (isAreaSelectComplete)
            {
                CityLoadConfigGUI.Draw(this.config);
                ImportButton.Draw(this.config, progressGUI);
            }
            PlateauEditorStyle.Separator(0);
            progressGUI.Draw();
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

        public void ReceiveResult(string[] areaMeshCodes, Extent extent,
            PredefinedCityModelPackage availablePackageFlags)
        {
            // TODO availablePackageFlags は、ローカルモードでは動作しますがサーバーモードでは None になります。
            //      これは、サーバーからメッシュコードの一覧を受け取る段階では存在するパッケージ種が不明だからです。
            //      そのため PackageFlags はとりあえず全種類として初期化しています。
            //      これには存在しないパッケージ種の設定GUIまで表示されるという欠点があります。
            //      しかしGMLファイルをダウンロードするときにはパッケージ種は分かるわけで、
            //      工夫すればサーバーに余計な負荷をかけることなしに範囲選択直後のこの段階でもパッケージ種を判別できるかもしれません。
            // this.config.InitWithPackageFlags(availablePackageFlags);
            this.config.InitWithPackageFlags((PredefinedCityModelPackage)~0u);
            this.config.AreaMeshCodes = areaMeshCodes;
            this.config.Extent = extent;
        }

    }
}
