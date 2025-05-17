using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;

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
        /// <param name="path">新しいパス（例: "Assets/AddressableAssetsData/BuildPath"）</param>
        public static void SetGroupLoadAndBuildPath(string groupName, string path)
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

            string customPathName = "CustomPath_" + groupName;
            string profileName = "CustomProfile_" + groupName;
            if (!AddressablesUtility.TryCreateProfile(profileName, out var profileId))
            {
                Debug.LogError($"プロファイルの作成に失敗しました: {profileName}");
                return;
            }
            var profileSettings = settings.profileSettings;
            
            // プロファイル変数がなければ作成
            if (!profileSettings.GetVariableNames().Contains(customPathName))
            {
                profileSettings.CreateValue(customPathName, path);
            }

            // そのプロファイルIDにのみ値をセット
            profileSettings.SetValue(profileId, customPathName, path);

            // アクティブプロファイルをこのプロファイルに切り替え
            settings.activeProfileId = profileId;

            // グループのBuildPathとLoadPathに変数名をセット
            bundledSchema.BuildPath.SetVariableByName(settings, customPathName);
            bundledSchema.LoadPath.SetVariableByName(settings, customPathName);

            Debug.Log($"{groupName} の BuildPath/LoadPath を {path} に設定しました。");
        }

        /// <summary>
        /// 指定したグループの指定スキーマのIncludeInBuildフラグを設定します。
        /// </summary>
        /// <param name="groupName">グループ名</param>
        /// <param name="includeInBuild">ビルドに含める場合はtrue、含めない場合はfalse</param>
        public static void SetGroupIncludeInBuild(string groupName, bool includeInBuild)
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

            // スキーマIDでスキーマを取得
            var schema = group.GetSchema<AddressableAssetGroupSchema>();
            if (schema == null)
            {
                Debug.LogWarning($"スキーマが見つかりません");
                return;
            }

            // IncludeInBuildプロパティをリフレクションでセット
            var prop = schema.GetType().GetProperty("IncludeInBuild");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(schema, includeInBuild);
                UnityEditor.EditorUtility.SetDirty(group);
                Debug.Log($"{groupName} の IncludeInBuild を {includeInBuild} に設定しました。");
            }
            else
            {
                Debug.LogWarning($"IncludeInBuild プロパティがありません。");
            }
        }

        /// <summary>
        /// 新しいAddressablesプロファイルを作成します。
        /// </summary>
        /// <param name="profileName">新しいプロファイル名</param>
        /// <param name="templateProfileName">テンプレートとするプロファイル名（省略時は"Default"）</param>
        /// <returns>作成したプロファイルのID。失敗時はnull。</returns>
        private static bool TryCreateProfile(string profileName, out string newProfileId, string templateProfileName = "Default")
        {
            newProfileId = null;
            var settings = RequireAddressableSettings();
            if (settings == null)
            {
                return false;
            }

            var profileSettings = settings.profileSettings;
            string templateProfileId = profileSettings.GetProfileId(templateProfileName);
            if (string.IsNullOrEmpty(templateProfileId))
            {
                Debug.LogWarning($"テンプレートプロファイルが見つかりません: {templateProfileName}");
                return false;
            }

            newProfileId = profileSettings.AddProfile(profileName, templateProfileId);
            Debug.Log($"新しいプロファイルを作成: {profileName} (ID: {newProfileId})");
            return true;
        }
    }
} 