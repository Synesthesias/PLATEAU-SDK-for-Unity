using PLATEAU.CityImport.Import;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    // 動的タイル生成をいくつかの段階に分け、各段階ごとに処理を表現できるようにします。
    //
    // ++タイミング一覧++
    // IOnTileGenerateStart タイル生成の事前処理
    // 　　　　　　　　　　↓↓
    // IOnOneTileImported  タイル1つがインポートされるごとに呼ばれる
    // 　　　　　　　　　　↓↓
    // IBeforeTileAssetBuild アセットバンドルビルド直前
    // 　　　　　　　　　　↓↓
    // IAfterTileAssetBuild　アセットバンドルビルド後
    
    
    /// <summary>
    /// 動的タイル生成の事前処理を行うインターフェースです。
    /// </summary>
    internal interface IOnTileGenerateStart
    {
        /// <returns>成否を返します。</returns>
        public bool OnTileGenerateStart();
    }

    /// <summary>
    /// 動的タイル生成で、タイル1つがインポートされるごとに呼ばれます。
    /// </summary>
    internal interface IOnOneTileImported
    {
        /// <returns>成否を返します。</returns>
        public bool OnOneTileImported(TileImportResult importResult);
    }
    
    /// <summary>
    /// 動的タイル生成でアセットバンドルのビルド直前の処理を行うインターフェイスです。
    /// </summary>
    internal interface IBeforeTileAssetBuild
    {
        /// <returns>成否を返します。</returns>
        public bool BeforeTileAssetBuild();
    }
    
    /// <summary>
    /// 動的タイル生成で、アセットバンドルビルド後の事後処理行うインターフェイスです。
    /// </summary>
    internal interface IAfterTileAssetBuild
    {
        /// <returns>成否を返します。</returns>
        public bool AfterTileAssetBuild();
    }
}