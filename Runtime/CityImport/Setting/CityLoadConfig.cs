using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;

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

        public void InitWithPackageFlags(PredefinedCityModelPackage packageFlags)
        {
            this.perPackagePairSettings.Clear();
            foreach (var package in EnumUtil.EachFlags(packageFlags))
            {
                var predefined = CityModelPackageInfo.GetPredefined(package);
                // デフォルト値で設定します。
                var val = new PackageLoadSetting(true, predefined.hasAppearance, (uint)predefined.minLOD,
                    (uint)predefined.maxLOD,
                    MeshGranularity.PerPrimaryFeatureObject, false);
                this.perPackagePairSettings.Add(package, val);
            }
        }

        /// <summary>
        /// 設定に合うGMLファイルを検索します。
        /// 多数のファイルから検索するので、実行時間が長くなりがちである点にご注意ください。
        /// </summary>
        /// <returns>検索にヒットしたGMLのリストです。</returns>
        public List<GmlFile> SearchMatchingGMLList()
        {
            using var datasetSource = DatasetSource.Create(DatasetSourceConfig);
            var datasetAccessor = datasetSource.Accessor;

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

        /// <summary>
        /// 設定の条件に合うGMLファイルを検索し、その位置の中心を求め、中心を基準点として設定します。
        /// 中心 == 基準点 を返します。
        /// </summary>
        public PlateauVector3d SearchCenterPointAndSetAsReferencePoint()
        {
            var gmls = SearchMatchingGMLList();
            var center = CalcCenterPoint(gmls, CoordinateZoneID);
            ReferencePoint = center;
            return center;
        }
        
        public static PlateauVector3d CalcCenterPoint(IEnumerable<GmlFile> targetGmls, int coordinateZoneID)
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            var geoCoordSum = new GeoCoordinate(0, 0, 0);
            int count = 0;
            foreach (var gml in targetGmls)
            {
                geoCoordSum += gml.MeshCode.Extent.Center;
                count++;
            }

            if (count == 0) return new PlateauVector3d(0,0,0);
            var centerGeo = geoCoordSum / count;
            return geoReference.Project(centerGeo);
        }
    }
}
