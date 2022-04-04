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
        private static void Open() {
            var window = GetWindow<ModelFileConvertWindow>("Model File Converter");
            window.Show();
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
            if(!this.isInitialized || this.tabContents == null) Init();
            PlateauEditorStyle.Heading1("1. Choose Convert Type");
            
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, "GML to OBJ", "OBJ to FBX");
            var tabContent = this.tabContents[this.tabIndex];
            tabContent.DrawGUI();
        }
    }
}