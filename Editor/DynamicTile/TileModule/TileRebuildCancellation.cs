
using PLATEAU.DynamicTile;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    internal class TileRebuildCancellation : IAfterTileAssetBuild
    {
        public bool AfterTileAssetBuild()
        {
            // タイル生成中フラグを解除
            PLATEAUEditorEventListener.disableProjectChangeEvent = false;
            return true;
        }
    }
}
