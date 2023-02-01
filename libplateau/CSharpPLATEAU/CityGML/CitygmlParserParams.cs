using System.Runtime.InteropServices;

namespace PLATEAU.CityGML
{
    /// <summary>
    ///  GMLファイルのパース時の設定です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CitygmlParserParams
    {
        [MarshalAs(UnmanagedType.U1)]
        private readonly bool Optimize;
        /// <summary>
        /// <see cref="Tessellate"/> を false に設定すると、 <see cref="Polygon"/> が頂点を保持する代わりに <see cref="LinearRing"/> を保持することがあります。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        private readonly bool Tessellate;

        [MarshalAs(UnmanagedType.U1)]
        private readonly bool IgnoreGeometries;

        public CitygmlParserParams(bool optimize, bool tessellate, bool ignoreGeometries)
        {
            this.Optimize = optimize;
            this.Tessellate = tessellate;
            this.IgnoreGeometries = ignoreGeometries;
        }
    }
}
