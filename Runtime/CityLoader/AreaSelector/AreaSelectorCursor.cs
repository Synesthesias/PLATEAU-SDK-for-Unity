using System;
using System.Collections.Specialized;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
#endif

namespace PLATEAU.CityLoader.AreaSelector
{
    [ExecuteInEditMode]
    public class AreaSelectorCursor : MonoBehaviour
    {
        // [SerializeField] private Vector2 areaMin = Vector2.zero;
        // [SerializeField] private Vector2 areaMax = Vector2.one * 100;
        private readonly Color cursorColor = Color.blue;
        private const float boxUpperHeight = 30f;
        private const float boxBottomHeight = 1f; // カーソルの線が地面と重なって隠れない程度の高さ
        private const float boxCenterHeight = (boxUpperHeight + boxBottomHeight) / 2.0f;
        private const float boxSizeY = boxUpperHeight - boxBottomHeight;

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
            var nextPos = new Vector3(prevPos.x, boxCenterHeight, prevPos.z);
            trans.position = nextPos;
            
            var prevLocalScale = trans.lossyScale;
            var nextLocalScale = new Vector3(prevLocalScale.x, boxSizeY, prevLocalScale.z);
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
            var size = trans.localScale;
            Gizmos.DrawWireCube(centerPos, size);
        }

        private void OnSceneGUI(SceneView _)
        {
            var prevColor = Handles.color;
            Handles.color = this.cursorColor;
            var trans = transform;
            
            CenterPointHandle(trans);
            CornerPointHandle(trans);
            
            Handles.color = prevColor;
        }

        private static void CenterPointHandle(Transform trans)
        {
            var pos = trans.position;
            var handlePos = new Vector3(pos.x, boxUpperHeight, pos.z);
            EditorGUI.BeginChangeCheck();

            handlePos = Slider2D(handlePos);
            handlePos = Handles.Slider(handlePos, Vector3.right);
            handlePos = Handles.Slider(handlePos, Vector3.forward);
            
            if (EditorGUI.EndChangeCheck())
            {
                trans.position = new Vector3(handlePos.x, boxCenterHeight, handlePos.z);
            }
        }

        private static void CornerPointHandle(Transform trans)
        {
            var pos = trans.position;
            var size = trans.localScale;
            var prevPosMax = CalcAreaMax(pos, size);
            var prevPosMin = CalcAreaMin(pos, size);
            
            EditorGUI.BeginChangeCheck();
            
            var nextPosMax = Slider2D(prevPosMax);
            var minHandlePos = new Vector3(prevPosMin.x, boxUpperHeight, prevPosMin.z);
            
            
            var minHandlePosNext = Slider2D(minHandlePos);
            var nextPosMin = new Vector3(minHandlePosNext.x, boxBottomHeight, minHandlePosNext.z);
            
            if (EditorGUI.EndChangeCheck())
            {
                (nextPosMin, nextPosMax) = SwapMinMaxIfReversed(nextPosMin, nextPosMax);
                var (nextCenter2, nextSize2) = CalcTransFromArea(nextPosMax, nextPosMin);
                trans.position = nextCenter2;
                trans.localScale = nextSize2;
            }
        }

        private static Vector3 Slider2D(Vector3 sliderPos)
        {
            return Handles.Slider2D(
                sliderPos, Vector3.up, Vector3.forward,
                Vector3.right, HandleUtility.GetHandleSize(sliderPos) * 0.5f,
                Handles.SphereHandleCap, 15
            );
        }

        private static void Slider2DHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            Handles.DotHandleCap(controlID, position, rotation, size, eventType);
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
    
}
