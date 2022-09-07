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
        // [SerializeField] private Vector2 areaMin = Vector2.zero;
        // [SerializeField] private Vector2 areaMax = Vector2.one * 100;
        private readonly Color cursorColor = Color.blue;
        private const float boxUpperPos = 100f;

        // public Vector2 CenterPos
        // {
        //     get => (this.areaMin + this.areaMax) / 2.0f;
        //     set
        //     {
        //         var diff = value - CenterPos;
        //         this.areaMin += diff / 2.0f;
        //         this.areaMax += diff / 2.0f;
        //     }
        // }

        // public Vector2 Size
        // {
        //     get => this.areaMax - this.areaMin;
        // }

        private void Start()
        {
            #if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            #endif
            var trans = transform;
            var prevPos = trans.position;
            var nextPos = new Vector3(prevPos.x, boxUpperPos / 2.0f, prevPos.z);
            trans.position = nextPos;
            
            var prevLocalScale = trans.lossyScale;
            var nextLocalScale = new Vector3(prevLocalScale.x, boxUpperPos, prevLocalScale.z);
            trans.localScale = nextLocalScale;
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
            var trans = transform;
            var centerPos = trans.position;
            var size = trans.lossyScale;
            // Vector3 centerPos3D = new Vector3(centerPos.x, boxUpperPos/2.0f, centerPos.y);
            // Vector3 size3D = new Vector3(size.x, boxUpperPos, size.z);
            Gizmos.DrawWireCube(centerPos, size);
        }

        private void OnSceneGUI(SceneView _)
        {
            var prevColor = Handles.color;
            Handles.color = this.cursorColor;
            // Vector3 nextCenter3D = new Vector3(CenterPos.x, boxUpperPos, CenterPos.y);
            var trans = transform;
            var pos = trans.position;
            var nextCenter = new Vector3(pos.x, boxUpperPos, pos.z);
            
            EditorGUI.BeginChangeCheck();
            
            nextCenter = Handles.Slider2D(nextCenter, Vector3.up, Vector3.forward, Vector3.right,
                HandleUtility.GetHandleSize(nextCenter), Handles.CircleHandleCap, 15);
        
            nextCenter = Handles.Slider(nextCenter, Vector3.right);
            nextCenter = Handles.Slider(nextCenter, Vector3.forward);
            
            if (EditorGUI.EndChangeCheck())
            {
                trans.position = nextCenter;
            }
            
            EditorGUI.BeginChangeCheck();
            var size = trans.localScale;
            var prevPosMax = CalcAreaMax(pos, size);
            var nextPosMax = Handles.Slider2D(prevPosMax, Vector3.up, Vector3.forward, Vector3.right, HandleUtility.GetHandleSize(prevPosMax),
                Handles.CircleHandleCap, 15);
            var prevPosMin = CalcAreaMin(pos, size);
            var minHandlePos = new Vector3(prevPosMin.x, boxUpperPos, prevPosMin.z);
            var minHandlePosNext = Handles.Slider2D(minHandlePos, Vector3.up,Vector3.forward, Vector3.right, HandleUtility.GetHandleSize(prevPosMax),
                Handles.CircleHandleCap, 15);
            var nextPosMin = new Vector3(minHandlePosNext.x, 0, minHandlePosNext.z);
            if (EditorGUI.EndChangeCheck())
            {
                (nextPosMin, nextPosMax) = SwapMinMaxIfReversed(nextPosMin, nextPosMax);
                var (nextCenter2, nextSize2) = CalcTransFromArea(nextPosMax, nextPosMin);
                trans.position = nextCenter2;
                trans.localScale = nextSize2;
            }
            
            
            
            Handles.color = prevColor;
        }
        #endif

        private static Vector3 CalcAreaMax(Vector3 center, Vector3 size)
        {
            return center + size / 2.0f;
        }

        private static Vector3 CalcAreaMin(Vector3 center, Vector3 size)
        {
            return center - size / 2.0f;
        }

        private static (Vector3 center, Vector3 size) CalcTransFromArea(Vector3 areaMax, Vector3 areaMin)
        {
            var center = (areaMax + areaMin) / 2.0f;
            var size = areaMax - areaMin;
            return (center, size);
        }

        private static (Vector3 min, Vector3 max) SwapMinMaxIfReversed(Vector3 min, Vector3 max)
        {
            var minX = min.x;
            var minY = min.y;
            var minZ = min.z;
            var maxX = max.x;
            var maxY = max.y;
            var maxZ = max.z;
            if (minX > maxX) (minX, maxX) = (maxX, minX);
            if (minY > maxY) (minY, maxY) = (maxY, minY);
            if (minZ > maxZ) (minZ, maxZ) = (maxZ, minZ);
            return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }
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
