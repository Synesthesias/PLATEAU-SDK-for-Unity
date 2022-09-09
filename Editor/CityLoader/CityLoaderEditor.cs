using PLATEAU.CityLoader.Load;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.CityLoader
{
    [CustomEditor(typeof(CityLoaderBehaviour))]
    public class CityGridLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = (CityLoaderBehaviour)target;
            base.OnInspectorGUI();
            if (PlateauEditorStyle.MainButton("ロード"))
            {
                var task = behaviour.LoadAsync();
                task.ContinueWithErrorCatch();
            }
        }
    }
}