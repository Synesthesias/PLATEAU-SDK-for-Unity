using System;
using PLATEAU.CityExport;
using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGUIParts
{
    /// <summary>
    /// エクスポートで出力するファイルフォーマットを選択するGUIです。
    /// </summary>
    public class MeshFileFormatGui
    {
        public MeshFileFormat SelectedFormat { get; private set; } = MeshFileFormat.FBX;
        public event Action<MeshFileFormat> OnFormatChanged;
        public void Draw()
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var prevFormat = SelectedFormat;
                SelectedFormat = (MeshFileFormat)EditorGUILayout.EnumPopup("出力形式", SelectedFormat);
                if (prevFormat != SelectedFormat)
                {
                    InvokeOnFormatChanged();
                }
            }
        }

        public void InvokeOnFormatChanged()
        {
            OnFormatChanged?.Invoke(SelectedFormat);
        }
    }
}