using PLATEAU.DynamicTile;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// リビルド時に、前回のビルドで生成された addressables_content_state.bin を読み込み、
    /// Addressableのビルド設定に反映させます。
    /// これにより、Addressableの差分ビルドが正しく機能するようにします。
    /// </summary>
    public class RestoreAddressablesStateForRebuild : IOnTileGenerateStart
    {
        private readonly DynamicTileProcessingContext context;
        private const string ContentStateFileName = "addressables_content_state.bin";

        public RestoreAddressablesStateForRebuild(DynamicTileProcessingContext context)
        {
            this.context = context;
        }

        public bool OnTileGenerateStart()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
                return true; // 致命的ではないので続ける
            }

            // 差分ビルドに必要な state ファイルを探します。
            var contentStatePath = Path.Combine(context.BuildFolderPath, ContentStateFileName);
            
            if (File.Exists(contentStatePath))
            {
                // state ファイルの存在する場所を Addressable の設定に教えます。
                // これにより、ContentUpdateScript.BuildContentUpdate がこのファイルを読んで差分を計算します。
                settings.ContentStateBuildPath = context.BuildFolderPath;
                Debug.Log($"Addressables content state found and set for incremental build: {context.BuildFolderPath}");
            }
            else
            {
                // state ファイルがない場合、通常は初回ビルドなので何もしません。
                // New Buildが実行されます。
                Debug.Log("Addressables content state not found. A full build will be performed.");
            }
            
            return true;
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
        }
    }
}
