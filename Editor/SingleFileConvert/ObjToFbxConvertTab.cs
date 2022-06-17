using PlateauUnitySDK.Editor.Converters;
using UnityEditor;
using static PlateauUnitySDK.Editor.Converters.ObjToFbxConverter;

namespace PlateauUnitySDK.Editor.SingleFileConvert
{
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    internal class ObjToFbxConvertTab : BaseConvertTab
    {
        private readonly ObjToFbxConverter converter;
        private FbxFormat fbxFormat;

        protected override string SourceFileExtension => "obj";
        public override string DestFileExtension => "fbx";
        public override IFileConverter FileConverter => this.converter;

        /// <summary>初期化処理です。</summary>
        public ObjToFbxConvertTab()
        {
            this.converter = new ObjToFbxConverter();
            this.converter.SetConfig(this.fbxFormat);
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
            this.converter.SetConfig(this.fbxFormat);
        }
    }
}