using System;
using System.Collections.Specialized;
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
        [SerializeField] private Vector2 areaMin = Vector2.zero;
        [SerializeField] private Vector2 areaMax = Vector2.one * 100;
        private readonly Color cursorColor = Color.blue;
        private const float boxUpperPos = 100f;

        public Vector2 CenterPos
        {
            get => (this.areaMin + this.areaMax) / 2.0f;
            set
            {
                var diff = value - CenterPos;
                this.areaMin += diff / 2.0f;
                this.areaMax += diff / 2.0f;
            }
        }

        public Vector2 Size
        {
            get => this.areaMax - this.areaMin;
        }

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

        private void OnDrawGizmos()
        {
            Vector3 centerPos3D = new Vector3(CenterPos.x, boxUpperPos/2.0f, CenterPos.y);
            Vector3 size3D = new Vector3(Size.x, boxUpperPos, Size.y);
            Gizmos.DrawWireCube(centerPos3D, size3D);
        }

        private void OnSceneGUI(SceneView _)
        {
            var prevColor = Handles.color;
            Handles.color = this.cursorColor;
            Vector3 nextCenter3D = new Vector3(CenterPos.x, boxUpperPos, CenterPos.y);
            
            
            EditorGUI.BeginChangeCheck();
            
            nextCenter3D = Handles.Slider2D(nextCenter3D, Vector3.up, Vector3.forward, Vector3.right,
                HandleUtility.GetHandleSize(nextCenter3D), Handles.CircleHandleCap, 15);
        
            nextCenter3D = Handles.Slider(nextCenter3D, Vector3.right);
            nextCenter3D = Handles.Slider(nextCenter3D, Vector3.forward);
            
            if (EditorGUI.EndChangeCheck())
            {
                CenterPos = new Vector2(nextCenter3D.x, nextCenter3D.z);
            }
            Handles.color = prevColor;
        }
        #endif
    }

    // #if UNITY_EDITOR
    // [CustomEditor(typeof(AreaSelectorCursor))]
    // public class AreaSelectorCursorEditor : Editor
    // {
    //     private readonly Color cursorColor = Color.blue;
    //     private void OnSceneGUI()
    //     {
    //         var cursor = (AreaSelectorCursor)target;
    //         var prevColor = Handles.color;
    //         Handles.color = this.cursorColor;
    //         
    //         EditorGUI.BeginChangeCheck();
    //         
    //         cursor.cursorPos = Handles.Slider2D(cursor.cursorPos, Vector3.up, Vector3.forward, Vector3.right,
    //             HandleUtility.GetHandleSize(cursor.cursorPos), Handles.CircleHandleCap, 15);
    //
    //         cursor.cursorPos = Handles.Slider(cursor.cursorPos, Vector3.right);
    //
    //         if (EditorGUI.EndChangeCheck())
    //         {
    //             Debug.Log("slider dragged.");
    //         }
    //         Handles.color = prevColor;
    //     }
    // }
    // #endif
}
