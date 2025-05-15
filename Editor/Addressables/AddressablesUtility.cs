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
                    false, // setAsDefaultGroup
                    false, // readOnly
                    true,  // postEvent
                    null   // schemasToCopy
                );
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
    }
} 