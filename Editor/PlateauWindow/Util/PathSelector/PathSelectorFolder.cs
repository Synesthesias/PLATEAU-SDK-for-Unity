using UnityEditor;

namespace PLATEAU.Editor.PlateauWindow.Util.PathSelector
{
    internal class PathSelectorFolder : PathSelectorBase
    {
        protected override string PathSelectorDialogue()
        {
            return EditorUtility.OpenFolderPanel("フォルダ選択", "", "");
        }
    }
}
