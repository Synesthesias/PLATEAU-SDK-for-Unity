using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// Gmlファイルを読んで <see cref="Runtime.SemanticsLoader.IdToGmlFileTable"/> を出力するGUIを提供します。
    /// </summary>
    public class GmlToIdFileTableConvertTab : BaseConvertTab
    {
        private readonly GmlToIdFileTableConverter converter = new GmlToIdFileTableConverter();

        private bool doOptimize = true;

        private bool doTessellate;
        protected override string SourceFileExtension => "gml";
        public override string DestFileExtension => "asset";
        public override IFileConverter FileConverter => this.converter;


        public override void HeaderInfoGUI()
        {
            
        }

        public override void ConfigureGUI()
        {
            this.doOptimize = EditorGUILayout.Toggle("Optimize", this.doOptimize);
            this.doTessellate = EditorGUILayout.Toggle("Tessellate", this.doTessellate);
        }

        public override void OnConfigureGUIChanged()
        {
            this.converter.SetConfig(this.doOptimize, this.doTessellate);
        }
    }
}