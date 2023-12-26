using System.Collections.Generic;
using System.Threading;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Config
{
    /// <summary>
    /// 都市インポートの設定です。
    /// インポート画面の設定GUIでユーザーはこの設定値を書き換えていくことになります。
    /// パッケージごとのインポート設定については<see cref="PackageImportConfigDict"/>を参照してください。
    /// 設定GUIについてはCityAddGUIおよびCityLoadConfigGUIを参照してください。
    /// </summary>
    public class CityImportConfig
    {
        public ConfigBeforeAreaSelect ConfBeforeAreaSelect { get; set; } = new ConfigBeforeAreaSelect();
        
        /// <summary>
        /// 範囲選択で選択された範囲です。
        /// </summary>
        public MeshCodeList AreaMeshCodes { get; private set; }
        
        
        /// <summary>
        /// 基準点です。
        /// ユーザーが選択した直交座標系の原点から、何メートルの地点を3Dモデルの原点とするかです。
        /// </summary>
        public PlateauVector3d ReferencePoint { get; set; }

        /// <summary>
        /// 都市モデル読み込みの、パッケージ種ごとの設定です。
        /// </summary>
        public PackageImportConfigDict PackageImportConfigDict { get; private set; }

        private CityImportConfig(){}

        /// <summary>
        /// デフォルト設定の<see cref="CityImportConfig"/>を作ります。
        /// </summary>
        public static CityImportConfig CreateDefault()
        {
            return new CityImportConfig();
        }
        
        /// <summary>
        /// 範囲選択画面の結果から設定値を作ります。
        /// ユーザーがGUIでインポート設定する場合：
        /// ここで設定する値は、範囲選択後にインポート設定GUIで表示される初期値となります。
        /// このあと、ユーザーのGUI操作によって設定値を書き換えていくことになります。
        /// </summary>
        public static CityImportConfig CreateWithAreaSelectResult(AreaSelectResult result)
        {
            var ret = CityImportConfig.CreateDefault();
            ret.ConfBeforeAreaSelect = result.ConfBeforeAreaSelect;
            ret.InitWithPackageLodsDict(result.PackageToLodDict);
            ret.AreaMeshCodes = result.AreaMeshCodes;
            ret.ReferencePoint = ret.AreaMeshCodes.ExtentCenter(ret.ConfBeforeAreaSelect.CoordinateZoneID);
            return ret;
        }

        public PackageImportConfig GetConfigForPackage(PredefinedCityModelPackage package)
        {
            return PackageImportConfigDict.GetConfigForPackage(package);
        }
        
        /// <summary>
        /// 設定に合うGMLファイルを検索します。
        /// 多数のファイルから検索するので、実行時間が長くなりがちである点にご注意ください。
        /// </summary>
        /// <returns>検索にヒットしたGMLのリストです。</returns>
        public List<GmlFile> SearchMatchingGMLList( CancellationToken? token )
        {
            token?.ThrowIfCancellationRequested();


            // 地域ID(メッシュコード)で絞り込みます。

            using var datasetSource = DatasetSource.Create(ConfBeforeAreaSelect.DatasetSourceConfig);
            using var datasetAccessor = datasetSource.Accessor.FilterByMeshCodes(AreaMeshCodes.Data);

            // パッケージ種ごとの設定で「ロードする」にチェックが入っているパッケージ種で絞り込みます。
            var targetPackages = PackageImportConfigDict.PackagesToLoad();
            var foundGMLList = new List<GmlFile>();

            // 絞り込まれたGMLパスを戻り値のリストに追加します。
            foreach (var package in targetPackages)
            {
                var gmlFiles = datasetAccessor.GetGmlFiles(package);
                int gmlCount = gmlFiles.Length;
                for (int i = 0; i < gmlCount; i++)
                {
                    var gml = gmlFiles.At(i);
                    foundGMLList.Add(gml);
                }
            }

            return foundGMLList;
        }
        
        /// <summary>
        /// パッケージ種とLODの組から設定値を作ります。
        /// </summary>
        private void InitWithPackageLodsDict(PackageToLodDict dict)
        {
            PackageImportConfigDict = new PackageImportConfigDict(dict);
        }


        /// <summary>
        /// インポート設定について、C++のstructに変換します。
        /// </summary>
        internal MeshExtractOptions CreateNativeConfigFor(PredefinedCityModelPackage package)
        {
            var packageConf = GetConfigForPackage(package);
            return packageConf.ConvertToNativeOption(ReferencePoint, ConfBeforeAreaSelect.CoordinateZoneID);
        } 
        
        private static bool ShouldExcludeCityObjectOutsideExtent(PredefinedCityModelPackage package)
        {
            return package != PredefinedCityModelPackage.Relief;
        }
        
    }
}
