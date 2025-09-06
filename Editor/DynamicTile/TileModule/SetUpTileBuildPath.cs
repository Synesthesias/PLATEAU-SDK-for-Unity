using System.IO;
using PLATEAU.DynamicTile;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルインポートでのビルドパスを設定します。
    /// </summary>
    internal class SetUpTileBuildPath : IBeforeTileExport
    {
        private DynamicTileProcessingContext context;

        public SetUpTileBuildPath(DynamicTileProcessingContext context)
        {
            this.context = context;
        }
        
        public bool BeforeTileExport()
        {
            // プロジェクト内であればアセットバンドルをStreamingAssets以下に出力します。
            // なぜならデフォルトのローカルビルドパスであるLibrary以下は、2回目にプロジェクト外に出力した時にクリアされカタログが読めなくなるためです。
            // プロジェクト外であればユーザー指定のフォルダをそのまま使用します。
                

            // ビルド先パスを決定
            if (context.IsExcludeAssetFolder)
            {
                // プロジェクト外: ユーザー指定のフォルダーをそのまま使用
                // noop
            }
            else
            {
                // プロジェクト内: StreamingAssets/PLATEAUBundles/{GroupName}
                var bundleOutputPath = Path.Combine(
                    Application.streamingAssetsPath,
                    AddressableLoader.AddressableLocalBuildFolderName,
                    context.AddressableGroupName);
                bundleOutputPath = PathUtil.FullPathToAssetsPath(bundleOutputPath);
                context.BuildFolderPath = bundleOutputPath;
            }

            return true;
        }
    }
}