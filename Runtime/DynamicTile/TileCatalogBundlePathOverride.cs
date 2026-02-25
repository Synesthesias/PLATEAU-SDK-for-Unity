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

                if (string.IsNullOrEmpty(catalogDirName) || string.IsNullOrEmpty(baseDir))
                    return raw;

                // built-in系は触らない
                var fileNameForBuiltinCheck = Path.GetFileName(raw.Replace('\\', '/'));
                if (!string.IsNullOrEmpty(fileNameForBuiltinCheck) &&
                    (fileNameForBuiltinCheck.Contains("unitybuiltinassets", StringComparison.OrdinalIgnoreCase) ||
                     fileNameForBuiltinCheck.Contains("unitydefaultresources", StringComparison.OrdinalIgnoreCase)))
                {
                    return raw;
                }

                var normalized = NormalizeToUriLike(raw);

                // すでにbaseDir配下ならそのまま
                if (normalized.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                    return raw;

                // カタログのフォルダ名を含むものだけ変換
                var needle = "/" + catalogDirName + "/";
                if (!normalized.Contains(needle, StringComparison.OrdinalIgnoreCase))
                {
                    return raw;
                }

                var fileName = GetFileName(normalized);
                if (string.IsNullOrEmpty(fileName)) return raw;

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

            // "E:/..." 形式を file:/// に(Windows環境対応)
            if (t.Length >= 3 && char.IsLetter(t[0]) && t[1] == ':' && t[2] == '/')
                return "file:///" + t;

            // Unix/macOS環境も対応
            if (t.Length >= 1 && t[0] == '/')
            {
                try
                {
                    return new Uri(t).AbsoluteUri;
                }
                catch
                {
                    // なにもしない
                }
            }

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