using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// ファイル変換ウィンドウのタブの基底クラスです。
    /// ある拡張子からある拡張子への変換を行うGUIを提供します。
    /// </summary>
    public abstract class ConvertTabBase : ScrollableEditorWindowContents
    {
        private readonly ConvertFileSelectorGUIUtil fileSelectorGUIUtil = new ConvertFileSelectorGUIUtil();
        
        // 変換元、変換先、Converter をサブクラスで指定します。
        protected abstract string SourceFileExtension { get; }
        protected abstract string DestFileExtension { get; }
        protected abstract IFileConverter FileConverter { get; }

        protected ConvertTabBase()
        {
            this.fileSelectorGUIUtil.OnEnable();
        }

        protected override void DrawScrollable()
        {
            using (PlateauEditorStyle.VerticalScope())
            {
                HeaderInfoGUI();
            }
            this.fileSelectorGUIUtil.SourceFileSelectMenu(SourceFileExtension);
            this.fileSelectorGUIUtil.DestinationFileSelectMenu(DestFileExtension);
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
            this.fileSelectorGUIUtil.PrintConvertButton(FileConverter);
        }

        protected abstract void HeaderInfoGUI();

        /// <summary>
        /// ファイル変換の設定に関するGUIをサブクラスで実装します。
        /// </summary>
        protected abstract void ConfigureGUI();

        /// <summary>
        /// <see cref="ConfigureGUI"/> の値に変更があったときの処理を実装します。
        /// </summary>
        protected abstract void OnConfigureGUIChanged();
    }
}