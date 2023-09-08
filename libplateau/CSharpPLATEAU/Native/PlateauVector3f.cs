using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PlateauVector3f
    {
        public float X;
        public float Y;
        public float Z;

        public PlateauVector3f(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return $"({this.X}, {this.Y}, {this.Z})";
        }
        
        public static PlateauVector3f operator +(PlateauVector3f op1, PlateauVector3f op2)
        {
            return new PlateauVector3f(op1.X + op2.X, op1.Y + op2.Y, op1.Z + op2.Z);
        }

        public static PlateauVector3f operator -(PlateauVector3f op1, PlateauVector3f op2)
        {
            return new PlateauVector3f(op1.X - op2.X, op1.Y - op2.Y, op1.Z - op2.Z);
        }

        public static PlateauVector3f operator *(PlateauVector3f vec, float scalar)
        {
            return new PlateauVector3f(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);
        }

        public static PlateauVector3f operator /(PlateauVector3f vec, float scalar)
        {
            return new PlateauVector3f(vec.X / scalar, vec.Y / scalar, vec.Z / scalar);
        }
    }
}
