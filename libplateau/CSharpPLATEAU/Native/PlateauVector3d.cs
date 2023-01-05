using System.Runtime.InteropServices;

namespace PLATEAU.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PlateauVector3d
    {
        public double X;
        public double Y;
        public double Z;

        public PlateauVector3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return $"({this.X}, {this.Y}, {this.Z})";
        }
        
        public static PlateauVector3d operator+(PlateauVector3d op1, PlateauVector3d op2)
        {
            return new PlateauVector3d(op1.X + op2.X, op1.Y + op2.Y, op1.Z + op2.Z);
        }

        public static PlateauVector3d operator-(PlateauVector3d op1, PlateauVector3d op2)
        {
            return new PlateauVector3d(op1.X - op2.X, op1.Y - op2.Y, op1.Z - op2.Z);
        }

        public static PlateauVector3d operator*(PlateauVector3d vec, double scalar)
        {
            return new PlateauVector3d(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);
        }

        public static PlateauVector3d operator /(PlateauVector3d vec, double scalar)
        {
            return new PlateauVector3d(vec.X / scalar, vec.Y / scalar, vec.Z / scalar);
        }
    }
}
