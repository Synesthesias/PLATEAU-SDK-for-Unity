using LibPLATEAU.NET;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウです。
    /// </summary>
    public class ObjToFbxFileConvertWindow : EditorWindow {
        private readonly ConvertFileSelectorGUI fileSelectorGUI = new ConvertFileSelectorGUI();
        private ObjToFbxFileConverter fileConverter;
        private Vector2 scrollPosition;
        
        /// <summary> ウィンドウを表示します。 </summary>
        [MenuItem("Plateau/OBJ to FBX File Converter Window")]
        private static void Init() {
            var window = GetWindow<ObjToFbxFileConvertWindow>("Obj to Fbx Convert Window");
            window.Show();
            window.fileConverter = new ObjToFbxFileConverter();
            window.fileConverter.SetConfig();
        }

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        private void OnEnable() {
            this.fileSelectorGUI.OnEnable();
        }

        

        /// <summary> GUI表示のメインメソッドです。 </summary>
        private void OnGUI() {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            PlateauEditorStyle.Heading1("File Convert Window : OBJ to FBX");
            EditorGUILayout.Space(15f);
            EditorGUILayout.LabelField("入力objファイルはAssetsフォルダ内のファイルのみ指定できますが、");
            EditorGUILayout.LabelField("出力fbxファイルはAssetsフォルダの外でも指定できます。");
            this.fileSelectorGUI.SourceFileSelectMenu("obj");
            this.fileSelectorGUI.DestinationFileSelectMenu("fbx");

            
            PlateauEditorStyle.Heading1("3. Configure");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                EditorGUI.BeginChangeCheck();
                // TODO ここに設定を記載
                if (EditorGUI.EndChangeCheck()) {
                    this.fileConverter.SetConfig();
                }
            }
            ConvertFileSelectorGUI.Space();

            this.fileSelectorGUI.PrintConvertButton(this.fileConverter);
            
            EditorGUILayout.EndScrollView();
            
        }
        
        
    }
}
