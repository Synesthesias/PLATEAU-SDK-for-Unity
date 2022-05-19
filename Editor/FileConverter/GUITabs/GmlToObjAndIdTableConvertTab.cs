using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    // TODO BaseConvertTab.cs と共通する箇所が多いのでまとめられるか検討
    public class GmlToObjAndIdTableConvertTab : ScrollableEditorWindowContents
    {
        
        private BaseConvertTab[] tabs = new BaseConvertTab[]
        {
            new GmlToObjFileConvertTab(),
            new GmlToIdFileTableConvertTab()
        };

        private string SourceFileExtension => "gml";
        private string sourceFilePath;

        public GmlToObjAndIdTableConvertTab()
        {
            this.sourceFilePath = ConvertFileSelectorGUIUtil.DefaultPath();
        }
        
        protected override void DrawScrollable()
        {
            using (PlateauEditorStyle.VerticalScope())
            {
                foreach (var tab in tabs)
                {
                    tab.HeaderInfoGUI();
                }    
            }
            ConvertFileSelectorGUIUtil.FileSelectGUI(
                ref this.sourceFilePath,
                SourceFileExtension,
                ConvertFileSelectorGUIUtil.FilePanelType.Open,
                $"Select {SourceFileExtension} File"
            );
            foreach (var tab in tabs)
            {
                tab.SourceFilePath = this.sourceFilePath;
            }

            foreach (var tab in tabs)
            {
                ConvertFileSelectorGUIUtil.FileSelectGUI(
                    ref tab.DestFilePath,
                    tab.DestFileExtension,
                    ConvertFileSelectorGUIUtil.FilePanelType.Save,
                    $"Select {tab.DestFileExtension} export path"
                );
            }

            foreach (var tab in tabs)
            {
                PlateauEditorStyle.Heading1($"Configure {tab.DestFileExtension}");
                using (PlateauEditorStyle.VerticalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    tab.ConfigureGUI();
                    if (EditorGUI.EndChangeCheck())
                    {
                        tab.OnConfigureGUIChanged();
                    }
                }
            }
            
            

        }
    }
}