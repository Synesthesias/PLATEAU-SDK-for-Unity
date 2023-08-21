using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.Common.PathSelector
{
    /// <summary>
    /// Unityの拡張エディタで、パスを選択するGUIの基底クラスです。
    /// パス選択のダイアログ表示処理はサブクラスで実装されます。
    /// </summary>
    internal abstract class PathSelectorBase
    {
        protected string SelectedPath { get; private set; }
        public virtual string Draw(string labelText)
        {
            const int Height = 50;
            bool isButtonPressed = false;
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                using(new EditorGUILayout.HorizontalScope(GUILayout.Height(Height)))
                {
                    // 表題ラベル
                    var labelContent = new GUIContent(labelText);
                    var labelStyle = new GUIStyle(EditorStyles.label);
                    var labelSize = labelStyle.CalcSize(labelContent);
                    PlateauEditorStyle.CenterAlignVertical(() =>
                    {
                        PlateauEditorStyle.LabelSizeFit(labelContent,
                                new GUIStyle(EditorStyles.label));
                    }, GUILayout.Width(labelSize.x), GUILayout.Height(Height));
                    // パス表示
                    string displayFolderPath = string.IsNullOrEmpty(SelectedPath) ? "未選択" : SelectedPath;
                    PlateauEditorStyle.MultiLineLabelWithBox(displayFolderPath, GUILayout.Height(Height));
                    // 「参照」ボタン
                    const int ButtonWidth = 80;
                    PlateauEditorStyle.CenterAlignVertical(() =>
                    {
                        if (PlateauEditorStyle.MiniButton("参照...", ButtonWidth))
                        {
                            isButtonPressed = true;
                        }
                    }, GUILayout.Width(ButtonWidth), GUILayout.Height(Height));
                    
                }

            }
            
            // ボタンが押された時。
            // この処理は本来ならば上記の if(ボタン押下時){処理} の処理中に移動して良さそうですが、
            // Unityのバグで VerticaScope 内で時間のかかる処理をするとエラーメッセージが出るので
            // VerticalScope の外に移動しています。
            // 参考 : https://qiita.com/kumaS-kumachan/items/8d669e56feaf6e47adf1
            if (isButtonPressed)
            {
                string dialoguePath = PathSelectorDialogue();
                SelectedPath = string.IsNullOrEmpty(dialoguePath) ? SelectedPath : dialoguePath;
            }

            return SelectedPath;
        }

        /// <summary>
        /// パスを選択するダイアログを表示します。
        /// </summary>
        protected abstract string PathSelectorDialogue();
    }
}
