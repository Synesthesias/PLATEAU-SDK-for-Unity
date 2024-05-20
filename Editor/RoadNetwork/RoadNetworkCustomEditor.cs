using JetBrains.Annotations;
using PLATEAU.RoadNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace PLATEAU.Editor.RoadNetwork
{
    //[CustomEditor(typeof(PLATEAURoadNetworkTester))/*, CanEditMultipleObjects*/]
    //public class RoadNetworkCustomEditor : UnityEditor.Editor
    //{

    //    public void OnSceneGUI()
    //    {
    //        var hasOpen = RoadNetworkEditorWindow.HasOpenInstances();
    //        if (hasOpen == false)
    //        {
    //            return;
    //        }

    //        var editorInterface = RoadNetworkEditorWindow.GetEditorInterface();
    //        if (editorInterface == null) 
    //            return;

    //        //if (Event.current.type != EventType.Repaint)
    //        //    return;

    //        var guiSystem = editorInterface.SceneGUISystem;
    //        guiSystem.SetEditingTarget(target as PLATEAURoadNetworkTester);
    //        guiSystem.OnSceneGUI();


    //    }

}
