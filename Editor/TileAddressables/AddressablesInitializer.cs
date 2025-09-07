using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace PLATEAU.Editor.TileAddressables
{
    [InitializeOnLoad]
    public class AddressablesInitializer
    {
        private const string AddressablesDataFolderName = "AddressableAssetsData";
        private const string AddressablesDataPath = "Assets/" + AddressablesDataFolderName;
        private const string AddressablesSettingsName = "AddressableAssetSettings";

        static AddressablesInitializer()
        {
            // Unityエディタ初期化直後はAssetDatabaseやAddressablesの内部が完全に初期化されていないことがあるため、
            // Addressablesの初期化は遅延コールで行う
            EditorApplication.delayCall += InitializeAddressables;
        }

        private static void InitializeAddressables()
        {
            // Addressablesの型チェック
            var type = System.Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor");
            if (type == null)
            {
                Debug.LogWarning(
                    "PLATEAU SDKの一部機能にはAddressablesパッケージが必要です。Package Managerからインストールしてください。"
                );
                return;
            }

            if (AddressableAssetSettingsDefaultObject.Settings != null)
            {
                return;
            }

            try
            {
                if (!AssetDatabase.IsValidFolder(AddressablesDataPath))
                {
                    AssetDatabase.CreateFolder("Assets", AddressablesDataFolderName);
                }

                var settings = AddressableAssetSettings.Create(
                    AddressablesDataPath,
                    AddressablesSettingsName,
                    true,
                    true
                );
                AddressableAssetSettingsDefaultObject.Settings = settings;
                AssetDatabase.Refresh();

                Debug.Log("Addressablesの設定が正常に初期化されました。");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Addressables初期化中にエラーが発生しました: {e.Message}");
            }
        }
    }
}