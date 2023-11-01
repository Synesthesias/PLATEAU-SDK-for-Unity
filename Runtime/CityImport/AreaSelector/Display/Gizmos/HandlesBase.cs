using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos
{
    /// <summary>
    /// シーンビューにハンドルを表示する MonoBehaviour の基底クラスです。
    /// </summary>
    [ExecuteInEditMode]
    public abstract class HandlesBase : MonoBehaviour
    {
        private void OnEnable()
        {
            EnableHandles();
        }

        private void OnDisable()
        {
            DisableHandles();
        }

        private void EnableHandles()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#endif
        }

        private void DisableHandles()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
#endif
        }

        #if UNITY_EDITOR
        protected abstract void OnSceneGUI(SceneView sceneView);
        #endif
    }
}
