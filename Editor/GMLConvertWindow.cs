using System.IO;
using LibPLATEAU.NET;
using PlateauUnitySDK.Runtime;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor {
    
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウです。
    /// </summary>
    public class GMLConvertWindow : EditorWindow {
        private FileConvertEditorWindowUtil windowUtil = new FileConvertEditorWindowUtil();
        private GmlToObjConverter converter;
        private bool optimizeFlg = true;
        private bool mergeMeshFlg = true;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private Vector2 scrollPosition;
        
        /// <summary> ウィンドウを表示します。 </summary>
        [MenuItem("Plateau/GML Converter Window")]
        private static void Init() {
            var window = GetWindow<GMLConvertWindow>("GML Convert Window");
            window.Show();
            window.converter = new GmlToObjConverter();
            window.converter.SetConfig(window.optimizeFlg, window.mergeMeshFlg, window.axesConversion);
        }

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        private void OnEnable() {
            this.windowUtil.OnEnable();
        }

        

        /// <summary> GUI表示のメインメソッドです。 </summary>
        private void OnGUI() {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            PlateauEditorStyle.Heading1("File Convert Window : GML to OBJ");
            EditorGUILayout.Space(15f);
            
            this.windowUtil.PrintSourceFileSelectMenu("gml");
            this.windowUtil.PrintDestinationFileSelectMenu("obj");

            
            PlateauEditorStyle.Heading1("3. Configure");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                EditorGUI.BeginChangeCheck();
                this.optimizeFlg = EditorGUILayout.Toggle("Optimize", this.optimizeFlg);
                this.mergeMeshFlg = EditorGUILayout.Toggle("Merge Mesh", this.mergeMeshFlg);
                this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("Axes Conversion", this.axesConversion);
                if (EditorGUI.EndChangeCheck()) {
                    this.converter.SetConfig(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
                }
            }
            FileConvertEditorWindowUtil.Space();

            this.windowUtil.PrintConvertButton(converter);
            
            EditorGUILayout.EndScrollView();
            
        }
        
        
    }
}
