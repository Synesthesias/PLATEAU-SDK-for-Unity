using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityLoader.AreaSelector
{
    [ExecuteInEditMode]
    public class AreaSelectorCursor : MonoBehaviour
    {
        public Vector3 cursorPos = Vector3.zero;
        private readonly Color cursorColor = Color.blue;

        private void Start()
        {
            #if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            #endif
        }

        private void OnDestroy()
        {
            #if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
            #endif
        }


        #if UNITY_EDITOR
        private void OnSceneGUI(SceneView _)
        {
            var prevColor = Handles.color;
            Handles.color = this.cursorColor;
            
            EditorGUI.BeginChangeCheck();
            
            this.cursorPos = Handles.Slider2D(this.cursorPos, Vector3.up, Vector3.forward, Vector3.right,
                HandleUtility.GetHandleSize(this.cursorPos), Handles.CircleHandleCap, 15);
        
            this.cursorPos = Handles.Slider(this.cursorPos, Vector3.right);
        
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("slider dragged.");
            }
            Handles.color = prevColor;
        }
        #endif
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(AreaSelectorCursor))]
    public class AreaSelectorCursorEditor : Editor
    {
        private readonly Color cursorColor = Color.blue;
        private void OnSceneGUI()
        {
            var cursor = (AreaSelectorCursor)target;
            var prevColor = Handles.color;
            Handles.color = this.cursorColor;
            
            EditorGUI.BeginChangeCheck();
            
            cursor.cursorPos = Handles.Slider2D(cursor.cursorPos, Vector3.up, Vector3.forward, Vector3.right,
                HandleUtility.GetHandleSize(cursor.cursorPos), Handles.CircleHandleCap, 15);
    
            cursor.cursorPos = Handles.Slider(cursor.cursorPos, Vector3.right);
    
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("slider dragged.");
            }
            Handles.color = prevColor;
        }
    }
    #endif
}
