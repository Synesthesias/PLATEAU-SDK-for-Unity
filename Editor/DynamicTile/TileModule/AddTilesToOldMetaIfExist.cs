using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
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
        public async Task<bool> BeforeTileAssetBuildAsync()
        {
            if (IsMetaExistInAssets())
            {
                // Assets内のケース
                AddExistingMetaInAssets();
            }
            else
            {
                // Assets外のケース
               await AddExistingMetaOutsideAssetsAsync();
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
                existingMeta.AddMetaInfo(info.AddressName, info.GroupName, info.Extent, info.LOD, info.ZoomLevel);
            }
                
            EditorUtility.SetDirty(existingMeta);
            AssetDatabase.SaveAssets();
                
            context.MetaStore = existingMeta;

            // 追加前のアセットバンドルをAddressable Groupに登録
            try
            {

                var prefabs = new TilePrefabGetter().GetTilePrefabsFromMeta(existingMeta, context.AddressableGroupName);
                foreach(var prefab in prefabs)
                {
                    AddressablesUtility.RegisterAssetAsAddressable(prefab.Path, prefab.Address, prefab.GroupName,
                        new List<string> { DynamicTileExporter.AddressableLabel });
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
        private async Task AddExistingMetaOutsideAssetsAsync()
        {
            // Assets外のケースでは、前回出力フォルダに置いた unitypackage を取り込み、
            // 旧メタをマージしつつ旧プレハブを Addressables に登録します（上書きではなく追加）。
            if (string.IsNullOrEmpty(context.BuildFolderPath) || !Directory.Exists(context.BuildFolderPath))
            {
                return;
            }

            // unitypackage の候補を決定
            string expected = context.UnityPackagePath;
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

            // unitypackageをインポートしてプレハブ群を取り込みます。
            // 非同期的にインポートする必要があります。もし同期的にした場合、Unityエディタに処理が戻らず、インポートしたいファイルが一部欠落したまま次の処理に移り、デバッグが難しい問題を引き起こします。
            var ok = await EditorAsync.ImportPackageAsync(packagePath, false); 
            if (!ok)
            {
                Debug.LogError("import failed.");
                return;
            }
            
            AssetDatabase.Refresh();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 旧メタを取得（パッケージに含まれている想定）
            var rootAssetPath = AssetPathUtil.NormalizeAssetPath(context.AssetConfig.AssetPath);
            // ルート直下から探索
            var guids = AssetDatabase.FindAssets("t:PLATEAUDynamicTileMetaStore", new[] { rootAssetPath });
            PLATEAUDynamicTileMetaStore oldMeta;
            if (guids != null && guids.Length > 0)
            {
                var metaPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                oldMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(metaPath);
            }
            else
            {
                Debug.LogError("no meta found.");
                return;
            }

            if (oldMeta != null && oldMeta.TileMetaInfos != null && context.MetaStore != null)
            {
                // 旧メタ情報を新メタにマージ（yield を含まないので try/catch 可）
                foreach (var info in oldMeta.TileMetaInfos)
                {
                    if (info == null) continue;
                    try
                    {
                        context.MetaStore.AddMetaInfo(info.AddressName, info.GroupName, info.Extent, info.LOD,
                            info.ZoomLevel);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"旧メタのマージ中に失敗: {ex.Message}");
                    }
                }


                // 旧プレハブを Addressables に再登録
                var addresses = new TilePrefabGetter().GetDistinctAddressesFromMeta(oldMeta);
                if (addresses == null) return;
                
                var groupName = context.AddressableGroupName;

                var tilePrefabGetter = new TilePrefabGetter();
                foreach (var address in addresses)
                {
                    if (string.IsNullOrEmpty(address)) continue;

                    try
                    {
                        var prefab = tilePrefabGetter.GetPrefabFromAddress(address, groupName);
                        if (prefab != null)
                        {
                            AddressablesUtility.RegisterAssetAsAddressable(prefab.Path, prefab.Address, groupName,
                                new List<string> { DynamicTileExporter.AddressableLabel });
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

        public bool IsMetaExistInAssets()
        {
            var existingMeta = GetExistingMetaInAssets();
            return existingMeta != null && context.MetaStore != null && context.MetaStore.TileMetaInfos != null;
        }

        private PLATEAUDynamicTileMetaStore GetExistingMetaInAssets()
        {
            return AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(context.DataPath);
        }
        
        
        
    }
}