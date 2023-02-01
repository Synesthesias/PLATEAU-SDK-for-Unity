using System;
using PLATEAU.Interop;
using System.Runtime.InteropServices;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshCode
    {
        private readonly int FirstRow;
        private readonly int FirstCol;
        private readonly int SecondRow;
        private readonly int SecondCol;
        private readonly int ThirdRow;
        private readonly int ThirdCol;
        public readonly int Level;
        [MarshalAs(UnmanagedType.U1)] private readonly bool isValid;

        public bool IsValid
        {
            get
            {
                var result = NativeMethods.plateau_mesh_code_is_valid(this, out bool resultIsValid);
                DLLUtil.CheckDllError(result);
                return resultIsValid;
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

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern MeshCode plateau_mesh_code_parse(
                [In] string code);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_code_get_extent(
                [In] MeshCode meshCode,
                [In, Out] ref Extent outExtent);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_code_is_valid(
                [In] MeshCode meshCode,
                [MarshalAs(UnmanagedType.U1)]out bool outIsValid);
        }
    }
}
