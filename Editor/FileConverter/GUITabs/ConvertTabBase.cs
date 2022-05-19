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
    public abstract class ConvertTabBase : ScrollableEditorWindowContents
    {
        
        // 変換元、変換先、Converter をサブクラスで指定します。
        protected abstract string SourceFileExtension { get; }
        protected abstract string DestFileExtension { get; }
        protected abstract IFileConverter FileConverter { get; }
        
        private string sourceFilePath;
        private string destinationFilePath;

        protected ConvertTabBase()
        {
            ConvertFileSelectorGUIUtil.SetDefaultPath(ref this.sourceFilePath, ref this.destinationFilePath);
        }

        protected override void DrawScrollable()
        {
            using (PlateauEditorStyle.VerticalScope())
            {
                HeaderInfoGUI();
            }
            ConvertFileSelectorGUIUtil.SourceFileSelectMenu(SourceFileExtension, ref this.sourceFilePath);
            ConvertFileSelectorGUIUtil.DestinationFileSelectMenu(ref this.destinationFilePath, DestFileExtension);
            PlateauEditorStyle.Heading1("4. Configure");
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
            ConvertFileSelectorGUIUtil.PrintConvertButton(FileConverter, this.sourceFilePath, this.destinationFilePath);
        }

        /// <summary>
        /// ウィンドウの上のほうに書いておきたい注意書きがあれば実装します。
        /// </summary>
        protected abstract void HeaderInfoGUI();

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