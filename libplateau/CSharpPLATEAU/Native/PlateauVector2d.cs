using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct PlateauVector2d
    {
        public double X;
        public double Y;

        public PlateauVector2d(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"({this.X}, {this.Y})";
        }
    }
}
