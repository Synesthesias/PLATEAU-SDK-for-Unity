using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;
using static PlateauUnitySDK.Editor.FileConverter.Converters.ObjToFbxFileConverter;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    public class ObjToFbxFileConvertTab : ScrollableEditorWindowContents
    {
        private readonly ConvertFileSelectorGUI fileSelectorGUI = new ConvertFileSelectorGUI();
        private readonly ObjToFbxFileConverter fileConverter;
        private FbxFormat fbxFormat;

        /// <summary>初期化処理です。</summary>
        public ObjToFbxFileConvertTab()
        {
            this.fileConverter = new ObjToFbxFileConverter();
            this.fileConverter.SetConfig(this.fbxFormat);
            this.fileSelectorGUI.OnEnable();
        }


        /// <summary> GUI表示のメインメソッドです。 </summary>
        public override void DrawScrollable()
        {
            // ファイルの入出力指定のGUIを fileSelectorGUI に委譲して描画します。
            using (PlateauEditorStyle.VerticalScope())
            {
                EditorGUILayout.LabelField("入力objファイルはAssetsフォルダ内のファイルのみ指定できますが、");
                EditorGUILayout.LabelField("出力fbxファイルはAssetsフォルダの外でも指定できます。");
            }

            this.fileSelectorGUI.SourceFileSelectMenu("obj");
            this.fileSelectorGUI.DestinationFileSelectMenu("fbx");

            // fbxファイル特有の設定をするGUIです。
            PlateauEditorStyle.Heading1("4. Configure");
            using (PlateauEditorStyle.VerticalScope())
            {
                EditorGUI.BeginChangeCheck();
                this.fbxFormat = (FbxFormat)EditorGUILayout.EnumPopup("FBX Format", this.fbxFormat);
                if (EditorGUI.EndChangeCheck()) this.fileConverter.SetConfig(this.fbxFormat);
            }

            ConvertFileSelectorGUI.Space();

            // 変換ボタンです。
            this.fileSelectorGUI.PrintConvertButton(this.fileConverter);
        }
    }
}