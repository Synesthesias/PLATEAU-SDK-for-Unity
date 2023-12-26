using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Geometries
{
    /// <summary>
    /// 各列挙子について、3つのアルファベットはXYZ軸がどの方角、方向になるかを表しています。<br/>
    /// N,S,E,Wはそれぞれ北,南,東,西<br/>
    /// U,Dはそれぞれ上,下<br/>
    /// に対応します。<br/>
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// PLATEAUでの座標系
        /// </summary>
        ENU = 0,
        WUN = 1,
        /// <summary>
        /// Unreal Engineでの座標系
        /// </summary>
        ESU = 2,
        /// <summary>
        /// Unityでの座標系
        /// </summary>
        EUN = 3
    }

    public static class CoordinateSystemExtension
    {
        public static string ToNaturalLanguage(this CoordinateSystem system)
        {
            switch (system)
            {
                case CoordinateSystem.ENU:
                    return "ENU(PLATEAUでの座標系)";
                case CoordinateSystem.ESU:
                    return "ESU(Unreal Engineでの座標系)";
                case CoordinateSystem.EUN:
                    return "EUN(Unityでの座標系)";
                case CoordinateSystem.WUN:
                    return "WUN(EUNの左右反転補正)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(system), "Unknown coordinate system.");
            }
        }
    }

    /// <summary>
    /// 極座標と平面直角座標を変換します。
    /// また座標変換の基準を保持します。
    /// </summary>
    public class GeoReference : PInvokeDisposable
    {
        private GeoReference(IntPtr geoReferencePtr) : base(geoReferencePtr)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referencePoint">
        /// 平面直角座標に変換したあと、この座標が原点となるように並行移動します。
        /// </param>
        /// <param name="unitScale">
        /// 平面直角座標に変換したあと、拡大縮小します。
        /// </param>
        /// <param name="coordinateSystem">
        /// 平面直角座標のX,Y,Z軸の向きを決めます。
        /// </param>
        /// <param name="zoneID">
        /// 国土交通省告示第九号に基づく平面直角座標系の原点の番号です。
        /// 関東地方では 9 を選択すると歪みが少なくなりますが、
        /// この値を間違えても、ぱっと見ですぐ分かるような歪みにはなりません。
        /// 詳しくはこちらを参照してください :
        /// https://www.gsi.go.jp/sokuchikijun/jpc.html
        /// </param>
        public static GeoReference Create(PlateauVector3d referencePoint, float unitScale, CoordinateSystem coordinateSystem,
            int zoneID)
        {
            var result = NativeMethods.plateau_create_geo_reference(
                out var geoReferencePtr, referencePoint, unitScale,
                coordinateSystem, zoneID
            );
            DLLUtil.CheckDllError(result);
            return new GeoReference(geoReferencePtr);
        }

        public PlateauVector3d Project(GeoCoordinate geoCoordinate)
        {
            var result = NativeMethods.plateau_geo_reference_project(
                Handle, out var outXyz,
                geoCoordinate);
            DLLUtil.CheckDllError(result);
            return outXyz;
        }

        public GeoCoordinate Unproject(PlateauVector3d point)
        {
            var result = NativeMethods.plateau_geo_reference_unproject(
                Handle, out var outLatLon,
                point);
            DLLUtil.CheckDllError(result);
            return outLatLon;
        }

        public static PlateauVector3d ConvertAxisToENU(CoordinateSystem axis, PlateauVector3d vertex)
        {
            var result = NativeMethods.plateau_geo_reference_convert_axis_to_enu(axis, vertex, out var outVertex);
            DLLUtil.CheckDllError(result);
            return outVertex;
        }

        public static PlateauVector3d ConvertAxisFromENUTo(CoordinateSystem axis, PlateauVector3d vertex)
        {
            var result = NativeMethods.plateau_geo_reference_convert_axis_from_enu_to(axis, vertex, out var outVertex);
            DLLUtil.CheckDllError(result);
            return outVertex;
        }

        public PlateauVector3d ReferencePoint =>
            DLLUtil.GetNativeValue<PlateauVector3d>(Handle,
                NativeMethods.plateau_geo_reference_get_reference_point);

        public int ZoneID =>
            DLLUtil.GetNativeValue<int>(Handle,
                NativeMethods.plateau_geo_reference_get_zone_id);

        public float UnitScale =>
            DLLUtil.GetNativeValue<float>(Handle,
                NativeMethods.plateau_geo_reference_get_unit_scale);

        public CoordinateSystem CoordinateSystem =>
            DLLUtil.GetNativeValue<CoordinateSystem>(Handle,
                NativeMethods.plateau_geo_reference_get_coordinate_system);

        protected override void DisposeNative()
        {
            DLLUtil.ExecNativeVoidFunc(Handle, NativeMethods.plateau_delete_geo_reference);
        }

        ~GeoReference()
        {
            Dispose();
        }

        public static string[] ZoneIdExplanation => new[]
        {
            "01: 長崎, 鹿児島(南西部)",
            "02: 福岡, 佐賀, 熊本, 大分, 宮崎, 鹿児島(北東部)",
            "03: 山口, 島根, 広島",
            "04: 香川, 愛媛, 徳島, 高知",
            "05: 兵庫, 鳥取, 岡山",
            "06: 京都, 大阪, 福井, 滋賀, 三重, 奈良, 和歌山",
            "07: 石川, 富山, 岐阜, 愛知",
            "08: 新潟, 長野, 山梨, 静岡",
            "09: 東京(本州), 福島, 栃木, 茨城, 埼玉, 千葉, 群馬, 神奈川",
            "10: 青森, 秋田, 山形, 岩手, 宮城",
            "11: 北海道(西部)",
            "12: 北海道(中央部)",
            "13: 北海道(東部)",
            "14: 諸島(東京南部)",
            "15: 沖縄",
            "16: 諸島(沖縄西部)",
            "17: 諸島(沖縄東部)",
            "18: 小笠原諸島",
            "19: 南鳥島"
        };

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_geo_reference(
                out IntPtr outGeoReferencePtr,
                PlateauVector3d referencePoint,
                float unitScale,
                CoordinateSystem coordinateSystem,
                int zoneId);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_geo_reference(
                [In] IntPtr geoReferencePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_project(
                [In] IntPtr geoReferencePtr,
                out PlateauVector3d outXyz,
                GeoCoordinate latLon);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_unproject(
                [In] IntPtr geoReferencePtr,
                out GeoCoordinate outLatLon,
                PlateauVector3d point);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_get_reference_point(
                [In] IntPtr handle,
                out PlateauVector3d outReferencePoint);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_get_zone_id(
                [In] IntPtr handle,
                out int zoneID);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_get_unit_scale(
                [In] IntPtr handle,
                out float unitScale);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_get_coordinate_system(
                [In] IntPtr handle,
                out CoordinateSystem outCoordinateSystem);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_convert_axis_to_enu(
                CoordinateSystem axis,
                PlateauVector3d vertex, 
                out PlateauVector3d outVertex);


            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geo_reference_convert_axis_from_enu_to(
                CoordinateSystem axis,
                PlateauVector3d vertex, 
                out PlateauVector3d outVertex);

        }
    }
}
