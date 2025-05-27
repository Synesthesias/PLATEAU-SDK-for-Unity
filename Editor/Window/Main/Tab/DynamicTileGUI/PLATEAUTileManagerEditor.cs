using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using PLATEAU.Util;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PLATEAU.Editor.Window.Main.Tab.DynamicTileGUI
{
    [CustomEditor(typeof(PLATEAUTileManager))]
    public class PLATEAUTileManagerEditor : UnityEditor.Editor
    {
        private const string NEEDS_ADDRESSABLES_BUILD_KEY = "PLATEAU_NEEDS_ADDRESSABLES_BUILD";
        private string previousCatalogPath;

        private void OnEnable()
        {
            var manager = (PLATEAUTileManager)target;
            previousCatalogPath = manager.CatalogPath;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var manager = (PLATEAUTileManager)target;
            if (manager.CatalogPath != previousCatalogPath)
            {
                previousCatalogPath = manager.CatalogPath;

                if (string.IsNullOrEmpty(previousCatalogPath))
                {
                    return;
                }
                EditorPrefs.SetBool(NEEDS_ADDRESSABLES_BUILD_KEY, true);
            }

            if (EditorPrefs.GetBool(NEEDS_ADDRESSABLES_BUILD_KEY))
            {
                if (GUILayout.Button("Addressablesを再ビルド"))
                {
                    if (!File.Exists(previousCatalogPath))
                    {
                        Dialogue.Display("カタログファイルが見つかりません: " + previousCatalogPath, "OK");
                        return;
                    }
                    
                    var directoryPath = Path.GetDirectoryName(previousCatalogPath);
                    if (string.IsNullOrEmpty(directoryPath))
                    {
                        Dialogue.Display("カタログファイルのディレクトリパスが無効です: " + directoryPath, "OK");
                        return;
                    }
                    
                    // パスを再設定してビルド
                    var defaultGroup = AddressablesUtility.GetDefaultGroup();
                    AddressablesUtility.SetRemoteProfileSettings(directoryPath, defaultGroup.name);
                    AddressablesUtility.SetGroupLoadAndBuildPath(defaultGroup.name);
                    
                    AddressablesUtility.BuildAddressables(false);
                    
                    Dialogue.Display("Addressablesのビルドが完了しました。", "OK");
                    EditorPrefs.SetBool(NEEDS_ADDRESSABLES_BUILD_KEY, false);
                }
            }
        }
    }
}