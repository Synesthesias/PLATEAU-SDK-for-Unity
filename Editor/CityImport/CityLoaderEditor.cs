using System;
using PLATEAU.CityImport.Load;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    [CustomEditor(typeof(CityLoaderBehaviourOLD)), Obsolete]
    public class CityGridLoaderEditorOLD : UnityEditor.Editor
    {
        // public override void OnInspectorGUI()
        // {
        //     var behaviour = (CityLoaderBehaviourOLD)target;
        //     base.OnInspectorGUI();
        //     if (PlateauEditorStyle.MainButton("ロード"))
        //     {
        //         var task = behaviour.LoadAsync();
        //         task.ContinueWithErrorCatch();
        //     }
        // }
    }
}