using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.GUIContents
{
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    public class GmlToObjFileConvertTab : BaseConvertTab
    {
        private bool optimizeFlg = true;
        private MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private DllLogLevel logLevel = DllLogLevel.Error;
        private GmlToObjFileConverter fileConverter;

        protected override string SourceFileExtension => "gml";
        public override string DestFileExtension => "obj";
        public override IFileConverter FileConverter => this.fileConverter;

        /// <summary>初期化処理です。</summary>
        public GmlToObjFileConvertTab()
        {
            this.fileConverter = new GmlToObjFileConverter();
            this.fileConverter.SetConfig(this.meshGranularity, this.axesConversion, this.optimizeFlg);
        }

        public override void HeaderInfoGUI()
        {
            EditorGUILayout.LabelField("Assetsフォルダ外のファイルも指定できます。");
        }

        public override void ConfigureGUI()
        {
            this.optimizeFlg = EditorGUILayout.Toggle("最適化", this.optimizeFlg);
            this.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", this.meshGranularity);
            this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("座標軸変換", this.axesConversion);
            this.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログ詳細度", this.logLevel);
        }

        public override void OnConfigureGUIChanged()
        {
            this.fileConverter.SetConfig(this.meshGranularity, this.axesConversion, this.optimizeFlg, this.logLevel);
        }
    }
}