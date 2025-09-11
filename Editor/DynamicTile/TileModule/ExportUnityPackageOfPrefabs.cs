using System;
using System.IO;
using PLATEAU.DynamicTile;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
	internal class ExportUnityPackageOfPrefabs : IAfterTileAssetBuild
	{
		private readonly DynamicTileProcessingContext context;

		public ExportUnityPackageOfPrefabs(DynamicTileProcessingContext context)
		{
			this.context = context;
		}

		public bool AfterTileAssetBuild()
		{
			try
			{
				// 生成プレハブのルート（Assets 相対）
				string rootAssetPath = AssetPathUtil.NormalizeAssetPath(context.AssetConfig.AssetPath);
				if (!AssetDatabase.IsValidFolder(rootAssetPath))
				{
					Debug.LogWarning($"UnityPackage 出力対象フォルダが見つかりません: {rootAssetPath}");
					return true; // 失敗としては扱わない（対象がなければ何もしない）
				}

				// まずはプロジェクト配下に安全に出力
				string packageFileName = $"{context.AddressName}_Prefabs.unitypackage";
				string tempPackageProjectRelativePath = packageFileName; // プロジェクトルート直下
				AssetDatabase.ExportPackage(
					new[] { rootAssetPath },
					tempPackageProjectRelativePath,
					ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
				);

				string tempPackageFullPath = Path.GetFullPath(tempPackageProjectRelativePath);

				// プロジェクト外に出力指定されている場合は、外部へ移動
				if (!string.IsNullOrEmpty(context.BuildFolderPath) && Path.IsPathRooted(context.BuildFolderPath))
				{
					if (!Directory.Exists(context.BuildFolderPath))
					{
						Directory.CreateDirectory(context.BuildFolderPath);
					}
					string destPath = Path.Combine(context.BuildFolderPath, packageFileName);
					File.Copy(tempPackageFullPath, destPath, true);
					File.Delete(tempPackageFullPath);
					AssetDatabase.Refresh();
					Debug.Log($"UnityPackage を外部に出力しました: {destPath}");
				}
				else
				{
					Debug.Log($"UnityPackage を出力しました: {tempPackageFullPath}");
				}

				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"UnityPackage 出力中にエラー: {e}");
				return false;
			}
		}
	}
}


