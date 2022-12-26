using System;
using PLATEAU.Interop;
using System.Runtime.InteropServices;

namespace PLATEAU.Dataset
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
        [MarshalAs(UnmanagedType.U1)] private bool isValid;

        public bool IsValid
        {
            get
            {
                var result = NativeMethods.plateau_mesh_code_is_valid(this, out bool isValid);
                DLLUtil.CheckDllError(result);
                return isValid;
            }
        }

        public Extent Extent
        {
            get
            {
                ThrowIfInvalid();
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
            ThrowIfInvalid();
            string secondString = Level2();
            if (this.Level == 2)
                return secondString;
            return secondString + $"{this.ThirdRow | 0}{this.ThirdCol | 0}";
        }

        public string Level2()
        {
            ThrowIfInvalid();
            return $"{this.FirstRow | 00}{this.FirstCol | 00}{this.SecondRow | 0}{this.SecondCol | 0}";
        }

        private void ThrowIfInvalid()
        {
            if (IsValid) return;
            throw new Exception("Invalid MeshCode. ( MeshCode.Invalid == true)");
        }
    }
}
