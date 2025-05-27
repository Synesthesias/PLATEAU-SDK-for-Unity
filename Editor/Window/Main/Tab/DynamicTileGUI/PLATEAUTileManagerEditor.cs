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
        private string previousCatalogPath;

        private void OnEnable()
        {
            var manager = (PLATEAUTileManager)target;
            previousCatalogPath = manager.CatalogPath;
            
            EditorPrefs.SetBool("PLATEAU_NEEDS_ADDRESSABLES_BUILD", false);
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
                EditorPrefs.SetBool("PLATEAU_NEEDS_ADDRESSABLES_BUILD", true);
            }

            if (EditorPrefs.GetBool("PLATEAU_NEEDS_ADDRESSABLES_BUILD"))
            {
                if (GUILayout.Button("Addressablesを再ビルド"))
                {
                    if (!File.Exists(previousCatalogPath))
                    {
                        Dialogue.Display("カタログファイルが見つかりません: " + previousCatalogPath, "OK");
                        return;
                    }
                    
                    var directoryPath = Path.GetDirectoryName(previousCatalogPath);
                    
                    // パスを再設定してビルド
                    var defaultGroup = AddressablesUtility.GetDefaultGroup();
                    AddressablesUtility.SetRemoteProfileSettings(directoryPath);
                    AddressablesUtility.SetGroupLoadAndBuildPath(defaultGroup.name);
                    
                    AddressablesUtility.BuildAddressables(false);
                    
                    Dialogue.Display("Addressablesのビルドが完了しました。", "OK");
                    EditorPrefs.SetBool("PLATEAU_NEEDS_ADDRESSABLES_BUILD", false);
                }
            }
        }
    }
}