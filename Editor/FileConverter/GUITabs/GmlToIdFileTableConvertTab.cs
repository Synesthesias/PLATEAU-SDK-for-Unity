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
        protected override string DestFileExtension => "asset";
        protected override IFileConverter FileConverter => this.converter;
        

        protected override void HeaderInfoGUI()
        {
            
        }

        protected override void ConfigureGUI()
        {
            this.doOptimize = EditorGUILayout.Toggle("Optimize", this.doOptimize);
            this.doTessellate = EditorGUILayout.Toggle("Tessellate", this.doTessellate);
        }

        protected override void OnConfigureGUIChanged()
        {
            this.converter.SetConfig(this.doOptimize, this.doTessellate);
        }
    }
}