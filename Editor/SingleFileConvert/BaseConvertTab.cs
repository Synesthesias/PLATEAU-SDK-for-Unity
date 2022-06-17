using PlateauUnitySDK.Editor.Converters;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;

namespace PlateauUnitySDK.Editor.SingleFileConvert
{
    /// <summary>
    /// ファイル変換ウィンドウのタブの基底クラスです。
    /// ある拡張子からある拡張子への変換を行うGUIを提供します。
    /// <see cref="SingleFileConvertWindow"/> によって保持されます。
    /// </summary>
    internal abstract class BaseConvertTab : ScrollableEditorWindowContents
    {
        
        // 変換元、変換先、Converter をサブクラスで指定します。
        protected abstract string SourceFileExtension { get; }
        public abstract string DestFileExtension { get; }
        public abstract IFileConverter FileConverter { get; }

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
            SrcFileSelectGUI();
            DstFileSelectGUI();
            ConfigureGUIOuter();
            ConvertFileSelectorGUIUtil.Space();
            ConvertFileSelectorGUIUtil.PrintConvertButton(Convert);
        }

        /// <summary>
        /// ウィンドウの上のほうに書いておきたい注意書きがあれば実装します。
        /// </summary>
        public abstract void HeaderInfoGUI();

        protected virtual void SrcFileSelectGUI()
        {
            ConvertFileSelectorGUIUtil.FileSelectGUI(
                ref this.SourceFilePath,
                SourceFileExtension,
                ConvertFileSelectorGUIUtil.FilePanelType.Open,
                $"Select {SourceFileExtension} File"
            );
        }

        /// <summary>
        /// 変換先ファイルを選択するGUIを表示します。
        /// </summary>
        public virtual void DstFileSelectGUI()
        {
            ConvertFileSelectorGUIUtil.FileSelectGUI(
                ref this.DestFilePath,
                DestFileExtension,
                ConvertFileSelectorGUIUtil.FilePanelType.Save,
                $"Select {DestFileExtension} Export Path"
            );
        }

        /// <summary>
        /// <see cref="ConfigureGUI"/> と <see cref="OnConfigureGUIChanged"/> を呼びます。
        /// </summary>
        public void ConfigureGUIOuter()
        {
            HeaderDrawer.Draw($"Configure {DestFileExtension}");
            using (PlateauEditorStyle.VerticalScope())
            {
                EditorGUI.BeginChangeCheck();
                ConfigureGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    OnConfigureGUIChanged();
                }
            }
        }

        /// <summary>
        /// ファイル変換の設定に関するGUIを実装します。
        /// </summary>
        protected abstract void ConfigureGUI();

        /// <summary>
        /// <see cref="ConfigureGUI"/> の値に変更があったときの処理を実装します。
        /// </summary>
        protected abstract void OnConfigureGUIChanged();

        /// <summary>
        /// 変換し、成否をboolで返します。
        /// </summary>
        protected virtual bool Convert()
        {
            return FileConverter.Convert(this.SourceFilePath, this.DestFilePath);
        }
    }
}