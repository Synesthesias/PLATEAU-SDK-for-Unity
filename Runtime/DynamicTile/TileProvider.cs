using System;
using System.IO;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// タイル用のカタログに対して適用するCustomProviderです。
    /// 外部ディレクトリに出力したAssetBundleを、出力時と別環境下でも読めるようにするためのスクリプトです。
    /// ビルド時にカタログに埋め込まれたAssetBundleのInternalId(パス)を補正し、別パスでも読めるようにします。
    /// </summary>
    [System.ComponentModel.DisplayName("Custom Tile AssetBundle Provider")]
    public sealed class TileCatalogAssetBundleProvider : AssetBundleProvider
    {
        private string baseDir;
        private string catalogDirName;

        /// <summary>
        /// AddressableLoader側から、今回読むカタログのパスを渡す。
        /// アセットの読む前にセットする必要がある。
        /// </summary>
        public void SetCatalogPath(string catalogPath)
        {
            if (string.IsNullOrEmpty(catalogPath))
            {
                baseDir = null;
                catalogDirName = null;
                return;
            }

            var catalogUri = ToFileUri(catalogPath);
            if (catalogUri == null)
            {
                baseDir = null;
                catalogDirName = null;
                return;
            }

            baseDir = new Uri(catalogUri, ".").AbsoluteUri;

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
        }

        public override void Provide(ProvideHandle provideHandle)
        {
            var location = provideHandle.Location;
            if (location == null)
            {
                base.Provide(provideHandle);
                return;
            }

            var raw = location.InternalId;
            if (string.IsNullOrEmpty(raw))
            {
                base.Provide(provideHandle);
                return;
            }

            // .bundle だけ
            if (!raw.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase))
            {
                base.Provide(provideHandle);
                return;
            }

            // catalog情報が無いなら補正しない
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(catalogDirName))
            {
                base.Provide(provideHandle);
                return;
            }

            // built-in 系は触らない
            var fileNameForBuiltinCheck = Path.GetFileName(raw.Replace('\\', '/'));
            if (!string.IsNullOrEmpty(fileNameForBuiltinCheck) &&
                (fileNameForBuiltinCheck.Contains("unitybuiltinassets", StringComparison.OrdinalIgnoreCase) ||
                 fileNameForBuiltinCheck.Contains("unitydefaultresources", StringComparison.OrdinalIgnoreCase)))
            {
                base.Provide(provideHandle);
                return;
            }

            var normalized = NormalizeToUriLike(raw);

            // すでにbaseDir配下ならそのまま
            if (normalized.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                base.Provide(provideHandle);
                return;
            }

            // カタログのフォルダ名を含むものだけ変換
            var needle = "/" + catalogDirName + "/";
            if (!normalized.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                base.Provide(provideHandle);
                return;
            }

            var markerIndex = normalized.LastIndexOf(needle, StringComparison.OrdinalIgnoreCase);

            var relativePath = normalized.Substring(markerIndex + needle.Length);
            if (string.IsNullOrEmpty(relativePath))
            {
                base.Provide(provideHandle);
                return;
            }

            var fixedInternalId = new Uri(new Uri(baseDir), relativePath).AbsoluteUri;

            // location差し替え
            var deps = (location.Dependencies != null)
                ? location.Dependencies.ToArray()
                : Array.Empty<IResourceLocation>();

            var newLocation = new ResourceLocationBase(
                location.PrimaryKey,
                fixedInternalId,
                location.ProviderId,
                location.ResourceType,
                deps)
            {
                Data = location.Data
            };

            provideHandle = ReplaceLocation(provideHandle, newLocation);
            base.Provide(provideHandle);
        }

        private static Uri ToFileUri(string pathOrUri)
        {
            try
            {
                if (pathOrUri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    return new Uri(pathOrUri);

                var full = Path.GetFullPath(pathOrUri);
                return new Uri(full);
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
            {
                return t;
            }

            // "E:/..." -> file:///E:/...
            if (t.Length >= 3 && char.IsLetter(t[0]) && t[1] == ':' && t[2] == '/')
                return "file:///" + t;

            // Unix/macOS "/..." -> file://...
            if (t.Length >= 1 && t[0] == '/')
            {
                try { return new Uri(t).AbsoluteUri; }
                catch { /* ignore */ }
            }

            return t;
        }

        /// <summary>
        /// リフレクションを利用してLocationを差し替えます。
        /// WARNING: AddressablePackageのバージョン変更等で動かなくなる可能性があることに気を付けてください。
        /// </summary>
        private static ProvideHandle ReplaceLocation(ProvideHandle handle, IResourceLocation newLocation)
        {
            try
            {
                var t = typeof(ProvideHandle);
                var rmHandleField = t.GetField("m_InternalOp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var internalOp = rmHandleField?.GetValue(handle);
                if (internalOp == null)
                {
                    UnityEngine.Debug.LogWarning("m_InternalOpの取得失敗。タイルカタログ内のPath補正が出来ませんでした。");
                    return handle;
                }

                var opType = internalOp.GetType();

                var locationProp = opType.GetProperty("Location", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (locationProp != null && locationProp.CanWrite)
                {
                    locationProp.SetValue(internalOp, newLocation);
                    return handle;
                }

                var locationField = opType.GetField("m_Location", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (locationField != null)
                {
                    locationField.SetValue(internalOp, newLocation);
                    return handle;
                }

                UnityEngine.Debug.LogWarning("Locationの差し替え失敗。タイルカタログ内のPath補正が出来ませんでした。");
            }
            catch(Exception e)
            {
                // 差し替えできない場合は元のまま
                UnityEngine.Debug.LogWarning(e.Message);
                UnityEngine.Debug.LogWarning("Locationの差し替え失敗。タイルカタログ内のPath補正が出来ませんでした。");
            }

            return handle;
        }
    }
}