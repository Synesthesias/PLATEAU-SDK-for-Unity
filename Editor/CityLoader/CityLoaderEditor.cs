using System;
using PLATEAU.CityLoader.Load;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.CityLoader
{
    [CustomEditor(typeof(CityLoaderBehaviourOLD)), Obsolete]
    public class CityGridLoaderEditorOLD : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = (CityLoaderBehaviourOLD)target;
            base.OnInspectorGUI();
            if (PlateauEditorStyle.MainButton("ロード"))
            {
                var task = behaviour.LoadAsync();
                task.ContinueWithErrorCatch();
            }
        }
    }
}