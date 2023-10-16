using System;
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
        public string[] AreaMeshCodes { get; private set; }
        
        /// <summary>
        /// 平面直角座標系の番号です。
        /// 次のサイトで示される平面直角座標系の番号です。
        /// https://www.gsi.go.jp/sokuchikijun/jpc.html
        /// </summary>
        public int CoordinateZoneID { get; set; } = 9;
        
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


            // 地域ID(メッシュコード)で絞り込みます。
            var meshCodes = AreaMeshCodes.Select(MeshCode.Parse).Where(code => code.IsValid).ToArray();

            using var datasetSource = DatasetSource.Create(DatasetSourceConfig);
            using var datasetAccessor = datasetSource.Accessor.FilterByMeshCodes(meshCodes);

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
                this.perPackagePairSettings.Add(package, PackageLoadSetting.CreateSettingFor(package, availableMaxLOD));
            }
        }

        /// <summary>
        /// 範囲の中心を基準点として設定します。
        /// これは基準点設定GUIに表示される初期値であり、ユーザーが「範囲の中心を入力」ボタンを押したときに設定される値でもあります。
        /// </summary>
        public PlateauVector3d SetReferencePointToExtentCenter()
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(CoordinateZoneID);

            // 選択エリアを囲むExtentを計算
            var Extent = new Extent();
            Extent.Min = new GeoCoordinate(180.0, 180.0, 0.0);
            Extent.Max = new GeoCoordinate(-180.0, -180.0, 0.0);
            for (var i = 0; i < AreaMeshCodes.Length; ++i)
            {
                var PartialExtent = MeshCode.Parse(AreaMeshCodes[i]).Extent;
                Extent.Min.Latitude = Math.Min(PartialExtent.Min.Latitude, Extent.Min.Latitude);
                Extent.Min.Longitude = Math.Min(PartialExtent.Min.Longitude, Extent.Min.Longitude);
                Extent.Max.Latitude = Math.Max(PartialExtent.Max.Latitude, Extent.Max.Latitude);
                Extent.Max.Longitude = Math.Max(PartialExtent.Max.Longitude, Extent.Max.Longitude);
            }

            var center = geoReference.Project(Extent.Center);
            ReferencePoint = center;
            return center;
        }
        /// <summary>
        /// インポート設定について、C++のstructに変換します。
        /// </summary>
        internal MeshExtractOptions CreateNativeConfigFor(PredefinedCityModelPackage package)
        {
            var packageConf = GetConfigForPackage(package);
            return packageConf.ConvertToNativeOption(ReferencePoint, CoordinateZoneID);
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
