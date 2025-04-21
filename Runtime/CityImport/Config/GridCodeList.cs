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
    /// グリッドコードのリストによって範囲を表現したクラスです。
    /// </summary>
    public class GridCodeList : IDisposable
    {
        private NativeVectorGridCode gridCodes;
        public NativeVectorGridCode GridCodes => gridCodes;
        private bool disposed;

        private GridCodeList(NativeVectorGridCode gridCodes)
        {
            this.gridCodes = gridCodes;
        }

        /// <summary>
        /// 範囲選択画面の<see cref="GridCodeGizmoDrawer"/>から選択範囲を生成します。
        /// </summary>
        internal static GridCodeList CreateFromGridCodeDrawers(IEnumerable<GridCodeGizmoDrawer> drawers)
        {
            var gridCodes = NativeVectorGridCode.Create();
            foreach (var drawer in drawers)
            {
                foreach (var gridID in drawer.GetSelectedGridIds())
                {
                    using var gridCode = GridCode.Create(gridID);
                    gridCodes.AddCopyOf(gridCode);
                }
            }

            return new GridCodeList(gridCodes);
        }

        /// <summary>
        /// グリッド番号の文字列としての配列から範囲を生成します。
        /// </summary>
        public static GridCodeList CreateFromGridCodesStr(IEnumerable<string> gridCodesStr)
        {
            var gridCodes = NativeVectorGridCode.Create();
            foreach (var gridStr in gridCodesStr)
            {
                using var gridCode = GridCode.Create(gridStr);
                gridCodes.AddCopyOf(gridCode);
            }

            return new GridCodeList(gridCodes);
        }

        /// <summary>
        /// 空の範囲を生成します。
        /// </summary>
        internal static GridCodeList Empty => new GridCodeList(NativeVectorGridCode.Create());

        /// <summary>
        /// メッシュコードの数を返します。
        /// </summary>
        public int Count => gridCodes.Length;
        
        
        /// <summary>
        /// データセットのうち範囲を取り出したとき、利用可能なパッケージとそのLODを求めます。
        /// </summary>
        public PackageToLodDict CalcAvailablePackageLodInGridCodes(IDatasetSourceConfig datasetSourceConfig)
        {
            using var datasetSource = DatasetSource.Create(datasetSourceConfig);
            using var accessorAll = datasetSource.Accessor;
            using var accessor = accessorAll.FilterByGridCodes(gridCodes);
            using var gmlFiles = accessor.GetAllGmlFiles();
            var ret = new PackageToLodDict();
            int gmlCount = gmlFiles.Length;
            if (gmlCount == 0)
            {
                return ret;
            }
            using var progressBar = new ProgressBar();
            for (int i = 0; i < gmlCount; i++)
            {
                var gml = gmlFiles.At(i);
                int maxLod = gml.GetMaxLod();
                ret.MergePackage(gml.Package, maxLod);

                //Progress表示
                float progress = (float)(i+1) / gmlCount;
                progressBar.Display("利用可能なデータを検索中です...", progress);
            }
            return ret;
        }

        /// <summary>
        /// インデックス指定でグリッドコードを取得します。
        /// </summary>
        public GridCode At(int index)
        {
            return gridCodes.At(index);
        }

        /// <summary>
        /// 範囲の中心を返します。
        /// これは基準点設定GUIに表示される初期値であり、ユーザーが「範囲の中心を入力」ボタンを押したときに設定される値でもあります。
        /// </summary>
        public PlateauVector3d ExtentCenter(int coordinateZoneID)
        {
            if (gridCodes.Length == 0)
            {
                throw new Exception("グリッドコードがありません");
            }
            using var geoReference = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);

            // 選択エリアを囲むExtentを計算
            var extent = new Extent
            {
                Min = new GeoCoordinate(double.MaxValue, double.MaxValue, 0.0),
                Max = new GeoCoordinate(double.MinValue, double.MinValue, 0.0)
            };
            for (int i = 0; i < gridCodes.Length; i++)
            {
                var gridCode = gridCodes.At(i);
                if (!gridCode.IsValid) continue;
                var partialExtent = gridCode.Extent;
                extent.Min.Latitude = Math.Min(partialExtent.Min.Latitude, extent.Min.Latitude);
                extent.Min.Longitude = Math.Min(partialExtent.Min.Longitude, extent.Min.Longitude);
                extent.Max.Latitude = Math.Max(partialExtent.Max.Latitude, extent.Max.Latitude);
                extent.Max.Longitude = Math.Max(partialExtent.Max.Longitude, extent.Max.Longitude);
            }

            var center = geoReference.Project(extent.Center);
            return center;
        }

        ~GridCodeList() => Dispose(false);
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            gridCodes?.Dispose();
            gridCodes = null;
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}