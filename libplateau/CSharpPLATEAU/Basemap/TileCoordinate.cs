using System.Runtime.InteropServices;

namespace PLATEAU.Basemap
{
    /// <summary>
    /// 地理院地図のタイル座標です。
    /// タイル座標については国土地理院のWebサイトを参照してください。
    /// <see href="https://maps.gsi.go.jp/development/siyou.html#siyou-zm"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCoordinate
    {
        public int Column;
        public int Row;
        public int ZoomLevel;

        public TileCoordinate(int column, int row, int zoomLevel)
        {
            this.Column = column;
            this.Row = row;
            this.ZoomLevel = zoomLevel;
        }

        public override string ToString()
        {
            return $"TileCoordinate: (Column={this.Column}, Row={this.Row}, ZoomLevel={this.ZoomLevel})";
        }
    }
}
