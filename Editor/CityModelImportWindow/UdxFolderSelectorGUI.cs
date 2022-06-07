using System;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// udxフォルダを選択するGUIを提供します。
    /// </summary>
    public class UdxFolderSelectorGUI
    {
        private string udxFolderPath;
        private event Action<string> OnUdxPathChanged;

        public UdxFolderSelectorGUI(Action<string> onUdxPathChanged)
        {
            OnUdxPathChanged += onUdxPathChanged;
        }

        /// <summary>
        /// udxフォルダを選択するGUIを表示し、選択されたフォルダのパスを返します。
        /// </summary>
        public string Draw()
        {
            HeaderDrawer.Draw("取得データ選択");
            using (new EditorGUILayout.HorizontalScope())
            {
                this.udxFolderPath = EditorGUILayout.TextField("インポートフォルダ", this.udxFolderPath);
                if (PlateauEditorStyle.MainButton("参照..."))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("udxフォルダ選択", Application.dataPath, "udx");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        this.udxFolderPath = selectedPath;
                        OnUdxPathChanged?.Invoke(selectedPath);
                    }
                }
            }

            return this.udxFolderPath;
        }

    }
}