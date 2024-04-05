using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    /// <summary>
    /// オブジェクトの向きを共通ライブラリとやりとりするためのクラスです。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlateauQuaternion
    {
        // フィールドの定義順はC++と完全に一致する必要があります。
        public double X;
        public double Y;
        public double Z;
        public double W;
        
        public PlateauQuaternion(double x, double y, double z, double w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
    }
}
