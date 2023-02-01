using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct PlateauVector2f
    {
        public float X;
        public float Y;

        public PlateauVector2f(float x, float y)
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
