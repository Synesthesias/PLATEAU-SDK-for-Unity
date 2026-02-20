using System;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 外部ディレクトリに置かれたタイル用のカタログを読む際に呼び出します。
    /// ビルド時にカタログに埋め込まれたAssetBundleのInternalId(パス)を、別パスでも読めるようにします
    /// </summary>
    public static class TileCatalogBundlePathOverride
    {
        public static void Apply(string catalogPath)
        {
            if (string.IsNullOrEmpty(catalogPath)) return;

            var catalogUri = ToFileUri(catalogPath);
            if (catalogUri == null) return;

            var baseDir = new Uri(catalogUri, ".").AbsoluteUri;

            string catalogDirName = null;
            try
            {
                var localPath = catalogUri.LocalPath.Replace('\\', '/');
                var dir = Path.GetDirectoryName(localPath);
                catalogDirName = string.IsNullOrEmpty(dir) ? null : new DirectoryInfo(dir).Name;
            }
            catch
            {
                catalogDirName = null;
            }

            Addressables.InternalIdTransformFunc = (IResourceLocation loc) =>
            {
                var raw = loc.InternalId;
                if (string.IsNullOrEmpty(raw)) return raw;

                // bundleだけ対象
                if (!raw.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase))
                    return raw;

                var normalized = NormalizeToUriLike(raw);

                // すでにbaseDir配下ならそのまま
                if (!string.IsNullOrEmpty(baseDir) &&
                    normalized.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                {
                    return normalized;
                }

                // カタログのフォルダ名が含まれるものだけ変換（可能な場合）
                if (!string.IsNullOrEmpty(catalogDirName))
                {
                    var needle = "/" + catalogDirName + "/";
                    if (!normalized.Contains(needle, StringComparison.OrdinalIgnoreCase))
                        return normalized;
                }

                // ファイル名だけにしてbaseDirへ
                var fileName = GetFileName(normalized);
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(baseDir))
                    return normalized;

                // Built-in系は触らない（重複ロードの原因になりやすい）
                if (fileName.Contains("unitybuiltinassets", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Contains("unitydefaultresources", StringComparison.OrdinalIgnoreCase))
                {
                    return normalized; // そのまま
                }

                return baseDir + fileName;
            };
        }

        private static Uri ToFileUri(string pathOrUri)
        {
            try
            {
                if (pathOrUri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    return new Uri(pathOrUri);

                var full = Path.GetFullPath(pathOrUri);
                return new Uri(full); // file:///...
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeToUriLike(string s)
        {
            var t = s.Replace('\\', '/');

            if (t.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                return t;

            // "E:/..." 形式を file:/// に
            if (t.Length >= 3 && char.IsLetter(t[0]) && t[1] == ':' && t[2] == '/')
                return "file:///" + t;

            return t;
        }

        private static string GetFileName(string uriLike)
        {
            var t = uriLike.Replace('\\', '/');
            var last = t.LastIndexOf('/');
            return (last >= 0 && last + 1 < t.Length) ? t.Substring(last + 1) : Path.GetFileName(t);
        }
    }
}