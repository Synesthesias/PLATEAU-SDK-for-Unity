using LibPLATEAU.NET;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウです。
    /// </summary>
    public class GmlToObjFileConvertWindow : EditorWindow {
        private readonly ConvertFileSelectorGUI fileSelectorGUI = new ConvertFileSelectorGUI();
        private GmlToObjFileConverter fileConverter;
        private bool optimizeFlg = true;
        private bool mergeMeshFlg = true;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private Vector2 scrollPosition;
        
        /// <summary> ウィンドウを表示します。 </summary>
        [MenuItem("Plateau/GML to OBJ File Converter Window")]
        private static void Init() {
            var window = GetWindow<GmlToObjFileConvertWindow>("GML Convert Window");
            window.Show();
            window.fileConverter = new GmlToObjFileConverter();
            window.fileConverter.SetConfig(window.optimizeFlg, window.mergeMeshFlg, window.axesConversion);
        }

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        private void OnEnable() {
            this.fileSelectorGUI.OnEnable();
        }

        

        /// <summary> GUI表示のメインメソッドです。 </summary>
        private void OnGUI() {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            PlateauEditorStyle.Heading1("File Convert Window : GML to OBJ");
            EditorGUILayout.Space(15f);
            EditorGUILayout.LabelField("Assetsフォルダ外のファイルも指定できます。");
            this.fileSelectorGUI.SourceFileSelectMenu("gml");
            this.fileSelectorGUI.DestinationFileSelectMenu("obj");

            
            PlateauEditorStyle.Heading1("3. Configure");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                EditorGUI.BeginChangeCheck();
                this.optimizeFlg = EditorGUILayout.Toggle("Optimize", this.optimizeFlg);
                this.mergeMeshFlg = EditorGUILayout.Toggle("Merge Mesh", this.mergeMeshFlg);
                this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("Axes Conversion", this.axesConversion);
                if (EditorGUI.EndChangeCheck()) {
                    this.fileConverter.SetConfig(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
                }
            }
            ConvertFileSelectorGUI.Space();

            this.fileSelectorGUI.PrintConvertButton(this.fileConverter);
            
            EditorGUILayout.EndScrollView();
            
        }
        
        
    }
}
