using PLATEAU.DynamicTile;
using PLATEAU.Util;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
	/// <summary>
	/// アセットバンドルのビルド直前に、出力先フォルダ内の旧.bundle類を削除します。
	/// Content Update 用の content state などは保持します。
	/// </summary>
internal class DeleteOldBundlesBeforeBuild : IBeforeTileAssetBuild
	{
		private readonly DynamicTileProcessingContext context;

    public DeleteOldBundlesBeforeBuild(DynamicTileProcessingContext context)
		{
			this.context = context;
		}

    public Task<bool> BeforeTileAssetBuildAsync()
    {
        // ファイル名にアドレスが含まれる前提で、指定アドレスを含む旧bundleのみ削除
        try
        {
            var buildFolder = context.BuildFolderPath;
            if (string.IsNullOrEmpty(buildFolder)) return Task.FromResult(true);

            string fullPath;
            if (PathUtil.IsSubDirectoryOfAssets(buildFolder))
            {
                fullPath = AssetPathUtil.GetFullPath(buildFolder);
            }
            else
            {
                fullPath = buildFolder;
            }

            if (!Directory.Exists(fullPath)) return Task.FromResult(true);

            var targets = context.TargetAddresses;
            if (targets == null || targets.Count == 0) return Task.FromResult(true);

            var targetSubstrings = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var addr in targets)
            {
                if (string.IsNullOrEmpty(addr)) continue;
                targetSubstrings.Add(addr);
            }

            var bundleFiles = Directory.GetFiles(fullPath, "*.bundle", SearchOption.TopDirectoryOnly);
            foreach (var f in bundleFiles)
            {
                var name = Path.GetFileName(f);
                if (string.IsNullOrEmpty(name)) continue;
                var lower = name.ToLowerInvariant();
                bool match = false;
                foreach (var sub in targetSubstrings)
                {
                    if (lower.Contains(sub.ToLowerInvariant())) { match = true; break; }
                }
                if (!match) continue;

                try
                {
                    FileUtil.DeleteFileOrDirectory(f);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"対象bundleの削除に失敗しました: {f} - {e.Message}");
                }
            }
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"未参照bundle削除中にエラー: {ex.Message}\n{ex.StackTrace}");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
	}
}


