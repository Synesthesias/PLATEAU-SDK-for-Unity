using System;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// 入力フォルダのパスを選択するGUIを提供します。
    /// 新しいパスが選択されたとき、 <see cref="OnPathChanged"/> が呼ばれます。
    /// </summary>
    internal class InputFolderSelectorGUI
    {
        private string folderPath;

        public string FolderPath
        {
            set
            {
                bool isChanged = this.folderPath != value;
                this.folderPath = value;
                if (isChanged)
                {
                    OnPathChanged?.Invoke(this.folderPath);
                }
            }
        }

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
            // bool isPathChanged = false;
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("入力フォルダ");
                string displayFolderPath = string.IsNullOrEmpty(this.folderPath) ? "未選択" : this.folderPath;
                PlateauEditorStyle.MultiLineLabelWithBox(displayFolderPath);

                    if (PlateauEditorStyle.MiniButton("参照..."))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel(title, Application.dataPath, "udx");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        this.folderPath = selectedPath;
                        OnPathChanged?.Invoke(this.folderPath);
                    }
                }
            }


            return this.folderPath;
        }

    }
}