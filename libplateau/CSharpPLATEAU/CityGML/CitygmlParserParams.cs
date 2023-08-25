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
        private bool optimize;

        [MarshalAs(UnmanagedType.U1)]
        private bool keepVertices;

        [MarshalAs(UnmanagedType.U1)]
        private bool tessellate;

        [MarshalAs(UnmanagedType.U1)]
        private bool ignoreGeometries;

        public bool Optimize
        {
            get => this.optimize; set => this.optimize = value;
        }

        public bool KeepVertices
        {
            get => this.keepVertices; set => this.keepVertices = value;
        }

        /// <summary>
        /// <see cref="Tessellate"/> を false に設定すると、 <see cref="Polygon"/> が頂点を保持する代わりに <see cref="LinearRing"/> を保持します。
        /// </summary>
        public bool Tesselate
        {
            get => this.tessellate; set => this.tessellate = value;
        }

        public bool IgnoreGeometries
        {
            get => this.ignoreGeometries; set => this.ignoreGeometries = value;
        }

        public CitygmlParserParams(bool optimize, bool keepVertices, bool tessellate, bool ignoreGeometries)
        {
            this.optimize = optimize;
            this.keepVertices = keepVertices;
            this.tessellate = tessellate;
            this.ignoreGeometries = ignoreGeometries;
        }
    }
}
