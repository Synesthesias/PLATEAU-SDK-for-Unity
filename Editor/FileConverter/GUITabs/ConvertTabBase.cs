using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// ファイル変換ウィンドウのタブの基底クラスです。
    /// ある拡張子からある拡張子への変換を行うGUIを提供します。
    /// </summary>
    public abstract class ConvertTabBase : ScrollableEditorWindowContents
    {
        private ConvertFileSelectorGUI fileSelectorGUI = new ConvertFileSelectorGUI();
        
        // 変換元、変換先、Converter をサブクラスで指定します。
        protected abstract string SourceFileExtension { get; }
        protected abstract string DestFileExtension { get; }
        protected abstract IFileConverter FileConverter { get; }

        public ConvertTabBase()
        {
            this.fileSelectorGUI.OnEnable();
        }
        
        public override void DrawScrollable()
        {
            HeaderInfoGUI();
            this.fileSelectorGUI.SourceFileSelectMenu(SourceFileExtension);
            this.fileSelectorGUI.DestinationFileSelectMenu(DestFileExtension);
            PlateauEditorStyle.Heading1("4. Configure");
            using (PlateauEditorStyle.VerticalScope())
            {
                ConfigureGUI();   
            }
            ConvertFileSelectorGUI.Space();
            this.fileSelectorGUI.PrintConvertButton(FileConverter);
        }

        public abstract void HeaderInfoGUI();

        /// <summary>
        /// ファイル変換の設定に関するGUIをサブクラスで実装します。
        /// </summary>
        public abstract void ConfigureGUI();
    }
}