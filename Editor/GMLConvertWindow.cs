using System.IO;
using LibPLATEAU.NET;
using PlateauUnitySDK.Runtime;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor {
    public class GMLConvertWindow : EditorWindow {
        private string gmlFilePath = "";
        private string destinationFilePath = "";
        private string exportObjFileName = "exported";
        private const float spaceWidth = 20f;
        
        /// <summary> ウィンドウを表示します。 </summary>
        [MenuItem("Plateau/GML Converter Window")]
        private static void Init() {
            var window = GetWindow<GMLConvertWindow>("GML Convert Window");
            window.Show();
        }

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        private void OnEnable() {
            DefaultPathIfEmpty(ref this.gmlFilePath);
            DefaultPathIfEmpty(ref this.destinationFilePath);
        }

        private static void DefaultPathIfEmpty(ref string path) {
            if (string.IsNullOrEmpty(path)) {
                path = Application.dataPath;
            }
        }

        private void OnGUI() {
            EditorUtil.Heading1("1. Select GML File");
            if (GUILayout.Button("Select File")) {
                ButtonSelectGMLFilePushed();
            }
            GUILayout.Label("GML file path:");
            GUILayout.TextArea($"{this.gmlFilePath}");
            
            Space();
            EditorUtil.Heading1("2. Select Obj File Destination");
            
            if (GUILayout.Button("Select Destination")) {
                ButtonSelectDestination();
            }
            GUILayout.Label("Destination obj file path:");
            GUILayout.TextArea($"{this.destinationFilePath}");


            Space();
            EditorUtil.Heading1("4. Convert");
            
            if (GUILayout.Button("Convert")) {
                ButtonConvert();
            }
        }
        

        private void ButtonSelectGMLFilePushed() {
            string path = EditorUtility.OpenFilePanel("Select GML File", this.gmlFilePath, "gml");
            if (string.IsNullOrEmpty(path)) return;
            this.gmlFilePath = path;
        }
        
        private void ButtonSelectDestination() {
            string path = EditorUtility.SaveFilePanel(
                "Select Destination",
                Path.GetDirectoryName(this.gmlFilePath),
                "exported", "obj"
                );
            if (string.IsNullOrEmpty(path)) return;
            this.destinationFilePath = path;
        }

        private void ButtonConvert() {
            // TODO objWriterの設定はUIから変更できるようにする

            var gmlToObjConverter = new GmlToObjConverter(0, true, AxesConversion.RUF);
            bool result = gmlToObjConverter.Convert(this.gmlFilePath, this.destinationFilePath);
            EditorUtility.DisplayDialog(
                "Convert Result",
                result ? "Convert Complete!" : "Convert Failed...\nSee console log for detail.",
                "OK");
            if (result) {
                AssetDatabase.Refresh();
            }
            
        }

        private void Space() {
            EditorGUILayout.Space(spaceWidth);
        }
    }
}
