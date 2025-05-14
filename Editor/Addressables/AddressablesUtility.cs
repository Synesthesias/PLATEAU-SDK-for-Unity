using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace PLATEAU.Editor.Addressables
{
    /// <summary>
    /// Addressablesのグループ作成やアセットのAddressable登録を行うEditor専用ユーティリティクラス
    /// </summary>
    public static class AddressablesUtility
    {
        /// <summary>
        /// 指定した名前のグループを取得。なければ新規作成。
        /// </summary>
        public static AddressableAssetGroup GetOrCreateGroup(string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
                return null;
            }

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(
                    groupName,
                    false, // readOnly
                    false, // setAsDefaultGroup
                    true,  // createDefaultSchema
                    null   // schemas
                );
            }
            return group;
        }

        /// <summary>
        /// 指定したアセットを指定グループにAddressableとして登録
        /// </summary>
        public static void RegisterAssetAsAddressable(string assetPath, string address, string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsが見つかりません。");
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
        }
    }
} 