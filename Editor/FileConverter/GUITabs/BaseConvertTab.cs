using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// ファイル変換ウィンドウのタブの基底クラスです。
    /// ある拡張子からある拡張子への変換を行うGUIを提供します。
    /// <see cref="ModelFileConvertWindow"/> によって保持されます。
    /// </summary>
    public abstract class BaseConvertTab : ScrollableEditorWindowContents
    {
        
        // 変換元、変換先、Converter をサブクラスで指定します。
        protected abstract string SourceFileExtension { get; }
        public abstract string DestFileExtension { get; }
        protected abstract IFileConverter FileConverter { get; }

        public string SourceFilePath;
        public string DestFilePath;

        protected BaseConvertTab()
        {
            string defaultPath = ConvertFileSelectorGUIUtil.DefaultPath();
            this.SourceFilePath = defaultPath;
            this.DestFilePath = defaultPath;
        }

        protected override void DrawScrollable()
        {
            using (PlateauEditorStyle.VerticalScope())
            {
                HeaderInfoGUI();
            }
            ConvertFileSelectorGUIUtil.FileSelectGUI(
                ref this.SourceFilePath,
                SourceFileExtension,
                ConvertFileSelectorGUIUtil.FilePanelType.Open,
                $"Select {SourceFileExtension} File"
            );
            ConvertFileSelectorGUIUtil.FileSelectGUI(
                ref this.DestFilePath,
                DestFileExtension,
                ConvertFileSelectorGUIUtil.FilePanelType.Save,
                $"Select {DestFileExtension} Export Path"
            );
            PlateauEditorStyle.Heading1("Configure");
            using (PlateauEditorStyle.VerticalScope())
            {
                EditorGUI.BeginChangeCheck();
                ConfigureGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    OnConfigureGUIChanged();
                }
            }
            ConvertFileSelectorGUIUtil.Space();
            ConvertFileSelectorGUIUtil.PrintConvertButton(FileConverter, this.SourceFilePath, this.DestFilePath);
        }

        /// <summary>
        /// ウィンドウの上のほうに書いておきたい注意書きがあれば実装します。
        /// </summary>
        public abstract void HeaderInfoGUI();

        /// <summary>
        /// ファイル変換の設定に関するGUIを実装します。
        /// </summary>
        protected abstract void ConfigureGUI();

        /// <summary>
        /// <see cref="ConfigureGUI"/> の値に変更があったときの処理を実装します。
        /// </summary>
        protected abstract void OnConfigureGUIChanged();
    }
}