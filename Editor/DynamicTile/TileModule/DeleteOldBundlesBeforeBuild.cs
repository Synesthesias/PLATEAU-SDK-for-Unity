using PLATEAU.DynamicTile;
using PLATEAU.Util;
using System.IO;
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

		public async Task<bool> BeforeTileAssetBuildAsync()
		{
			try
			{
				// 部分リビルドの場合（対象アドレスが指定されている場合）は旧bundleを削除しない
				if (context.TargetAddresses != null && context.TargetAddresses.Count > 0)
				{
					await Task.CompletedTask;
					return true;
				}

				var buildFolder = context.BuildFolderPath;
				if (string.IsNullOrEmpty(buildFolder)) return true;

				// BuildFolderPath が Assets 配下ならフルパスへ解決
				string fullPath;
				if (PathUtil.IsSubDirectoryOfAssets(buildFolder))
				{
					fullPath = AssetPathUtil.GetFullPath(buildFolder);
				}
				else
				{
					fullPath = buildFolder;
				}

				if (!Directory.Exists(fullPath)) return true;

				int deleted = 0;
				foreach (var pattern in new[] { "*.bundle", "*.bundle.manifest" })
				{
					var files = Directory.GetFiles(fullPath, pattern, SearchOption.TopDirectoryOnly);
					foreach (var f in files)
					{
						try
						{
							FileUtil.DeleteFileOrDirectory(f);
							deleted++;
						}
						catch (System.Exception e)
						{
							Debug.LogWarning($"旧bundleの削除に失敗しました: {f} - {e.Message}");
						}
					}
				}

				if (deleted > 0)
				{
					Debug.Log($"旧bundleを削除しました: {deleted} 件 ({fullPath})");
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"旧bundle削除中にエラー: {ex.Message}\n{ex.StackTrace}");
				return false;
			}

			await Task.CompletedTask;
			return true;
		}
	}
}


