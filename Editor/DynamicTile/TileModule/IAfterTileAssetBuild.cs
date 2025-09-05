namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルエクスポートで、アセットバンドルビルド後の事後処理行うインターフェイスです。
    /// </summary>
    internal interface IAfterTileAssetBuild
    {
        public bool AfterTileAssetBuild(); // 成否を返します。
    }
}