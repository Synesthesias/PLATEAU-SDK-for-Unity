using PLATEAU.DynamicTile;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// タイル生成時の一時フォルダーを削除します
    /// </summary>
    internal class TileCleanupTempFolder : IOnTileGenerateStart
    {
        private readonly DynamicTileProcessingContext context;

        public TileCleanupTempFolder(DynamicTileProcessingContext context)
        {
            this.context = context;
        }


        public bool OnTileGenerateStart()
        {
            // 連続で同じフォルダが指定されたときはタイルを追加するので、そのケースはクリーンアップをしません。
            // そのため、前と違うフォルダが指定されたときの処理前に初めてクリーンアップします。
            if (!context.IsSameOutputPathAsPrevious)
            {
                CleanupTempFolder();
            }

            
            return true;
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
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