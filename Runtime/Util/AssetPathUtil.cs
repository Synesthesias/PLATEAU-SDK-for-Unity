using System.IO;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class AssetPathUtil
    {

        public static readonly string ASSET_PATH = "Assets";

        /// <summary>
        /// 「Assets/」以下のパスからフルパスを取得
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string GetFullPath(string assetPath)
        {
            var trimmed = assetPath.StartsWith(ASSET_PATH) ? assetPath.Substring(ASSET_PATH.Length + 1) : assetPath;
            var fullPath = Path.Combine(Application.dataPath, trimmed);
            return fullPath;
        }

        /// <summary>
        /// 絶対パスからUnityEditorでのアセットパスを取得
        /// </summary>
        /// <param name="fullPath">絶対パス</param>
        /// <returns></returns>
        public static string GetAssetPath(string fullPath)
        {
            var relativePath = Path.GetRelativePath(Application.dataPath, fullPath);
            var assetPath = GetAssetPathFromRelativePath(relativePath);
            return assetPath;
        }

        /// <summary>
        /// 相対パスからUnityEditorでのアセットパスを取得
        /// </summary>
        /// <param name="relativePath">相対パス</param>
        /// <returns></returns>
        public static string GetAssetPathFromRelativePath(string relativePath)
        {
            if (relativePath == ".") return ASSET_PATH;

            if(relativePath.StartsWith(ASSET_PATH))
                return relativePath;

            var assetPath = Path.Combine(ASSET_PATH, relativePath);
            return assetPath;
        }

        /// <summary>
        /// 指定したパスのディレクトリが存在しない場合、作成する
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static bool CreateDirectoryIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 指定したパスのディレクトリを作成する
        /// ディレクトリが存在する場合、連番を付ける
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns>連番ファイル名のパス</returns>
        public static string CreateDirectoryWithIncrementalNameIfExist(string directoryPath)
        {
            string newPath = directoryPath;
            if (Directory.Exists(directoryPath))
                newPath = CreateIncrementalPathName(directoryPath);
            Directory.CreateDirectory(newPath);
            return newPath;
        }

        /// <summary>
        /// 同名のディレクトリが存在する場合は、_1, _2, ... のように連番を付けて保存
        /// </summary>
        /// <param name="baseFullPath">フルパス</param>
        /// <returns>連番ファイル名のパス</returns>
        public static string CreateIncrementalPathName(string baseFullPath)
        {
            var incrementalPathName = baseFullPath;
            int count = 1;
            while (Directory.Exists(incrementalPathName))
            {
                incrementalPathName = $"{baseFullPath}_{count}";
                count++;
            }
            return incrementalPathName;
        }

        public static string NormalizeAssetPath(string assetPath)
        {
            string normalizedAssetPath = assetPath;
            if (Path.IsPathRooted(assetPath))
            {
                // 絶対パスの場合は相対パスに変換
                normalizedAssetPath = AssetPathUtil.GetAssetPath(assetPath);
            }
            else if (!assetPath.StartsWith("Assets"))
            {
                // Assetsで始まらない相対パスの場合は、Assetsからの相対パスに変換
                normalizedAssetPath = AssetPathUtil.GetAssetPathFromRelativePath(assetPath);
            }

            return normalizedAssetPath;
        }

        /// <summary>
        /// 指定されたパスがAssetsフォルダ直下（ルート）かどうかを判定します
        /// </summary>
        public static bool IsAssetsFolderRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string fullPath = Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');
            string dataPath = Path.GetFullPath(Application.dataPath).Replace('\\', '/').TrimEnd('/');
            return string.Equals(fullPath, dataPath, System.StringComparison.OrdinalIgnoreCase);
        }

    }
}
