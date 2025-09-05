namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルエクスポートの事前処理を行うインターフェースです。
    /// </summary>
    internal interface IBeforeTileExport
    {
        /// <summary> 成否を返します。 </summary>
        public bool BeforeTileExport();
    }
}