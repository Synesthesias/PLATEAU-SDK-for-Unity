using PLATEAU.DynamicTile;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// タイル生成時の一時フォルダーを削除します
    /// </summary>
    internal class TileCleanupTempFolder : IAfterTileAssetBuild
    {
        private readonly DynamicTileProcessingContext context;

        public TileCleanupTempFolder(DynamicTileProcessingContext context)
        {
            this.context = context;
        }


        public bool AfterTileAssetBuild()
        {
            CleanupTempFolder();
            
            return true;
        }


        private void CleanupTempFolder()
        {
            var assetPath = DynamicTileProcessingContext.PrefabsTempSavePath;
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                Debug.Log($"一時フォルダーなし: {assetPath}"); // Assets内のケース
                return;
            }
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                AssetDatabase.Refresh();
                Debug.Log($"一時フォルダーを削除しました: {assetPath}");
            }
            else
            {
                Debug.LogWarning("一時フォルダの削除に失敗しました: " + assetPath);
            }
        }
    }
}