using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    public class GmlToObjFileConvertTab : ConvertTabBase
    {
        private bool optimizeFlg = true;
        private bool mergeMeshFlg = true;
        private AxesConversion axesConversion = AxesConversion.RUF;
        private GmlToObjFileConverter fileConverter;

        protected override string SourceFileExtension => "gml";
        protected override string DestFileExtension => "obj";
        protected override IFileConverter FileConverter => this.fileConverter;

        /// <summary>初期化処理です。</summary>
        public GmlToObjFileConvertTab()
        {
            this.fileConverter = new GmlToObjFileConverter();
            this.fileConverter.SetConfig(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
        }

        public override void HeaderInfoGUI()
        {
            EditorGUILayout.LabelField("Assetsフォルダ外のファイルも指定できます。");
        }

        public override void ConfigureGUI()
        {
            this.optimizeFlg = EditorGUILayout.Toggle("Optimize", this.optimizeFlg);
            this.mergeMeshFlg = EditorGUILayout.Toggle("Merge Mesh", this.mergeMeshFlg);
            this.axesConversion = (AxesConversion)EditorGUILayout.EnumPopup("Axes Conversion", this.axesConversion);
        }

        public override void OnConfigureGUIChanged()
        {
            this.fileConverter.SetConfig(this.optimizeFlg, this.mergeMeshFlg, this.axesConversion);
        }
    }
}