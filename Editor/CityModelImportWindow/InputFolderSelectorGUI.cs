using System;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// 入力フォルダのパスを選択するGUIを提供します。
    /// 新しいパスが選択されたとき、 <see cref="OnPathChanged"/> が呼ばれます。
    /// </summary>
    public class InputFolderSelectorGUI
    {
        private string folderPath;
        private event Action<string> OnPathChanged;

        public InputFolderSelectorGUI(Action<string> onPathChanged)
        {
            OnPathChanged += onPathChanged;
        }

        /// <summary>
        /// フォルダを選択するGUIを表示し、選択されたフォルダのパスを返します。
        /// </summary>
        public string Draw(string title)
        {
            HeaderDrawer.Draw(title);
            using (new EditorGUILayout.HorizontalScope())
            {
                this.folderPath = EditorGUILayout.TextField("入力フォルダ", this.folderPath);
                if (PlateauEditorStyle.MainButton("参照..."))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel(title, Application.dataPath, "udx");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        this.folderPath = selectedPath;
                        OnPathChanged?.Invoke(selectedPath);
                    }
                }
            }

            return this.folderPath;
        }

    }
}