using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.Common.PathSelector
{
    /// <summary>
    /// フォルダ選択ダイアログを使ってパスを選択するGUIです。
    /// </summary>
    internal class PathSelectorFolder : PathSelectorBase
    {
        protected override string PathSelectorDialogue()
        {
            return EditorUtility.OpenFolderPanel("フォルダ選択", "", "");
        }
    }
}
