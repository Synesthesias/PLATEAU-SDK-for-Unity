using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
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
            // Assets外のケースでは、前回出力フォルダに置いた unitypackage を取り込み、
            // 旧メタをマージしつつ旧プレハブを Addressables に登録します（上書きではなく追加）。
            try
            {
                if (string.IsNullOrEmpty(context.BuildFolderPath) || !Directory.Exists(context.BuildFolderPath))
                {
                    return;
                }

                // unitypackage の候補を決定
                string expected = Path.Combine(context.BuildFolderPath, $"{context.AddressableGroupName}_Prefabs.unitypackage");
                string packagePath = File.Exists(expected)
                    ? expected
                    : Directory.GetFiles(context.BuildFolderPath, "*_Prefabs.unitypackage", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(File.GetLastWriteTime)
                        .FirstOrDefault();

                // パッケージが見つからなければ何もしない（旧タイルなし）
                if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
                {
                    return;
                }

                // 旧プレハブ群を取り込む。
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 旧メタを取得（パッケージに含まれている想定）
                var oldMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(DynamicTileProcessingContext.PrefabsTempSavePath);
                var rootAssetPath = AssetPathUtil.NormalizeAssetPath(context.AssetConfig.AssetPath);
                if (oldMeta == null)
                {
                    // 念のためルート直下から探索
                    var guids = AssetDatabase.FindAssets("t:PLATEAUDynamicTileMetaStore", new[] { DynamicTileProcessingContext.PrefabsTempSavePath });
                    if (guids != null && guids.Length > 0)
                    {
                        var metaPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        oldMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(metaPath);
                    }
                }

                if (oldMeta != null && oldMeta.TileMetaInfos != null && context.MetaStore != null)
                {
                    // 旧メタ情報を新メタにマージ
                    foreach (var info in oldMeta.TileMetaInfos)
                    {
                        if (info == null) continue;
                        context.MetaStore.AddMetaInfo(info.AddressName, info.Extent, info.LOD, info.ZoomLevel);
                    }

                    // 旧プレハブを Addressables に再登録
                    var addresses = GetDistinctAddressesFromMeta(oldMeta);
                    var groupName = context.AddressableGroupName;

                    foreach (var address in addresses)
                    {
                        if (string.IsNullOrEmpty(address)) continue;

                        try
                        {
                            var prefabGuids = AssetDatabase.FindAssets($"t:Prefab {address}", new[] { rootAssetPath });
                            if (prefabGuids != null && prefabGuids.Length > 0)
                            {
                                foreach (var guid in prefabGuids)
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

                    EditorUtility.SetDirty(context.MetaStore);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"旧メタデータの取り込みに失敗しました: {ex.Message}");
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