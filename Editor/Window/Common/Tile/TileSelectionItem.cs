using System;

namespace PLATEAU.Editor.Window.Common.Tile
{
    /// <summary>
    /// リストに表示するタイル選択アイテム
    /// </summary>
    public class TileSelectionItem
    {
        public string TileAddress { get; private set; }
        public string TilePath { get; private set; }

        public bool IsTile => string.Equals(TileAddress, TilePath); // Tileでない場合はタイル内のオブジェクトを指す

        public TileSelectionItem(string fullpath)
        {
            if (string.IsNullOrEmpty(fullpath))
                throw new ArgumentException("fullpath は null または空にできません。", nameof(fullpath));

            TilePath = fullpath;
            // path( 例: "xxxxx_op/LODx/xxxx-xxx") からタイルのAddress(例: "xxxxx_op")を取得する
            var splits = fullpath.Split('/');
            TileAddress = splits[0];
        }
    }
}
