using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.SingleFileConvert
{
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    internal class GmlToObjConvertTab : BaseConvertTab
    {
        private bool optimizeFlg = true;
        private MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private DllLogLevel logLevel = DllLogLevel.Error;
        private readonly GmlToObjConverter converter;
        private readonly GmlToObjConverterConfig converterConf;

        protected override string SourceFileExtension => "gml";
        public override string DestFileExtension => "obj";
        public override ISingleFileConverter SingleFileConverter => this.converter;

        /// <summary>初期化処理です。</summary>
        public GmlToObjConvertTab()
        {
            this.converter = new GmlToObjConverter();
            this.converterConf = new GmlToObjConverterConfig();
            ApplyGUIToConfig();
        }

        /// <summary>
        /// GUI上での設定を <see cref="GmlToObjConverter"/> に反映させます。
        /// </summary>
        private void ApplyGUIToConfig()
        {
            this.converterConf.MeshGranularity = this.meshGranularity;
            this.converterConf.AxesConversion = this.axesConversion;
            this.converterConf.OptimizeFlag = this.optimizeFlg;
            this.converterConf.LogLevel = this.logLevel;
            this.converter.Config = this.converterConf;
        }

        public override void HeaderInfoGUI()
        {
            EditorGUILayout.LabelField("Assetsフォルダ外のファイルも指定できます。");
        }

        protected override void ConfigureGUI()
        {
            this.optimizeFlg = EditorGUILayout.Toggle("最適化", this.optimizeFlg);
            this.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", this.meshGranularity);
            this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("座標軸変換", this.axesConversion);
            this.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログ詳細度", this.logLevel);
        }

        protected override void OnConfigureGUIChanged()
        {
            ApplyGUIToConfig();
        }
    }
}