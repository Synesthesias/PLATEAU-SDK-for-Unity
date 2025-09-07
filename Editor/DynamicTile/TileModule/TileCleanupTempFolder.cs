namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// タイル生成時の一時フォルダーを削除します
    /// </summary>
    internal class TileCleanupTempFolder : IAfterTileAssetBuild, IOnTileGenerationCancelled
    {
        public bool AfterTileAssetBuild()
        {
            CleanupTempFolder();
            return true;
        }
        
        public void OnTileGenerationCancelled()
        {
            CleanupTempFolder();
        }

        private void CleanupTempFolder()
        {
            // FIXME 現状、Assets外のアプリビルド後に動かすためにはここはコメントアウトする必要あり
            
            // var assetPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            // if (AssetDatabase.DeleteAsset(assetPath))
            // {
            //     AssetDatabase.Refresh();
            //     Debug.Log($"一時フォルダーを削除しました: {assetPath}");
            // }
            // else
            // {
            //     Debug.Log($"一時フォルダーなし: {assetPath}"); // Assets内のケース
            // }
            //
        }
    }
}