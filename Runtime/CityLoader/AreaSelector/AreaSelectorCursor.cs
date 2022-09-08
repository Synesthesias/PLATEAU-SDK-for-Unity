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

        private readonly Color cursorColor = Color.blue;
        private const float boxUpperHeight = 30f; // なんとなく見やすそうな高さ
        private const float boxBottomHeight = 1f; // カーソルの線が地面と重なって隠れない程度の高さ
        private const float boxCenterHeight = (boxUpperHeight + boxBottomHeight) / 2.0f;
        private const float boxSizeY = boxUpperHeight - boxBottomHeight;

        private void Start()
        {
            EnableHandles();
            var trans = transform;
            var prevPos = trans.position;
            var nextPos = new Vector3(prevPos.x, boxCenterHeight, prevPos.z);
            trans.position = nextPos;
            
            var prevLocalScale = trans.lossyScale;
            var nextLocalScale = new Vector3(prevLocalScale.x, boxSizeY, prevLocalScale.z);
            trans.localScale = nextLocalScale;
        }

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

        private void OnDestroy()
        {
            DisableHandles();
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

            // 中心点ハンドル
            handlePos = Slider2D(handlePos);
            // X軸ハンドル
            handlePos = Handles.Slider(handlePos, Vector3.right);
            // Z軸ハンドル
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
            
            // 最大点のハンドル
            var nextPosMax = Slider2D(prevPosMax);

            // 最小点のハンドル
            var minHandlePos = new Vector3(prevPosMin.x, boxUpperHeight, prevPosMin.z);
            var minHandlePosNext = Slider2D(minHandlePos);
            var nextPosMin = new Vector3(minHandlePosNext.x, boxBottomHeight, minHandlePosNext.z);
            
            // Xが最小、Zが最大の点のハンドル
            var minMaxHandlePos = new Vector3(nextPosMin.x, boxUpperHeight, nextPosMax.z);
            var minMaxHandlePosNext = Slider2D(minMaxHandlePos);
            nextPosMin = new Vector3(minMaxHandlePosNext.x, nextPosMin.y, nextPosMin.z);
            nextPosMax = new Vector3(nextPosMax.x, nextPosMax.y, minMaxHandlePosNext.z);
            
            // Xが最大、Zが最小の点のハンドル
            var maxMinHandlePos = new Vector3(nextPosMax.x, boxUpperHeight, nextPosMin.z);
            var maxMinHandlePosNext = Slider2D(maxMinHandlePos);
            nextPosMin = new Vector3(nextPosMin.x, nextPosMin.y, maxMinHandlePosNext.z);
            nextPosMax = new Vector3(maxMinHandlePosNext.x, nextPosMax.y, nextPosMax.z);
            
            
            if (EditorGUI.EndChangeCheck())
            {
                (nextPosMin, nextPosMax) = SwapMinMaxIfReversed(nextPosMin, nextPosMax);
                var (nextCenter, nextSize) = CalcTransFromArea(nextPosMax, nextPosMin);
                trans.position = nextCenter;
                trans.localScale = nextSize;
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
