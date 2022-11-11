using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
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

        protected void EnableHandles()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#endif
        }

        protected void DisableHandles()
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
