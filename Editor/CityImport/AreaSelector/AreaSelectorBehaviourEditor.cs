// using PLATEAU.CityImport.AreaSelector;
// using UnityEditor;
// using UnityEngine;
//
// namespace PLATEAU.Editor.CityImport.AreaSelector
// {
//     [CustomEditor(typeof(AreaSelectorBehaviour))]
//     public class AreaSelectorBehaviourEditor : UnityEditor.Editor
//     {
//         private void OnSceneGUI()
//         {
//             RotateCameraDown();
//         }
//         
//         private void RotateCameraDown()
//         {
//             Debug.Log("rotateCameraDown");
//             var cam = SceneView.currentDrawingSceneView.camera;
//             cam.transform.eulerAngles = new Vector3(-90, 0, 0);
//         }
//     }
// }
