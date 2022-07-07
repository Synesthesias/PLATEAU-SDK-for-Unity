using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private event Action<string, PathChangeMethod> OnPathChanged;
        
        public enum PathChangeMethod{SetterProperty, Dialogue}
        

        public string FolderPath
        {
            set
            {
                bool isChanged = this.folderPath != value;
                this.folderPath = value;
                if (isChanged)
                {
                    OnPathChanged?.Invoke(this.folderPath, PathChangeMethod.SetterProperty);
                }
            }
        }

        public InputFolderSelectorGUI(Action<string, PathChangeMethod> onPathChanged)
        {
            OnPathChanged += onPathChanged;
        }

        /// <summary>
        /// フォルダを選択するGUIを表示し、選択されたフォルダのパスを返します。
        /// </summary>
        public string Draw(string title)
        {
            HeaderDrawer.Draw(title);
            bool isButtonPressed = false;
            // var folderSelectTasks = new List<Task>();
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("入力フォルダ");
                
                if (PlateauEditorStyle.MiniButton("参照..."))
                {
                    // folderSelectTasks.Add(FolderSelectDialogue());
                    isButtonPressed = true;
                }
            }

            // フォルダ選択ダイアログの処理をここで行います。
            // 本当なら Task は使わずに、普通に上の if(ボタン押下){} の中に処理を書きたいところですが、
            // Unityのバグで、 VerticalScope 内で時間のかかる処理をするとエラーメッセージが出てしまいます。
            // そこで VerticalScope の外に処理を持ってくるために
            // foreach(Task task in folderSelectTasks) task.RunSynchronously();
            if (isButtonPressed)
            {
                string selectedPath = EditorUtility.OpenFolderPanel("フォルダ選択", Application.dataPath, "");
                Debug.Log($"selectedPath : {selectedPath}");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    this.folderPath = selectedPath;
                    OnPathChanged?.Invoke(this.folderPath, PathChangeMethod.Dialogue);
                    Debug.Log($"folderPath = {this.folderPath}");
                }
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                string displayFolderPath = string.IsNullOrEmpty(this.folderPath) ? "未選択" : this.folderPath;
                PlateauEditorStyle.MultiLineLabelWithBox(displayFolderPath);
            }

            return this.folderPath;
        }

        private Task FolderSelectDialogue()
        {
            return new Task(() =>
            {
                string selectedPath = EditorUtility.OpenFolderPanel("フォルダ選択", Application.dataPath, "");
                Debug.Log($"selectedPath : {selectedPath}");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    this.folderPath = selectedPath;
                    OnPathChanged?.Invoke(this.folderPath, PathChangeMethod.Dialogue);
                    Debug.Log($"folderPath = {this.folderPath}");
                }
            });
            

            // await Task.Delay(100);
        }
        
        

    }
}