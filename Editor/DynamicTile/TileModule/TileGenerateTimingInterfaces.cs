using PLATEAU.CityImport.Import;
using System.Threading;
using System.Threading.Tasks;

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
    //
    // キャンセル時: IOnTileGenerationCancelled
    
    
    /// <summary>
    /// 動的タイル生成の事前処理を行うインターフェースです。
    /// </summary>
    internal interface IOnTileGenerateStart
    {
        /// <summary> 事前処理を行います。</summary>
        /// <returns>成否を返します。</returns>
        bool OnTileGenerateStart();

        /// <summary>例外が補捉れた場合の処理です。</summary>
        void OnTileGenerateStartFailed();
    }

    /// <summary>
    /// 動的タイル生成で、タイル1つがインポートされるごとに呼ばれます。
    /// </summary>
    internal interface IOnOneTileImported
    {
        /// <returns>成否を返します。</returns>
        bool OnOneTileImported(TileImportResult importResult);
    }
    
    /// <summary>
    /// 動的タイル生成でアセットバンドルのビルド直前の処理を行うインターフェイスです。
    /// </summary>
    internal interface IBeforeTileAssetBuild
    {
        /// <returns>成否を返します。</returns>
        Task<bool> BeforeTileAssetBuildAsync(CancellationToken ct);
    }
    
    /// <summary>
    /// 動的タイル生成で、アセットバンドルビルド後の事後処理行うインターフェイスです。
    /// </summary>
    internal interface IAfterTileAssetBuild
    {
        /// <returns>成否を返します。</returns>
        bool AfterTileAssetBuild();
    }

    internal interface IOnTileGenerationCancelled
    {
        void OnTileGenerationCancelled();
    }

    internal interface IOnTileBuildFailed
    {
        void OnTileBuildFailed();
    }
}