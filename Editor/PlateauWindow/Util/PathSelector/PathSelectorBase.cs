using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.Util.PathSelector
{
    internal abstract class PathSelectorBase
    {
        protected string SelectedPath { get; set; }
        public virtual string Draw(string labelText)
        {
            bool isButtonPressed = false;
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField(labelText);
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
                string dialoguePath = PathSelectorDialogue();
                this.SelectedPath = string.IsNullOrEmpty(dialoguePath) ? SelectedPath : dialoguePath;
            }

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                string displayFolderPath = string.IsNullOrEmpty(SelectedPath) ? "未選択" : SelectedPath;
                PlateauEditorStyle.MultiLineLabelWithBox(displayFolderPath);
            }

            return this.SelectedPath;
        }

        protected abstract string PathSelectorDialogue();
    }
}
