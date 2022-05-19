using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using static PlateauUnitySDK.Editor.FileConverter.Converters.ObjToFbxFileConverter;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    public class ObjToFbxFileConvertTab : BaseConvertTab
    {
        private readonly ObjToFbxFileConverter fileConverter;
        private FbxFormat fbxFormat;

        protected override string SourceFileExtension => "obj";
        public override string DestFileExtension => "fbx";
        protected override IFileConverter FileConverter => this.fileConverter;

        /// <summary>初期化処理です。</summary>
        public ObjToFbxFileConvertTab()
        {
            this.fileConverter = new ObjToFbxFileConverter();
            this.fileConverter.SetConfig(this.fbxFormat);
        }

        public override void HeaderInfoGUI()
        {
            EditorGUILayout.LabelField("入力objファイルはAssetsフォルダ内のファイルのみ指定できますが、");
            EditorGUILayout.LabelField("出力fbxファイルはAssetsフォルダの外でも指定できます。");
        }

        public override void ConfigureGUI()
        {
            this.fbxFormat = (FbxFormat)EditorGUILayout.EnumPopup("FBX Format", this.fbxFormat);
        }

        public override void OnConfigureGUIChanged()
        {
            this.fileConverter.SetConfig(this.fbxFormat);
        }
    }
}