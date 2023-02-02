using System.Linq;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Geometries;
using PLATEAU.Native;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// インポート画面で「サーバー」が選択されたときのGUIです。
    /// </summary>
    internal class CityImportRemoteGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private DatasetSource datasetSource;
        private DatasetAccessor accessor;
        private int selectedDatasetGroupIndex;
        private int selectedDatasetIndex;
        private readonly CityLoadConfig config = new CityLoadConfig();
        private ServerDatasetFetchGUI serverDatasetFetchGUI;
        
        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;


        public CityImportRemoteGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.serverDatasetFetchGUI = new ServerDatasetFetchGUI(parentEditorWindow);
            progressGUI ??= new ProgressDisplayGUI(parentEditorWindow);
            progressGUI.ParentEditorWindow = parentEditorWindow;
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
                this.selectedDatasetGroupIndex = EditorGUILayout.Popup("都道府県", this.selectedDatasetGroupIndex, datasetGroupTitles);
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
            PlateauEditorStyle.MultiLineLabelWithBox($"タイトル: {dataset.Title}\n説明    : {dataset.Description}\n種別: {dataset.PackageFlags.ToJapaneseName()}");
            
            this.config.DatasetSourceConfig ??= new DatasetSourceConfig(true, "", "", "", "");
            var sourceConf = this.config.DatasetSourceConfig;
            sourceConf.ServerDatasetID = dataset.ID;
            sourceConf.ServerUrl = this.serverDatasetFetchGUI.ServerUrl;
            sourceConf.ServerToken = this.serverDatasetFetchGUI.ServerToken;

            this.config.CoordinateZoneID = EditorGUILayout.Popup(
                "基準座標系", this.config.CoordinateZoneID - 1,
                GeoReference.ZoneIdExplanation) + 1; // 番号は 1 スタート

            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");

            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes, sourceConf,
                    this, this.config.CoordinateZoneID);

            if (isAreaSelectComplete)
            {
                PlateauEditorStyle.Heading("地物別設定", "num3.png");
                CityLoadConfigGUI.Draw(this.config);
                ImportButton.Draw(this.config, progressGUI);
            }
            PlateauEditorStyle.Separator(0);
            progressGUI.Draw();
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
            this.config.SearchCenterPointAndSetAsReferencePoint();
        }

    }
}
