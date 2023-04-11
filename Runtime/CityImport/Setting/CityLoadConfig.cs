using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// 都市インポートの設定です。
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

        // 都市モデル読み込みの、パッケージ種ごとの設定です。
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
        
        public void InitWithPackageLodsDict(PackageToLodDict dict)
        {
            this.perPackagePairSettings.Clear();
            foreach (var pair in dict)
            {
                var package = pair.Key;
                var maxLod = pair.Value;
                var predefined = CityModelPackageInfo.GetPredefined(package);
                var val = new PackageLoadSetting(true, predefined.hasAppearance, predefined.minLOD,
                    maxLod, MeshGranularity.PerPrimaryFeatureObject, false);
                this.perPackagePairSettings.Add(package, val);
            }
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
                    .Where(pair => pair.Value.loadPackage)
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

        public void InitWithAreaSelectResult(AreaSelectResult result)
        {
            InitWithPackageLodsDict(result.PackageToLodDict);
            AreaMeshCodes = result.AreaMeshCodes;
            Extent = result.Extent;
            SetReferencePointToExtentCenter();
        }

        /// <summary>
        /// 範囲の中心を基準点として設定します。
        /// </summary>
        public PlateauVector3d SetReferencePointToExtentCenter()
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(CoordinateZoneID);
            var center = geoReference.Project(Extent.Center);
            ReferencePoint = center;
            return center;
        }
    }
}
