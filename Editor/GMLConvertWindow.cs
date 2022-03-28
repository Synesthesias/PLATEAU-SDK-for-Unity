using System;
using System.IO;
using LibPLATEAU.NET;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor {
    public class GMLConvertWindow : EditorWindow {
        private string gmlFilePath = "";
        private string destinationDir = "";
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
            DefaultPathIfEmpty(ref this.destinationDir);
        }

        private static void DefaultPathIfEmpty(ref string path) {
            if (string.IsNullOrEmpty(path)) {
                path = Application.dataPath;
            }
        }

        private void OnGUI() {
            if (GUILayout.Button("1. Select GML File")) {
                ButtonSelectGMLFilePushed();
            }
            GUILayout.Label("GML file path:");
            GUILayout.TextArea($"{this.gmlFilePath}");
            
            Space();
            
            if (GUILayout.Button("2. Select Destination Directory.")) {
                ButtonSelectDestinationDirectory();
            }
            GUILayout.Label("Destination path:");
            GUILayout.TextArea($"{this.destinationDir}");

            Space();

            GUILayout.Label("3. Enter Exported obj file name:");
            using (new EditorGUILayout.HorizontalScope()) {
                this.exportObjFileName = GUILayout.TextArea(this.exportObjFileName);
                EditorGUILayout.LabelField(".obj", GUILayout.MaxWidth(50));
            }
            
            
            Space();
            
            if (GUILayout.Button("4. Convert")) {
                ButtonConvert();
            }
        }
        

        private void ButtonSelectGMLFilePushed() {
            string path = EditorUtility.OpenFilePanel("Select GML File", this.gmlFilePath, "gml");
            if (string.IsNullOrEmpty(path)) return;
            this.gmlFilePath = path;
        }
        
        private void ButtonSelectDestinationDirectory() {
            string path = EditorUtility.OpenFolderPanel("Select Destination Directory", this.destinationDir, "Dest");
            if (string.IsNullOrEmpty(path)) return;
            this.destinationDir = path;
        }

        private void ButtonConvert() {
            var parserParams = new CitygmlParserParams() {
                Optimize = 1
            };
            var cityModel = CityGml.Load(this.gmlFilePath, parserParams);
            string objPath = Path.Combine(this.destinationDir, $"{this.exportObjFileName}.obj");
            var objWriter = new ObjWriter();
            objWriter.Write(objPath, cityModel, this.gmlFilePath);
        }

        private void Space() {
            EditorGUILayout.Space(spaceWidth);
        }
    }
}
