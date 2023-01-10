using System;
using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    /// <summary>
    /// 緯度・経度・高さで表現される座標です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GeoCoordinate
    {
        public double Latitude;
        public double Longitude;
        public double Height;

        public GeoCoordinate(double lat, double lon, double height)
        {
            this.Latitude = lat;
            this.Longitude = lon;
            this.Height = height;
        }

        public override string ToString()
        {
            return $"GeoCoordinate: (Lat={this.Latitude}, Lon={this.Longitude}, Height={this.Height})";
        }

        public static GeoCoordinate Min(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                Math.Min(op1.Latitude, op2.Latitude),
                Math.Min(op1.Longitude, op2.Longitude),
                Math.Min(op1.Height, op2.Height)
            );
        }

        public static GeoCoordinate Max(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                Math.Max(op1.Latitude, op2.Latitude),
                Math.Max(op1.Longitude, op2.Longitude),
                Math.Max(op1.Height, op2.Height)
            );
        }
        
        public static GeoCoordinate operator +(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                op1.Latitude + op2.Latitude,
                op1.Longitude + op2.Longitude,
                op1.Height + op2.Height
            );
        }

        public static GeoCoordinate operator -(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                op1.Latitude - op2.Latitude,
                op1.Longitude - op2.Longitude,
                op1.Height - op2.Height
            );
        }

        public static GeoCoordinate operator *(GeoCoordinate geo, double scalar)
        {
            return new GeoCoordinate(
                geo.Latitude * scalar,
                geo.Longitude * scalar,
                geo.Height * scalar
            );
        }

        public static GeoCoordinate operator /(GeoCoordinate geo, double scalar)
        {
            if (Math.Abs(scalar) <= double.Epsilon) throw new DivideByZeroException();
            return geo * (1.0 / scalar);
        }

        /// <summary>
        /// 緯度、経度の値を2次元ベクトルとして見たときのベクトルの長さの2乗です。
        /// 高さは無視されます。
        /// </summary>
        public double SqrMagnitudeLatLon => this.Latitude * this.Latitude + this.Longitude * this.Longitude;
    }
}
