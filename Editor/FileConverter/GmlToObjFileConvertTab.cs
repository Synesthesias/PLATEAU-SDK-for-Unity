using LibPLATEAU.NET;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    public class GmlToObjFileConvertTab : IEditorWindowContents {
        private readonly ConvertFileSelectorGUI fileSelectorGUI = new ConvertFileSelectorGUI();
        private readonly GmlToObjFileConverter fileConverter;
        private bool optimizeFlg = true;
        private bool mergeMeshFlg = true;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private Vector2 scrollPosition;
        
        /// <summary>初期化処理です。</summary>
        public GmlToObjFileConvertTab() {
            this.fileConverter = new GmlToObjFileConverter();
            this.fileConverter.SetConfig(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
            this.fileSelectorGUI.OnEnable();
        }


        /// <summary> GUI表示のメインメソッドです。 </summary>
        public void DrawGUI() {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            // ファイルの入出力指定のGUIを fileSelectorGUI に委譲して描画します。
            EditorGUILayout.LabelField("Assetsフォルダ外のファイルも指定できます。");
            this.fileSelectorGUI.SourceFileSelectMenu("gml");
            this.fileSelectorGUI.DestinationFileSelectMenu("obj");

           // 変換に関する設定をするGUIです。 
            PlateauEditorStyle.Heading1("4. Configure");
            using (PlateauEditorStyle.VerticalScope()) {
                EditorGUI.BeginChangeCheck();
                this.optimizeFlg = EditorGUILayout.Toggle("Optimize", this.optimizeFlg);
                this.mergeMeshFlg = EditorGUILayout.Toggle("Merge Mesh", this.mergeMeshFlg);
                this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("Axes Conversion", this.axesConversion);
                if (EditorGUI.EndChangeCheck()) {
                    this.fileConverter.SetConfig(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
                }
            }
            ConvertFileSelectorGUI.Space();

            // 変換ボタンです。
            this.fileSelectorGUI.PrintConvertButton(this.fileConverter);
            
            EditorGUILayout.EndScrollView();
            
        }
        
        
    }
}
