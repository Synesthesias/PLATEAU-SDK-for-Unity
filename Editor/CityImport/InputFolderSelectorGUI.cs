﻿using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Util;
using UnityEditor;
using Application = UnityEngine.Application;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// 入力フォルダのパスを選択するGUIを提供します。
    /// 新しいパスが選択されたとき、 <see cref="onPathChangedListener"/> が呼ばれます。
    /// </summary>
    internal class InputFolderSelectorGUI
    {
        private string folderPath;
        private readonly IInputPathChangedEventListener onPathChangedListener;
        
        public enum PathChangeMethod{SetterProperty, Dialogue}


        private string FolderPath
        {
            set
            {
                bool isChanged = this.folderPath != value;
                this.folderPath = value;
                if (isChanged)
                {
                    this.onPathChangedListener.OnInputPathChanged(this.folderPath, PathChangeMethod.SetterProperty);
                }
            }
        }

        public InputFolderSelectorGUI(IInputPathChangedEventListener pathChangedEventListener)
        {
            this.onPathChangedListener = pathChangedEventListener;
        }


        /// <summary>
        /// フォルダを選択するGUIを表示し、選択されたフォルダのパスを返します。
        /// </summary>
        public string Draw(string title, string folderPathArg)
        {
            FolderPath = folderPathArg;
            
            HeaderDrawer.Draw(title);
            bool isButtonPressed = false;
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("入力フォルダ");
                
                if (PlateauEditorStyle.MiniButton("参照..."))
                {
                    isButtonPressed = true;
                }
            }

            // この処理は本来ならば上記の if(ボタン押下時){処理} の処理中に移動して良さそうですが、
            // Unityのバグで VerticaScope 内で時間のかかる処理をするとエラーメッセージが出るので
            // VerticalScope の外に移動しています。
            // 参考 : https://qiita.com/kumaS-kumachan/items/8d669e56feaf6e47adf1
            if (isButtonPressed)
            {
                string selectedPath = EditorUtility.OpenFolderPanel("フォルダ選択", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    this.folderPath = selectedPath;
                    this.onPathChangedListener.OnInputPathChanged(this.folderPath, PathChangeMethod.Dialogue);
                }
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                string displayFolderPath = string.IsNullOrEmpty(this.folderPath) ? "未選択" : this.folderPath;
                PlateauEditorStyle.MultiLineLabelWithBox(displayFolderPath);
            }

            return this.folderPath;
        }

        public bool IsPlateauFolderSelected()
        {
            return GmlSearcherModel.IsPathPlateauRoot(this.folderPath);
        }

        public void DisplayGuidanceAboutCopy()
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.HelpBox(
                    ShouldCopyToStreamingFolder()
                        ? $"入力フォルダは {PathUtil.FullPathToAssetsPath(PlateauUnityPath.StreamingGmlFolder)} にコピーされます。"
                        : "入力フォルダは既に StreamingAssets 内にあるのでコピーは省略されます。", MessageType.Info);
            }
        }

        /// <summary>
        /// インポート元パスが StreamingAssets の外であれば、そこにコピーすべきなので、
        /// パスを見てコピーすべきかどうかチェックします。
        /// </summary>
        private bool ShouldCopyToStreamingFolder()
        {
            return !CopyPlateauSrcFiles.IsInStreamingAssets(this.folderPath);
        }
        
    }

    /// <summary>
    /// 入力パスが変わったことの通知を受け取るインターフェイスです。
    /// </summary>
    internal interface IInputPathChangedEventListener
    {
        public void OnInputPathChanged(string path, InputFolderSelectorGUI.PathChangeMethod changeMethod);
    }
}