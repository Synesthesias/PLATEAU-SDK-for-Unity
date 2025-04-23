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
        private readonly int FourthRow;
        private readonly int FourthCol;
        private readonly int FifthRow;
        private readonly int FifthCol;
        public readonly int Level;
        [MarshalAs(UnmanagedType.U1)] private readonly bool isValid;

    }
}