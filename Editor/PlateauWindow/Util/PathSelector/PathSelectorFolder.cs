using UnityEditor;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.Util.PathSelector
{
    internal class PathSelectorFolder : PathSelectorBase
    {
        protected override string PathSelectorDialogue()
        {
            return EditorUtility.OpenFolderPanel("フォルダ選択", "", "");
        }
    }
}
