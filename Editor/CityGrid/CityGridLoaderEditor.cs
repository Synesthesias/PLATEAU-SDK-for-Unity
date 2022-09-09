using PLATEAU.CityGrid;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.CityGrid
{
    [CustomEditor(typeof(CityGridLoader))]
    public class CityGridLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var gridLoader = (CityGridLoader)target;
            base.OnInspectorGUI();
            if (PlateauEditorStyle.MainButton("ロード"))
            {
                var task = gridLoader.Load();
                task.ContinueWithErrorCatch();
            }
        }
    }
}