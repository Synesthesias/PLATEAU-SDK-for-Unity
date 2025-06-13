using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Addressableを使用してカタログとMeta情報をロードするクラス。
    /// </summary>
    public class AddressableLoader
    {
        // DynamicTileのラベル名
        private const string DynamicTileLabelName = "DynamicTile";

        /// <summary>
        /// 初期化処理
        /// 非同期で処理をすると、プレイ終了後に処理がとまるため、同期で処理を行う。
        /// </summary>
        /// <param name="catalogPath"></param>
        /// <returns></returns>
        public PLATEAUDynamicTileMetaStore Initialize(string catalogPath)
        {
            var init = Addressables.InitializeAsync();
            // NOTE: プレイ終了直後だと非同期で取得できないため、同期で取得
            init.WaitForCompletion();
 
            // カタログを取得
            if (!LoadCatalog(catalogPath, DynamicTileLabelName, out var metaStorePath))
            {
                Debug.LogError("カタログのロードに失敗しました。アドレスが見つかりません。");
                return null;
            }

            // meta情報をロード
            var metaStore = LoadMetaStore(metaStorePath);
            if (metaStore == null)
            {
                return null;
            }

            return metaStore;
        }
        
        /// <summary>
        /// file://プロトコルを除去します
        /// </summary>
        private string RemoveFileProtocol(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return path.StartsWith("file://") ? path.Substring("file://".Length) : path;
        }

        /// <summary>
        /// カタログファイルをロードします
        /// </summary>
        /// <param name="catalogPath">カタログファイルのパス</param>
        /// <param name="label">ラベル</param>
        /// <param name="metaStorePath"></param>
        /// <returns>ロードされたGameObjectのリスト</returns>
        private bool LoadCatalog(string catalogPath, string label, out string metaStorePath)
        {
            metaStorePath = string.Empty;
            try
            {
                var bundlePath = string.Empty;
                if (string.IsNullOrEmpty(catalogPath))
                {
                    // 空の場合はローカルのカタログパスを取得
                    catalogPath = GetLocalCatalogPath();
                    if (!string.IsNullOrEmpty(catalogPath))
                    {
                        bundlePath = Path.GetDirectoryName(RemoveFileProtocol(catalogPath));
                    }
                }
                else
                {
                    // パスを正規化（バックスラッシュをスラッシュに変換）
                    catalogPath = catalogPath.Replace('\\', '/');
                    
                    // bundlePathを保持
                    bundlePath = Path.GetDirectoryName(catalogPath);

                    // file://プロトコルを追加
                    if (!catalogPath.StartsWith("file://"))
                    {
                        catalogPath = "file://" + catalogPath;
                    }
                }
                
                // カタログファイルをロード
                var catalogHandle = Addressables.LoadContentCatalogAsync(catalogPath);
                
                // NOTE: プレイ終了直後だと非同期で取得できないため、同期で取得
                catalogHandle.WaitForCompletion();

                if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"カタログファイルのロードに失敗しました: {catalogPath}");
                    Addressables.Release(catalogHandle);
                    return false;
                }

                // ScriptableObjectのアドレスを取得
                if (catalogHandle.Result.Locate(label, typeof(ScriptableObject), out var scriptableObjectLocations))
                {
                    foreach (var location in scriptableObjectLocations)
                    {
                        if (location.Dependencies.Count <= 0)
                        {
                            continue;
                        }

                        if (bundlePath != null)
                        {
                            // メタ情報のパスを保持
                            metaStorePath = Path.Combine(bundlePath, location.Dependencies[0].PrimaryKey);
                            return true;
                        }
                    }
                }
                // カタログ用のhandleの解放
                Addressables.Release(catalogHandle);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"カタログのロード中にエラーが発生しました: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// カタログのローカルパスを取得します。
        /// </summary>
        /// <returns></returns>
        private string GetLocalCatalogPath()
        {
            foreach (var resourceLocator in Addressables.ResourceLocators)
            {
                foreach (var key in resourceLocator.Keys)
                {
                    if (!resourceLocator.Locate(key, typeof(object), out var locations))
                    {
                        continue;
                    }
                    foreach (var loc in locations)
                    {
                        string internalId = RemoveFileProtocol(loc.InternalId);

                        if (!internalId.EndsWith(".bundle"))
                        {
                            continue;
                        }

                        string dir = Path.GetDirectoryName(internalId);
                        if (string.IsNullOrEmpty(dir))
                        {
                            Debug.LogError("カタログファイルのディレクトリが取得できません");
                            return "";
                        }
                        // カタログファイルのパスを取得
                        var catalogFiles = Directory.GetFiles(dir, "catalog_*.json");
                        if (catalogFiles.Length == 0)
                        {
                            Debug.LogError("カタログファイルが見つかりません");
                            return "";
                        }
                        return catalogFiles[0];
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// meta情報をロードします。
        /// </summary>
        /// <returns></returns>
        private PLATEAUDynamicTileMetaStore LoadMetaStore(string metaStorePath)
        {
            PLATEAUDynamicTileMetaStore metaStore = null;
            try
            {
                // ファイルの存在確認
                if (!File.Exists(metaStorePath))
                {
                    Debug.LogError($"ファイルが存在しません: {metaStorePath}");
                    return null;
                }

                // Addressable経由だとロードできないので、直接ファイルシステムから読み込み
                var bundle = AssetBundle.LoadFromFile(metaStorePath);
                if (bundle != null)
                {
                    // バンドル内のアセット名をログ出力
                    string[] assetNames = bundle.GetAllAssetNames();
                    if (assetNames.Length == 0)
                    {
                        Debug.LogWarning($"バンドル内にアセットが見つかりません: {metaStorePath}");
                        return null;
                    }
                    metaStore = bundle.LoadAsset<PLATEAUDynamicTileMetaStore>(assetNames[0]);
                    bundle.Unload(false);
                }
                else
                {
                    Debug.LogError($"MetaStoreのロードに失敗しました: {metaStorePath}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"MetaStoreのロード中にエラーが発生しました: {ex.Message}\n{ex.StackTrace}");
            }
            return metaStore;
        }
    }
} 