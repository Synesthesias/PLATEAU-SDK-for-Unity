using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace PLATEAU.Editor.Addressables
{
    /// <summary>
    /// Addressablesのグループ作成やアセットのAddressable登録を行うEditor専用ユーティリティクラス
    /// </summary>
    public static class AddressablesUtility
    {
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
            var bundledSchema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
            if (bundledSchema == null)
            {
                bundledSchema = group.AddSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
            }
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
                return;
            }

            var group = GetOrCreateGroup(groupName);
            if (group == null)
            {
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

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                Debug.LogWarning($"グループが見つかりません: {groupName}");
                return;
            }

            var bundledSchema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
            if (bundledSchema == null)
            {
                Debug.LogWarning("BundledAssetGroupSchemaが見つかりません。");
                return;
            }
            
            if (!settings.profileSettings.GetVariableNames().Contains(groupName))
            {
                settings.profileSettings.CreateValue(groupName, "");
            }

            // グループのBuildPathとLoadPathに変数名をセット
            bundledSchema.BuildPath.SetVariableByName(settings, groupName);
            bundledSchema.LoadPath.SetVariableByName(settings, groupName);

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
            if (!profileSettings.GetVariableNames().Contains(pathVar))
            {
                profileSettings.CreateValue(pathVar, path);
            }
            profileSettings.SetValue(settings.activeProfileId, pathVar, path);

            settings.RemoteCatalogBuildPath.SetVariableByName(settings, pathVar);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, pathVar);
        }
        
        /// <summary>
        /// デフォルトのプロファイル設定を行います。
        /// </summary>
        public static void SetDefaultProfileSettings(string groupName)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                return;
            }

            // デフォルトプロファイルの作成
            var profileId = SetOrCreateProfile("Default");
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogError("デフォルトプロファイルの作成に失敗しました。");
                return;
            }

            // プロファイルをアクティブに設定
            settings.activeProfileId = profileId;
            
            settings.BuildRemoteCatalog = true;
            var profileSettings = settings.profileSettings;
            profileSettings.GetVariableNames().ForEach((name) => 
            {
                Debug.Log("プロファイル変数: " + name);
            });

            settings.RemoteCatalogBuildPath.SetVariableByName(settings, "Local.BuildPath");
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, "Local.LoadPath");

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                Debug.LogWarning($"グループが見つかりません: {groupName}");
                return;
            }

            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema == null)
            {
                Debug.LogWarning("BundledAssetGroupSchemaが見つかりません。");
                return;
            }

            SetGroupSchema(bundledSchema);
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
            groupSchema.BundleMode = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        }

        /// <summary>
        /// Addressablesのプロファイルを作成し、アクティブにします。
        /// </summary>
        /// <param name="profileName">作成するプロファイル名</param>
        /// <returns>作成したプロファイルのID</returns>
        public static string SetOrCreateProfile(string profileName)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
                return null;
            }

            // プロファイルが既に存在するか確認
            var profileSettings = settings.profileSettings;
            var existingProfileId = profileSettings.GetProfileId(profileName);
            if (!string.IsNullOrEmpty(existingProfileId))
            {
                settings.activeProfileId = existingProfileId;
                return existingProfileId;
            }

            // 新しいプロファイルを作成
            var newProfileId = profileSettings.AddProfile(profileName, settings.activeProfileId);
            if (string.IsNullOrEmpty(newProfileId))
            {
                Debug.LogError($"プロファイル '{profileName}' の作成に失敗しました。");
                return null;
            }

            // プロファイルをアクティブに設定
            settings.activeProfileId = newProfileId;
            return newProfileId;
        }
        
        /// <summary>
        /// Addressablesのビルドを実行します。
        /// </summary>
        /// <param name="cleanCache">ビルド前にキャッシュを消す場合はtrue</param>
        public static void BuildAddressables(bool cleanCache = false)
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
                return;
            }

            if (cleanCache)
            {
                UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.CleanPlayerContent();
                Debug.Log("Addressablesのキャッシュをクリアしました。");
            }

            // Addressablesのビルドを実行
            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent(out var result);
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"Addressablesのビルドでエラーが発生しました: {result.Error}");
            }
            else
            {
                Debug.Log("Addressablesのビルドが完了しました。");
            }
        }

        /// <summary>
        /// デフォルトグループ以外のグループを削除します。
        /// </summary>
        public static void RemoveNonDefaultGroups(string targetLabel = "")
        {
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                return;
            }

            var defaultGroup = settings.DefaultGroup;
            if (defaultGroup == null)
            {
                Debug.LogWarning("デフォルトグループが設定されていません。");
                return;
            }

            // グループのリストをコピー（削除中に変更されるため）
            var groups = settings.groups.ToList();
            foreach (var group in groups)
            {
                if (!string.IsNullOrEmpty(targetLabel) &&
                    !group.entries.Any(e => e.labels.Contains(targetLabel)))
                {
                    continue;
                }
                
                if (group == defaultGroup)
                {
                    continue;
                }
                settings.RemoveGroup(group);
                Debug.Log($"グループを削除しました: {group.Name}");
            }
        }
    }
} 