using PLATEAU.Interop;
using System.Runtime.InteropServices;

namespace PLATEAU.Udx
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshCode
    {
        public int FirstRow;
        public int FirstCol;
        public int SecondRow;
        public int SecondCol;
        public int ThirdRow;
        public int ThirdCol;
        public int Level;

        public Extent Extent
        {
            get
            {
                Extent value = new Extent();
                APIResult result = NativeMethods.plateau_mesh_code_get_extent(this, ref value);
                DLLUtil.CheckDllError(result);
                return value;
            }
        }

        public static MeshCode Parse(string code)
        {
            return NativeMethods.plateau_mesh_code_parse(code);
        }

        public override string ToString()
        {
            string secondString = $"{this.FirstRow | 00}{this.FirstCol | 00}{this.SecondRow | 0}{this.SecondCol | 0}";
            if (this.Level == 2)
                return secondString;
            return secondString + $"{this.ThirdRow | 0}{this.ThirdCol | 0}";
        }
    }
}
