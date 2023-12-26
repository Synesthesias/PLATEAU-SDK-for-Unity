using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityImport.AreaSelector.Display.Gizmos.AreaRectangles;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.Util;

namespace PLATEAU.CityImport.Config
{
    /// <summary>
    /// メッシュコードのリストによって範囲を表現したクラスです。
    /// </summary>
    public class MeshCodeList
    {
        private List<MeshCode> data;
        public List<MeshCode> Data => data;

        private MeshCodeList(IEnumerable<MeshCode> data)
        {
            this.data = data.ToList();
        }

        /// <summary>
        /// 範囲選択画面の<see cref="MeshCodeGizmoDrawer"/>から選択範囲を生成します。
        /// </summary>
        internal static MeshCodeList CreateFromMeshCodeDrawers(IEnumerable<MeshCodeGizmoDrawer> drawers)
        {
            var meshCodes = new List<MeshCode>();
            foreach (var drawer in drawers)
            {
                meshCodes.AddRange(drawer.GetSelectedMeshIds().Select(MeshCode.Parse).ToList());
            }

            return new MeshCodeList(meshCodes);
        }

        /// <summary>
        /// メッシュコード番号の文字列としての配列から範囲を生成します。
        /// </summary>
        public static MeshCodeList CreateFromMeshCodesStr(IEnumerable<string> meshCodesStr)
        {
            return new MeshCodeList(meshCodesStr.Select(MeshCode.Parse).Where(code => code.IsValid));
        }

        /// <summary>
        /// 空の範囲を生成します。
        /// </summary>
        internal static MeshCodeList Empty => new MeshCodeList(new List<MeshCode>{});

        /// <summary>
        /// メッシュコードの数を返します。
        /// </summary>
        public int Count => data.Count;
        
        
        /// <summary>
        /// データセットのうち範囲を取り出したとき、利用可能なパッケージとそのLODを求めます。
        /// </summary>
        public PackageToLodDict CalcAvailablePackageLodInMeshCodes(IDatasetSourceConfig datasetSourceConfig)
        {
            using var datasetSource = DatasetSource.Create(datasetSourceConfig);
            using var accessorAll = datasetSource.Accessor;
            using var accessor = accessorAll.FilterByMeshCodes(data);
            using var gmlFiles = accessor.GetAllGmlFiles();
            var ret = new PackageToLodDict();
            int gmlCount = gmlFiles.Length;
            using var progressBar = new ProgressBar();
            for (int i = 0; i < gmlCount; i++)
            {
                var gml = gmlFiles.At(i);
                int maxLod = gml.GetMaxLod();
                ret.MergePackage(gml.Package, maxLod);

                //Progress表示
                float progress = (float)i / gmlCount;
                progressBar.Display("利用可能なデータを検索中です...", progress);
            }
            return ret;
        }

        /// <summary>
        /// インデックス指定でメッシュコードを取得します。
        /// </summary>
        public MeshCode At(int index)
        {
            return data[index];
        }

        /// <summary>
        /// 範囲の中心を返します。
        /// これは基準点設定GUIに表示される初期値であり、ユーザーが「範囲の中心を入力」ボタンを押したときに設定される値でもあります。
        /// </summary>
        public PlateauVector3d ExtentCenter(int coordinateZoneID)
        {
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);

            // 選択エリアを囲むExtentを計算
            var extent = new Extent
            {
                Min = new GeoCoordinate(180.0, 180.0, 0.0),
                Max = new GeoCoordinate(-180.0, -180.0, 0.0)
            };
            foreach (var meshCode in data)
            {
                var partialExtent = meshCode.Extent;
                extent.Min.Latitude = Math.Min(partialExtent.Min.Latitude, extent.Min.Latitude);
                extent.Min.Longitude = Math.Min(partialExtent.Min.Longitude, extent.Min.Longitude);
                extent.Max.Latitude = Math.Max(partialExtent.Max.Latitude, extent.Max.Latitude);
                extent.Max.Longitude = Math.Max(partialExtent.Max.Longitude, extent.Max.Longitude);
            }

            var center = geoReference.Project(extent.Center);
            return center;
        }
    }
}