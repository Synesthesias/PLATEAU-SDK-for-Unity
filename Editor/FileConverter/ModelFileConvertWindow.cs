using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    public class ModelFileConvertWindow : EditorWindow {
        private IEditorWindowContents[] tabContents;
        private int tabIndex;
        private bool isInitialized;

        [MenuItem("Plateau/Model File Converter Window")]
        private static void Show() {
            var window = GetWindow<ModelFileConvertWindow>("Model File Converter");
            window.Init();
        }

        private void Init() {
            this.tabContents = new IEditorWindowContents[] {
                new GmlToObjFileConvertTab(),
                new ObjToFbxFileConvertTab()
            };
            this.isInitialized = true;
        }

        private void OnGUI() {
            if(!this.isInitialized) Init();
            PlateauEditorStyle.Heading1("1. Choose Convert Type");
            using (PlateauEditorStyle.VerticalScope()) {
                using (new EditorGUILayout.HorizontalScope()) {
                    this.tabIndex = GUILayout.Toolbar(
                        this.tabIndex,
                        new[] {"GML to OBJ", "OBJ to FBX"},
                        "LargeButton",
                        GUI.ToolbarButtonSize.Fixed
                    );
                }
            }

            this.tabContents[this.tabIndex].DrawGUI();
        }
    }
}