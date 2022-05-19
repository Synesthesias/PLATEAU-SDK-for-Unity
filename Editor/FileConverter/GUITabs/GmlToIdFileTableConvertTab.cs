using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    public class GmlToIdFileTableConvertTab : ConvertTabBase
    {
        private GmlToIdFileTableConverter converter = new GmlToIdFileTableConverter();

        private bool doOptimize = true;

        private bool doTessellate = false;
        protected override string SourceFileExtension => "gml";
        protected override string DestFileExtension => "asset";
        protected override IFileConverter FileConverter => this.converter;
        

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