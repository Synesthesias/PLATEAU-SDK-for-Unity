using PLATEAU.DynamicTile;
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace PLATEAU.Editor.TileAddressables
{
    /// <summary>
    /// Addressablesのグループ作成やアセットのAddressable登録を行うEditor専用ユーティリティクラス
    /// </summary>
    public static class AddressablesUtility
    {
        private const string TileBuildProfileName = "PLATEAU_TileBuildProfile";
        private const string ProfileVariableNameBuild = "PLATEAU_BuildPath";
        private const string ProfileVariableNameLoad = "PLATEAU_LoadPath";
        private const string SafeDefaultPathLoad  = "[UnityEngine.AddressableAssets.Addressables.RuntimePath]/[BuildTarget]";
        private const string SafeDefaultPathBuild = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";
        
        /// <summary>
        /// AddressableAssetSettingsを取得。nullの場合は警告を出す。
        /// </summary>
        private static AddressableAssetSettings RequireAddressableSettings()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
            }
            return settings;
        }

        /// <summary>
        /// 指定した名前のグループを取得。なければ新規作成。
        /// </summary>
        /// <param name="groupName">グループ名</param>
        public static AddressableAssetGroup GetOrCreateGroup(string groupName)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                return null;
            }

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(
                    groupName,
                    false, // setAsDefaultGroup
                    false, // readOnly
                    true,  // postEvent
                    null   // schemasToCopy
                );
            }

            // BundledAssetGroupSchemaがなければ追加
            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema == null)
            {
                group.AddSchema<BundledAssetGroupSchema>();
            }

            // FIXME: 動的タイルロード時のパフォーマンス向上のため、将来的に非圧縮を検討
            //bundledSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.Uncompressed;
            //EditorUtility.SetDirty(bundledSchema);
            //AssetDatabase.SaveAssets();

            return group;
        }
        
        /// <summary>
        /// 指定したアセットを指定グループにAddressableとして登録
        /// <param name="assetPath">登録するアセットのパス</param>
        /// <param name="address">設定するアドレス</param>
        /// <param name="groupName">所属させるグループ名</param>
        /// <param name="labels">アセットに設定するラベルのリスト</param>
        /// </summary>
        public static void RegisterAssetAsAddressable(
            string assetPath,
            string address,
            string groupName,
            List<string> labels)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettingsが見つかりません。");
                return;
            }

            var group = GetOrCreateGroup(groupName);
            if (group == null)
            {
                Debug.LogError($"グループの作成に失敗しました: {groupName}");
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"アセットが見つかりません: {assetPath}");
                return;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
            }
            entry.SetAddress(address);

            if (labels != null)
            {
                foreach (var label in labels)
                {
                    if (string.IsNullOrEmpty(label)) continue;
                    
                    if (!settings.GetLabels().Contains(label))
                    {
                        settings.AddLabel(label, true);
                    }

                    entry.SetLabel(label, true);
                }
            }
        }
        
        public static void BackToDefaultProfile()
        {
            var addrSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addrSettings == null) return;

            var profileSettings = addrSettings.profileSettings;
            var defaultProfileId = profileSettings.GetProfileId("Default");
            if (string.IsNullOrEmpty(defaultProfileId))
            {
                // フォールバック: アクティブなプロファイルを使用、無ければ作成
                defaultProfileId = addrSettings.activeProfileId;
                if (string.IsNullOrEmpty(defaultProfileId))
                {
                    defaultProfileId = profileSettings.AddProfile("Default", addrSettings.activeProfileId);
                    if (string.IsNullOrEmpty(defaultProfileId))
                    {
                        Debug.LogWarning("Addressables のプロファイルが見つからず、フォールバック作成にも失敗しました。");
                        return;
                    }
                }
            }

            // Default に戻す
            addrSettings.activeProfileId = defaultProfileId;

            // ProfileVariableNameが空だと不安定になるのでデフォルトの安全な値を入れます
            if (!profileSettings.GetVariableNames().Contains(ProfileVariableNameLoad))
            {
                profileSettings.CreateValue(ProfileVariableNameLoad, SafeDefaultPathLoad);
            }

            if (!profileSettings.GetVariableNames().Contains(ProfileVariableNameBuild))
            {
                profileSettings.CreateValue(ProfileVariableNameBuild, SafeDefaultPathBuild);
            }

            var currentLoad = profileSettings.GetValueByName(defaultProfileId, ProfileVariableNameLoad);
            if (string.IsNullOrEmpty(currentLoad))
            {
                profileSettings.SetValue(defaultProfileId, ProfileVariableNameLoad, SafeDefaultPathLoad);
            }

            var currentBuild = profileSettings.GetValueByName(defaultProfileId, ProfileVariableNameBuild);
            if (string.IsNullOrEmpty(currentBuild))
            {
                profileSettings.SetValue(defaultProfileId, ProfileVariableNameBuild, SafeDefaultPathBuild);
            }

            // RemoteCatalog の参照先を揃える
            addrSettings.RemoteCatalogBuildPath.SetVariableByName(addrSettings, ProfileVariableNameBuild);
            addrSettings.RemoteCatalogLoadPath.SetVariableByName(addrSettings, ProfileVariableNameLoad);
            EditorUtility.SetDirty(addrSettings);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 指定したグループのロードパス(LoadPath)とビルドパス(buildPath)を変更します。
        /// </summary>
        /// <param name="groupName">グループ名</param>
        public static void SetGroupLoadAndBuildPath(string groupName)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                return;
            }

            var group = GetOrCreateGroup(groupName);
            if (group == null)
            {
                Debug.LogError($"グループの作成に失敗しました: {groupName}");
                return;
            }

            var bundledSchema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
            if (bundledSchema == null)
            {
                Debug.LogWarning("BundledAssetGroupSchemaが見つかりません。");
                return;
            }
            
            if (!settings.profileSettings.GetVariableNames().Contains(ProfileVariableNameLoad))
            {
                settings.profileSettings.CreateValue(ProfileVariableNameLoad, SafeDefaultPathLoad); // デフォルト値はあとで必要なパスに書き換えること
            }

            if (!settings.profileSettings.GetVariableNames().Contains(ProfileVariableNameBuild))
            {
                settings.profileSettings.CreateValue(ProfileVariableNameBuild, SafeDefaultPathBuild); // デフォルト値はあとで必要なパスに書き換えること
            }

            // グループのBuildPathとLoadPathに変数名をセット
            bundledSchema.BuildPath.SetVariableByName(settings, ProfileVariableNameBuild);
            bundledSchema.LoadPath.SetVariableByName(settings, ProfileVariableNameLoad);

            SetGroupSchema(bundledSchema);
        }
        
        /// <summary>
        /// プロファイルを設定します。
        /// </summary>
        public static void SetRemoteProfileSettings(string path, string pathVar)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("pathパラメータが無効です。");
                return;
            }
            
            if (string.IsNullOrEmpty(pathVar))
            {
                Debug.LogError("pathVarパラメータが無効です。");
                return;
            }
            
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                return;
            }
            
            // カタログ設定
            settings.BuildRemoteCatalog = true;

            var profileSettings = settings.profileSettings;
            if (!profileSettings.GetVariableNames().Contains(ProfileVariableNameLoad))
            {
                profileSettings.CreateValue(ProfileVariableNameLoad, path);
            }

            if (!profileSettings.GetVariableNames().Contains(ProfileVariableNameBuild))
            {
                profileSettings.CreateValue(ProfileVariableNameBuild, path);
            }

            profileSettings.SetValue(settings.activeProfileId, ProfileVariableNameLoad, path);
            profileSettings.SetValue(settings.activeProfileId, ProfileVariableNameBuild, path);
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, ProfileVariableNameLoad);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, ProfileVariableNameBuild);
            
            // addressables_content_state.binもビルド先に保存します。これは差分ビルドで利用します。
            settings.ContentStateBuildPath = BuildPath(settings);
            
            SaveAddressableSettings();
        }

        public static void SaveAddressableSettings()
        {
            var settings = RequireAddressableSettings();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static string BuildPath(AddressableAssetSettings assetSettings)
        {
            return assetSettings.profileSettings.GetValueByName(assetSettings.activeProfileId, ProfileVariableNameBuild);
        }

        private static void SetGroupSchema(BundledAssetGroupSchema groupSchema)
        {
            if (groupSchema == null)
            {
                Debug.LogWarning("BundledAssetGroupSchemaがnullです。");
                return;
            }

            groupSchema.IncludeAddressInCatalog = true;
            groupSchema.IncludeGUIDInCatalog = true;
            groupSchema.IncludeLabelsInCatalog = true;
            groupSchema.IncludeInBuild = true;
            groupSchema.UseUnityWebRequestForLocalBundles = true;

            // bundleを個別にパックする
            groupSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        }

        /// <summary>
        /// Addressablesのプロファイルを作成し、アクティブにします。
        /// </summary>
        /// <returns>作成したプロファイルのID</returns>
        public static string SetOrCreateProfile()
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
                return null;
            }

            // プロファイルが既に存在するか確認
            var profileSettings = settings.profileSettings;
            var existingProfileId = profileSettings.GetProfileId(TileBuildProfileName);
            if (!string.IsNullOrEmpty(existingProfileId))
            {
                settings.activeProfileId = existingProfileId;
                return existingProfileId;
            }

            // 新しいプロファイルを作成
            var newProfileId = profileSettings.AddProfile(TileBuildProfileName, settings.activeProfileId);
            if (string.IsNullOrEmpty(newProfileId))
            {
                Debug.LogError($"プロファイル '{TileBuildProfileName}' の作成に失敗しました。");
                return null;
            }

            // プロファイルをアクティブに設定
            settings.activeProfileId = newProfileId;
            return newProfileId;
        }
        
        /// <summary>
        /// Addressablesのビルドを実行します。
        /// </summary>
        public static void BuildAddressables(TileBuildMode buildMode)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
                return;
            }

            // Addressablesのバージョン1.22の既知のバグを回避するため、GenerateBuildLayoutを一時的にオフにします。
            // これをしないとビルドに成功しているのにエラーメッセージが出現します。
            // バージョン2.0.8以降であればこの処理は不要です。
            bool prevGenerateBuildLayout = ProjectConfigData.GenerateBuildLayout;
            ProjectConfigData.GenerateBuildLayout = false;

            try
            {
                // Addressablesのビルドを実行
                AddressablesPlayerBuildResult result;
                switch (buildMode)
                {
                    case TileBuildMode.New:
                        AddressableAssetSettings.BuildPlayerContent(out result);
                        break;
                    case TileBuildMode.Add:
                        var contentStatePath = Path.Combine(BuildPath(settings), "addressables_content_state.bin");
                        result = ContentUpdateScript.BuildContentUpdate(settings, contentStatePath);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (!string.IsNullOrEmpty(result.Error))
                {
                    Debug.LogError($"Addressablesのビルドでエラーが発生しました: {result.Error}");
                }
                else
                {
                    Debug.Log("Addressablesのビルドが完了しました。");
                }
            }
            finally
            {
                // 設定を元に戻します
                ProjectConfigData.GenerateBuildLayout = prevGenerateBuildLayout;
            }
        }
        
        /// <summary>
        /// 指定したグループを削除します。
        /// </summary>
        /// <param name="groupName">削除するグループ名</param>
        /// <returns>削除に成功した場合はtrue</returns>
        public static bool RemoveGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                Debug.LogWarning("グループ名が指定されていません。");
                return false;
            }
            
            try
            {
                var settings = RequireAddressableSettings();
                if (settings == null)
                {
                    return false;
                }
                
                var group = settings.FindGroup(groupName);
                if (group == null)
                {
                    Debug.LogWarning($"グループが見つかりません: {groupName}");
                    return false;
                }
                
                if (group == settings.DefaultGroup)
                {
                    Debug.LogWarning("デフォルトグループは削除できません。");
                    return false;
                }
                
                settings.RemoveGroup(group);
                Debug.Log($"Addressableグループを削除しました: {groupName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Addressableグループの削除中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }

        public enum TileBuildMode
        {
            /// <summary> 新しいフォルダで新規ビルド </summary>
            New,
            /// <summary> 既存フォルダに追加 </summary>
            Add
        }
    }
} 