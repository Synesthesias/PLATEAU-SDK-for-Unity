using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

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
            if (IsMetaExistInAssets())
            {
                // Assets内のケース
                AddExistingMetaInAssets();
            }
            else
            {
                // Assets外のケース
                AddExistingMetaOutsideAssets();
            }
            
            Save();
            return true;
        }

        private void Save()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 追加処理で、メタがAssets内にあるケースです。
        /// </summary>
        private void AddExistingMetaInAssets()
        {
            var existingMeta = GetExistingMetaInAssets();
            foreach (var info in context.MetaStore.TileMetaInfos)
            {
                if (info == null) continue;
                existingMeta.AddMetaInfo(info.AddressName, info.Extent, info.LOD, info.ZoomLevel);
            }
                
            EditorUtility.SetDirty(existingMeta);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(context.DataPath, ImportAssetOptions.ForceUpdate);
                
            context.MetaStore = existingMeta;

            // 追加前のアセットバンドルをAddressable Groupに登録
            try
            {
                var addresses = GetDistinctAddressesFromMeta(existingMeta);

                if (addresses == null) return;

                var groupName = context.AddressableGroupName;
                var tempRoot = Path.Combine("Assets", AddressableLoader.AddressableLocalBuildFolderName, groupName)
                    .Replace('\\', '/');
                if (!Directory.Exists(tempRoot)) Directory.CreateDirectory(tempRoot);

                foreach (var address in addresses)
                {
                    if (string.IsNullOrEmpty(address)) continue;
                    try
                    {
                        var guids = AssetDatabase.FindAssets($"t:Prefab {address}");
                        if (guids != null && guids.Length > 0)
                        {
                            foreach (var guid in guids)
                            {
                                var path = AssetDatabase.GUIDToAssetPath(guid);
                                if (string.IsNullOrEmpty(path)) continue;
                                var name = Path.GetFileNameWithoutExtension(path);
                                if (!string.Equals(name, address, StringComparison.Ordinal)) continue;
                                AddressablesUtility.RegisterAssetAsAddressable(path, address, groupName,
                                    new List<string> { DynamicTileExporter.AddressableLabel });
                                break;
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"アドレス取り込みに失敗しました: {address} - {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"既存メタのアドレス取り込みに失敗しました: {ex}");
            }
        }

        /// <summary>
        /// 追加処理で、メタがAssets外にあるケースです。
        /// </summary>
        private void AddExistingMetaOutsideAssets()
        {
            // Assets外のケースで、既存のメタに新規分を追加します。
            try
            {
                if (string.IsNullOrEmpty(context.BuildFolderPath) || !Directory.Exists(context.BuildFolderPath))
                {
                    return;
                }

                var catalogFiles = TileCatalogSearcher.FindCatalogFiles(context.BuildFolderPath, true);
                if (catalogFiles.Length <= 0)
                {
                    return;
                }

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

                    if (addresses == null) return;

                    var groupName = context.AddressableGroupName;

                    var tempRoot = Path.Combine("Assets", AddressableLoader.AddressableLocalBuildFolderName, groupName)
                        .Replace('\\', '/');
                    if (!Directory.Exists(tempRoot)) Directory.CreateDirectory(tempRoot);

                    // 古いメタに存在する各アドレスを登録
                    foreach (var address in addresses)
                    {
                        if (string.IsNullOrEmpty(address)) continue;
                        try
                        {
                            if (!string.IsNullOrEmpty(latestCatalog))
                            {
                                // if (!catalogLoaded)
                                // {
                                //     var ch = Addressables.LoadContentCatalogAsync(latestCatalog);
                                //     ch.WaitForCompletion();
                                //     catalogHandle = ch;
                                //     catalogLoaded = true;
                                // }

                                // 古いアドレスをプレハブにしてAddressableに登録し直す
                                // var handle = Addressables.LoadAssetAsync<GameObject>(address);
                                // handle.WaitForCompletion();
                                // if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                                // {
                                //     var go = handle.Result;
                                //     var savePath = Path.Combine(tempRoot, address + ".prefab").Replace('\\', '/');
                                //     var saved = PrefabUtility.SaveAsPrefabAsset(go, savePath);
                                //     if (saved != null)
                                //     {
                                //         AddressablesUtility.RegisterAssetAsAddressable(savePath, address, groupName,
                                //             new List<string> { DynamicTileExporter.AddressableLabel });
                                //     }
                                // }
                                //
                                // Addressables.Release(handle);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"アドレス取り込みに失敗しました: {address} - {ex.Message}");
                        }
                    }

                    // if (catalogLoaded)
                    // {
                    //     Addressables.Release(catalogHandle.Value);
                    // }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"旧メタデータの読み込みに失敗しました: {ex.Message}");
            }
        }

        public bool IsMetaExistInAssets()
        {
            var existingMeta = GetExistingMetaInAssets();
            return existingMeta != null && context.MetaStore != null && context.MetaStore.TileMetaInfos != null;
        }

        private PLATEAUDynamicTileMetaStore GetExistingMetaInAssets()
        {
            return AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(context.DataPath);
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
        
    }
}