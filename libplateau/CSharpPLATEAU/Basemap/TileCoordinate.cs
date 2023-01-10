using System.Runtime.InteropServices;

namespace PLATEAU.Basemap
{
    /// <summary>
    /// 地理院地図のタイル座標です。
    /// タイル座標については国土地理院のWebサイトを参照してください。
    /// <see href="https://maps.gsi.go.jp/development/siyou.html#siyou-zm"/>
    /// データ構造は C++側と合わせる必要があります。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TileCoordinate
    {
        public readonly int Column;
        public readonly int Row;
        public readonly int ZoomLevel;

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
