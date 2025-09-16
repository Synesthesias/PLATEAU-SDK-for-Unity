using System;
using System.IO;
using PLATEAU.DynamicTile;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// タイルに相当するアセットをunitypackage形式でプレハブ出力します。
    /// プロジェクト外へタイル出力したあと、タイルを編集したい場合はunitypackageを取り込んでからタイルを再出力することになります。
    /// </summary>
	internal class ExportUnityPackageOfPrefabs : IAfterTileAssetBuild
	{
		private readonly DynamicTileProcessingContext context;

		public ExportUnityPackageOfPrefabs(DynamicTileProcessingContext context)
		{
			this.context = context;
		}

		public bool AfterTileAssetBuild()
		{
            if (!context.IsExcludeAssetFolder)
            {
                return true;
            }
            
			try
			{
				// 生成プレハブのルート（Assets 相対）
				string rootAssetPath = AssetPathUtil.NormalizeAssetPath(context.AssetConfig.AssetPath);
				if (!AssetDatabase.IsValidFolder(rootAssetPath))
				{
					Debug.LogWarning($"UnityPackage 出力対象フォルダが見つかりません: {rootAssetPath}");
                    return false;
                }

				// まずはプロジェクト配下に出力
				string packageFileName = $"{context.AddressableGroupName}_Prefabs.unitypackage";
				string tempPackageProjectRelativePath = packageFileName; // プロジェクトルート直下
				AssetDatabase.ExportPackage(
					new[] { rootAssetPath },
					tempPackageProjectRelativePath,
					ExportPackageOptions.Recurse // フラグにExportPackageOptions.IncludeDependenciesを付けると、ソースコード自体が古い物で上書きされることがあるので注意
				);

				string tempPackageFullPath = Path.GetFullPath(tempPackageProjectRelativePath);

                // プロジェクト外に出力
                if (!Directory.Exists(context.BuildFolderPath))
                {
                    Directory.CreateDirectory(context.BuildFolderPath);
                }
                string destPath = Path.Combine(context.BuildFolderPath, packageFileName);
                File.Copy(tempPackageFullPath, destPath, true);
                File.Delete(tempPackageFullPath);
                AssetDatabase.Refresh();

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


