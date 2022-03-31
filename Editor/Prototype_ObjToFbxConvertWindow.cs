using System.IO;
using LibPLATEAU.NET;
using PlateauUnitySDK.Runtime;
using UnityEditor;
using UnityEngine;

// 仮実装、あとで消す

namespace PlateauUnitySDK.Editor {
    
    /// <summary>
    /// objファイルを読んでfbxファイルに変換して出力する機能を持ったウィンドウです。
    /// </summary>
    public class Prototype_ObjToFbxConvertWindow : EditorWindow {
        private string objFilePath = "";
        private string destinationFilePath = "";
        private const float spaceWidth = 20f;
        private bool optimizeFlg = true;
        private bool mergeMeshFlg = true;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private Vector2 scrollPosition;
        
        /// <summary> ウィンドウを表示します。 </summary>
        [MenuItem("Plateau/Prototye Obj to Fbx Convert Window")]
        private static void Init() {
            var window = GetWindow<Prototype_ObjToFbxConvertWindow>("Obj to Fbx Convert Window");
            window.Show();
        }

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        private void OnEnable() {
            DefaultPathIfEmpty(ref this.objFilePath);
            DefaultPathIfEmpty(ref this.destinationFilePath);
        }

        /// <summary>
        /// パスのデフォルト値は、ファイル選択画面で最初に表示されるディレクトリに影響します。
        /// Assetsフォルダを起点にしたほうが操作性が良さそうなので、そのようにデフォルト値を設定します。
        /// </summary>
        private static void DefaultPathIfEmpty(ref string path) {
            if (string.IsNullOrEmpty(path)) {
                path = Application.dataPath; // Assetsフォルダ
            }
        }

        /// <summary> GUI表示のメインメソッドです。 </summary>
        private void OnGUI() {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            PlateauEditorStyle.Heading1("1. Select obj File");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                if (PlateauEditorStyle.MainButton("Select obj File")) {
                    ButtonSelectGMLFilePushed();
                }
                GUILayout.Label("obj file path:");
                GUILayout.TextArea($"{this.objFilePath}");
            }
            
            
            Space();
            PlateauEditorStyle.Heading1("2. Select fbx File Destination");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                if (PlateauEditorStyle.MainButton("Select fbx Destination")) {
                    ButtonSelectDestination();
                }
                GUILayout.Label("Destination fbx file path:");
                GUILayout.TextArea($"{this.destinationFilePath}");
            }


            // Space();
            // PlateauEditorStyle.Heading1("3. Configure");
            // using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
            //     this.optimizeFlg = EditorGUILayout.Toggle("Optimize", this.optimizeFlg);
            //     this.mergeMeshFlg = EditorGUILayout.Toggle("Merge Mesh", this.mergeMeshFlg);
            //     this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("Axes Conversion", this.axesConversion);
            // }
            
            Space();
            
            PlateauEditorStyle.Heading1("4. Convert");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                if (PlateauEditorStyle.MainButton("Convert")) {
                    ButtonConvert();
                }
            }
            
            EditorGUILayout.EndScrollView();
            
        }
        

        /// <summary> ボタン押下時に呼ばれます。gmlファイルを選択するウィンドウを出します。 </summary>
        private void ButtonSelectGMLFilePushed() {
            string path = EditorUtility.OpenFilePanel("Select obj File", this.objFilePath, "obj");
            if (string.IsNullOrEmpty(path)) return;
            this.objFilePath = path;
        }
        
        /// <summary> ボタン押下時に呼ばれます。objファイルの出力先を選択するウィンドウを出します。 </summary>
        private void ButtonSelectDestination() {
            string path = EditorUtility.SaveFilePanel(
                "Select Destination",
                Path.GetDirectoryName(this.objFilePath),
                "exportedFbx", "fbx"
                );
            if (string.IsNullOrEmpty(path)) return;
            this.destinationFilePath = path;
        }

        /// <summary> ボタン押下時に呼ばれます。gmlからobjに変換し、結果を表示します。 </summary>
        private void ButtonConvert() {
            // var gmlToObjConverter = new GmlToObjConverter(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
            // bool result = gmlToObjConverter.Convert(this.objFilePath, this.destinationFilePath);
            // EditorUtility.DisplayDialog(
            //     "Convert Result",
            //     result ? "Convert Complete!" : "Convert Failed...\nSee console log for detail.",
            //     "OK");
            // if (result) {
            //     AssetDatabase.Refresh();
            // }

            var objToFbxConverter = new Prototype_ObjToFbxConverter();
            objToFbxConverter.ConvertFile();

        }

        /// <summary> 空白を表示します。 </summary>
        private static void Space() {
            EditorGUILayout.Space(spaceWidth);
        }
    }
}
