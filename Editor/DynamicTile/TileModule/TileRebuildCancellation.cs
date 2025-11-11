
using PLATEAU.DynamicTile;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    internal class TileRebuildCancellation : IAfterTileAssetBuild
    {
        public bool AfterTileAssetBuild()
        {
            // タイル生成中フラグを設定
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
            return true;
        }
    }
}
