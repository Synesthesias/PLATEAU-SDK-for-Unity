using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    /// <summary>
    /// 最小点と最大点の2点で表現される範囲であり、
    /// 各点は緯度・経度・高さで表現されます。
    /// すなわち、2つの <see cref="GeoCoordinate"/> からなる範囲表現です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Extent
    {
        public GeoCoordinate Min;
        public GeoCoordinate Max;

        public Extent(GeoCoordinate min, GeoCoordinate max)
        {
            this.Min = min;
            this.Max = max;
        }

        public GeoCoordinate Center => new GeoCoordinate(
            (this.Min.Latitude + this.Max.Latitude) * 0.5,
            (this.Min.Longitude + this.Max.Longitude) * 0.5,
            (this.Min.Height + this.Max.Height) * 0.5);

        /// <summary>
        /// 共通部分を返します。
        /// なければ (-99, -99, -99), (-99, -99, -99)を返します。
        /// 高さを考慮しない場合は、ignoreHeightをtrueにします。
        /// </summary>
        public static Extent Intersection(Extent op1, Extent op2, bool ignoreHeight = false)
        {
            var max = GeoCoordinate.Max(op1.Max, op2.Max);
            var min = GeoCoordinate.Min(op1.Min, op2.Min);
            var intersectSize = op1.Size() + op2.Size() - (max - min);
            if (intersectSize.Latitude <= 0 || intersectSize.Latitude <= 0 || (!ignoreHeight && intersectSize.Height <= 0))
                return new Extent(new GeoCoordinate(-99,-99,-99), new GeoCoordinate(-99, -99, -99));
            var minMax = GeoCoordinate.Min(op1.Max, op2.Max);
            var maxMin = GeoCoordinate.Max(op1.Min, op2.Min);
            return new Extent(maxMin, minMax);
        }

        public GeoCoordinate Size()
        {
            return this.Max - this.Min;
        }

        public static readonly Extent All =
            new Extent(
                new GeoCoordinate(-90, -180, -9999),
                new GeoCoordinate(90, 180, 9999));
         
        public override string ToString()
        {
            return $"Extent: (Min={this.Min}, Max={this.Max})";
        }
    }
}
