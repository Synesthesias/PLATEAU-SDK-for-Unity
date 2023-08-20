using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// 都市インポートの設定です。
    /// インポート画面の設定GUIでユーザーはこの設定値を書き換えていくことになります。
    /// パッケージごとのインポート設定については<see cref="PackageLoadSetting"/>を参照してください。
    /// 設定GUIについてはCityAddGUIおよびCityLoadConfigGUIを参照してください。
    /// </summary>
    internal class CityLoadConfig
    {
        /// <summary>
        /// 都市モデル読み込みの全体設定です。
        /// </summary>
        public DatasetSourceConfig DatasetSourceConfig { get; set; }
        public string[] AreaMeshCodes { get; set; }
        
        /// <summary>
        /// 平面直角座標系の番号です。
        /// 次のサイトで示される平面直角座標系の番号です。
        /// https://www.gsi.go.jp/sokuchikijun/jpc.html
        /// </summary>
        public int CoordinateZoneID { get; set; } = 9;
        
        private Extent extent;
        /// <summary>
        /// 緯度・経度での範囲です。
        /// ただし、高さの設定は無視され、高さの範囲は必ず -99,999 ～ 99,999 になります。
        /// </summary>
        public Extent Extent
        {
            get => this.extent;
            set =>
                this.extent = new Extent(
                    new GeoCoordinate(value.Min.Latitude, value.Min.Longitude, -99999),
                    new GeoCoordinate(value.Max.Latitude, value.Max.Longitude, 99999));
        }
        
        
        /// <summary>
        /// 基準点です。
        /// ユーザーが選択した直交座標系の原点から、何メートルの地点を3Dモデルの原点とするかです。
        /// </summary>
        public PlateauVector3d ReferencePoint { get; set; }
        
        /// <summary>
        /// 都市モデル読み込みの、パッケージ種ごとの設定です。
        /// </summary>
        private readonly Dictionary<PredefinedCityModelPackage, PackageLoadSetting> perPackagePairSettings =
            new ();

        /// <summary>
        /// 値ペア (パッケージ種, そのパッケージに関する設定) を、パッケージごとの IEnumerable にして返します。
        /// </summary>
        public IEnumerable<KeyValuePair<PredefinedCityModelPackage, PackageLoadSetting>> ForEachPackagePair =>
            this.perPackagePairSettings;

        public PackageLoadSetting GetConfigForPackage(PredefinedCityModelPackage package)
        {
            return this.perPackagePairSettings[package];
        }

        /// <summary>
        /// 設定に合うGMLファイルを検索します。
        /// 多数のファイルから検索するので、実行時間が長くなりがちである点にご注意ください。
        /// </summary>
        /// <returns>検索にヒットしたGMLのリストです。</returns>
        public List<GmlFile> SearchMatchingGMLList( CancellationToken token )
        {
            token.ThrowIfCancellationRequested();

            using var datasetSource = DatasetSource.Create(DatasetSourceConfig);
            using var datasetAccessor = datasetSource.Accessor;

            // 地域ID(メッシュコード)で絞り込みます。
            var meshCodes = AreaMeshCodes.Select(MeshCode.Parse).Where(code => code.IsValid).ToArray();

            // パッケージ種ごとの設定で「ロードする」にチェックが入っているパッケージ種で絞り込みます。
            var targetPackages =
                this
                    .ForEachPackagePair
                    .Where(pair => pair.Value.LoadPackage)
                    .Select(pair => pair.Key);
            var foundGmls = new List<GmlFile>();

            // 絞り込まれたGMLパスを戻り値のリストに追加します。
            foreach (var package in targetPackages)
            {
                var gmlFiles = datasetAccessor.GetGmlFiles(package);
                int gmlCount = gmlFiles.Length;
                for (int i = 0; i < gmlCount; i++)
                {
                    var gml = gmlFiles.At(i);

                    if (!gml.MeshCode.IsValid) continue;
                    // メッシュコードで絞り込みます。
                    if (meshCodes.All(mc => mc.ToString() != gml.MeshCode.ToString())) continue;
                    foundGmls.Add(gml);
                }
            }

            return foundGmls;
        }

        /// <summary>
        /// 範囲選択画面の結果から設定値を作ります。
        /// ここで設定する値は、範囲選択後にインポート設定GUIで表示される初期値となります。
        /// このあと、ユーザーのGUI操作によって設定値を書き換えていくことになります。
        /// </summary>
        public void InitWithAreaSelectResult(AreaSelectResult result)
        {
            InitWithPackageLodsDict(result.PackageToLodDict);
            AreaMeshCodes = result.AreaMeshCodes;
            Extent = result.Extent;
            SetReferencePointToExtentCenter();
        }
        
        /// <summary>
        /// パッケージ種とLODの組から設定値を作ります。
        /// </summary>
        private void InitWithPackageLodsDict(PackageToLodDict dict)
        {
            this.perPackagePairSettings.Clear();
            foreach (var (package, availableMaxLOD) in dict)
            {
                var predefined = CityModelPackageInfo.GetPredefined(package);
                var val = new PackageLoadSetting(
                    package: package,
                    loadPackage: true,
                    includeTexture: predefined.hasAppearance,
                    minLOD: predefined.minLOD,
                    maxLOD: availableMaxLOD,
                    availableMaxLOD: availableMaxLOD,
                    MeshGranularity.PerPrimaryFeatureObject, 
                    doSetMeshCollider: true,
                    doSetAttrInfo: true);
                this.perPackagePairSettings.Add(package, val);
            }
        }

        /// <summary>
        /// 範囲の中心を基準点として設定します。
        /// これは基準点設定GUIに表示される初期値であり、ユーザーが「範囲の中心を入力」ボタンを押したときに設定される値でもあります。
        /// </summary>
        public PlateauVector3d SetReferencePointToExtentCenter()
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(CoordinateZoneID);
            var center = geoReference.Project(Extent.Center);
            ReferencePoint = center;
            return center;
        }

        // インポート設定のうち、Unityでは必ずこうなるという定数部分です。
        internal const CoordinateSystem MeshAxes = CoordinateSystem.EUN;
        internal const float UnitScale = 1.0f;
        
        
        /// <summary>
        /// インポート設定について、C++のstructに変換します。
        /// </summary>
        internal MeshExtractOptions CreateNativeConfigFor(PredefinedCityModelPackage package)
        {
            var packageConf = GetConfigForPackage(package);
            return new MeshExtractOptions(
                referencePoint: ReferencePoint,
                meshAxes: MeshAxes,
                meshGranularity: packageConf.MeshGranularity,
                minLOD: (uint)packageConf.MinLOD,
                maxLOD: (uint)packageConf.MaxLOD,
                exportAppearance: packageConf.IncludeTexture,
                gridCountOfSide: 10,
                unitScale: UnitScale,
                coordinateZoneID: CoordinateZoneID,
                excludeCityObjectOutsideExtent: ShouldExcludeCityObjectOutsideExtent(package),
                excludePolygonsOutsideExtent: ShouldExcludePolygonsOutsideExtent(package),
                extent: Extent,
                attachMapTile: true,
                mapTileZoomLevel: 15); // TODO ここで定数で決め打っている部分は、ユーザーが選択できるようにすると良い
        } 
        
        private static bool ShouldExcludeCityObjectOutsideExtent(PredefinedCityModelPackage package)
        {
            if (package == PredefinedCityModelPackage.Relief) return false;
            return true;
        }

        private static bool ShouldExcludePolygonsOutsideExtent(PredefinedCityModelPackage package)
        {
            return !ShouldExcludeCityObjectOutsideExtent(package);
        }
    }
}
