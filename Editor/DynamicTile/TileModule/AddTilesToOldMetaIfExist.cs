using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// タイル生成にあたって、保存先にすでにメタデータがあるなら、上書きではなく追加するようにします。
    /// </summary>
    public class AddTilesToOldMetaIfExist : IBeforeTileAssetBuild
    {
        private readonly DynamicTileProcessingContext context;

        public AddTilesToOldMetaIfExist(DynamicTileProcessingContext context)
        {
            this.context = context;
        }
        
        /// <summary>
        /// 保存先にすでにメタデータがあるなら、そこに新規のタイル情報を追加します。
        /// これにより上書きの代わりに新規追加になるようにします。
        /// 加えて、前の処理のタイルをAddressable Groupに追加することで新規追加後に新旧を両方読めるようにします。
        /// </summary>
        public bool BeforeTileAssetBuild()
        {
            string shorterGroupName = context.AddressableGroupName.Replace(DynamicTileProcessingContext.AddressableGroupBaseName + "_", "");
            string addressName = $"{DynamicTileExporter.AddressableAddressBase}_{shorterGroupName}";
            string normalizedAssetPath = AssetPathUtil.NormalizeAssetPath(context.AssetConfig.AssetPath);
            string dataPath = Path.Combine(normalizedAssetPath, addressName + ".asset").Replace('\\', '/');

            var existingMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
            if (existingMeta != null && context.MetaStore != null && context.MetaStore.TileMetaInfos != null)
            {
                // Assets内のケースで、既存のメタに新規分を追加します。
                foreach (var info in context.MetaStore.TileMetaInfos)
                {
                    if (info == null) continue;
                    existingMeta.AddMetaInfo(info.AddressName, info.Extent, info.LOD, info.ZoomLevel);
                }
                
                EditorUtility.SetDirty(existingMeta);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(dataPath, ImportAssetOptions.ForceUpdate);
                
                context.MetaStore = existingMeta;

                // 追加前のアセットバンドルをAddressable Groupに登録
                try
                {
                    var addresses = GetDistinctAddressesFromMeta(existingMeta);
                    EnsureAddressesInGroup(addresses, context.AddressableGroupName, null);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"既存メタのアドレス取り込みに失敗しました: {ex.Message}");
                }
            }
            else
            {
                // Assets外のケースで、既存のメタに新規分を追加します。
                try
                {
                    if (!string.IsNullOrEmpty(context.BuildFolderPath) && Directory.Exists(context.BuildFolderPath))
                    {
                        var catalogFiles = TileCatalogSearcher.FindCatalogFiles(context.BuildFolderPath, true);
                        if (catalogFiles.Length > 0)
                        {
                            var latestCatalog = catalogFiles[0];
                            var loader = new AddressableLoader();
                            var oldMeta = loader.InitializeAsync(latestCatalog).GetAwaiter().GetResult();
                            if (oldMeta != null && oldMeta.TileMetaInfos != null && context.MetaStore != null)
                            {
                                foreach (var info in oldMeta.TileMetaInfos)
                                {
                                    if (info == null) continue;
                                    context.MetaStore.AddMetaInfo(info.AddressName, info.Extent, info.LOD, info.ZoomLevel);
                                }

                                // 追加前のアセットバンドルをAddressable Groupに登録
                                var addresses = GetDistinctAddressesFromMeta(oldMeta);
                                EnsureAddressesInGroup(addresses, context.AddressableGroupName, latestCatalog);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"旧メタデータの読み込みに失敗しました: {ex.Message}");
                }
            }

            return true;
        }
        
        /// <summary>
        /// メタストアから重複のないアドレス一覧を抽出します。
        /// </summary>
        private static List<string> GetDistinctAddressesFromMeta(PLATEAUDynamicTileMetaStore meta)
        {
            if (meta == null || meta.TileMetaInfos == null) return new List<string>();
            return meta.TileMetaInfos
                .Where(i => i != null && !string.IsNullOrEmpty(i.AddressName))
                .Select(i => i.AddressName)
                .Distinct()
                .ToList();
        }
        
        /// <summary>
        /// 同じディレクトリに複数回タイル出力したとき、上書きではなく追加するために旧アセットバンドルを認識させる目的で、
        /// 指定アドレスを現在のグループに含めます。
        /// 1) Assets 内に同名Prefabがあれば再登録
        /// 2) 無ければ、catalogPath が指定されている場合に限り Addressables 経由でロード→一時Prefab化→登録
        /// </summary>
        private static void EnsureAddressesInGroup(IEnumerable<string> addresses, string groupName, string catalogPath)
        {
            if (addresses == null) return;

            AsyncOperationHandle<IResourceLocator>? catalogHandle = null;
            bool catalogLoaded = false;

            var tempRoot = Path.Combine("Assets", AddressableLoader.AddressableLocalBuildFolderName, groupName).Replace('\\','/');
            if (!Directory.Exists(tempRoot)) Directory.CreateDirectory(tempRoot);

            foreach (var address in addresses)
            {
                if (string.IsNullOrEmpty(address)) continue;
                try
                {
                    // 1) Assets 内にあるケース
                    var guids = AssetDatabase.FindAssets($"t:Prefab {address}");
                    bool registered = false;
                    if (guids != null && guids.Length > 0)
                    {
                        foreach (var guid in guids)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            if (string.IsNullOrEmpty(path)) continue;
                            var name = Path.GetFileNameWithoutExtension(path);
                            if (!string.Equals(name, address, StringComparison.Ordinal)) continue;
                            AddressablesUtility.RegisterAssetAsAddressable(path, address, groupName, new List<string> { DynamicTileExporter.AddressableLabel });
                            registered = true;
                            break;
                        }
                    }
                    if (registered) continue;

                    // 2) Assets外のケースでは、Addressableに認識させるために一時的にインポートしてプレハブにします。
                    if (!string.IsNullOrEmpty(catalogPath))
                    {
                        if (!catalogLoaded)
                        {
                            var ch = UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync(catalogPath);
                            ch.WaitForCompletion();
                            catalogHandle = ch;
                            catalogLoaded = true;
                        }
                        var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address);
                        handle.WaitForCompletion();
                        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                        {
                            var go = handle.Result;
                            var savePath = Path.Combine(tempRoot, address + ".prefab").Replace('\\','/');
                            var saved = PrefabUtility.SaveAsPrefabAsset(go, savePath);
                            if (saved != null)
                            {
                                AddressablesUtility.RegisterAssetAsAddressable(savePath, address, groupName, new List<string> { DynamicTileExporter.AddressableLabel });
                            }
                        }
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"アドレス取り込みに失敗しました: {address} - {ex.Message}");
                }
            }

            if (catalogLoaded && catalogHandle.HasValue)
            {
                UnityEngine.AddressableAssets.Addressables.Release(catalogHandle.Value);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}