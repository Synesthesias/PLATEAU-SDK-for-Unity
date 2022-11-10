﻿using System.Collections.Generic;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    internal class AreaSelectorCursor : BoxGizmoDrawer
    {

        private readonly Color cursorColor = Color.blue;
        public const float BoxUpperHeight = 30f; // なんとなく見やすそうな高さ
        public const float BoxBottomHeight = 1f; // カーソルの線が地面と重なって隠れない程度の高さ
        public const float BoxCenterHeight = (BoxUpperHeight + BoxBottomHeight) / 2.0f;
        private const float boxSizeY = BoxUpperHeight - BoxBottomHeight;
        private const float slider2DHandleSize = 0.35f;

        /// <summary>
        /// 引数である候補のうち、カーソルと重なる箇所のあるものをリストで返します。
        /// </summary>
        public List<MeshCodeGizmoDrawer> SelectedMeshCodes(List<MeshCodeGizmoDrawer> candidates)
        {
            var selected = new List<MeshCodeGizmoDrawer>();
            foreach(var candidate in candidates)
            {
                if (IsBoxIntersectXZ(candidate))
                {
                    selected.Add(candidate);
                }
            }
            Debug.Log($"selected count: {selected.Count}/{candidates.Count}, center={this.centerPos}, size={this.size}");

            return selected;
        }

        public Extent GetExtent(int coordinateZoneID, PlateauVector3d referencePoint)
        {
            using var geoReference = new GeoReference(referencePoint, 1.0f, CoordinateSystem.EUN, coordinateZoneID);
            var (min, max) = CalcMinMaxFromTrans();
            // 直交座標を（緯度・経度・高さ）に変換します。
            var geoMin = geoReference.Unproject(min.ToPlateauVector());
            var geoMax = geoReference.Unproject(max.ToPlateauVector());
            var extent = new Extent(geoMin, geoMax);
            return extent;
        }

        private void Init()
        {
            // EnableHandles();
            // var trans = transform;
            // var prevPos = trans.position;
            this.centerPos = new Vector3(0, BoxCenterHeight, 0);
            // trans.position = nextPos;
            
            // var prevLocalScale = trans.lossyScale;
            // var nextLocalScale = new Vector3(prevLocalScale.x, boxSizeY, prevLocalScale.z);
            // trans.localScale = nextLocalScale;
            this.size = new Vector3(1000, boxSizeY, 1000);
        }
        

        // private void OnDestroy()
        // {
        //     DisableHandles();
        // }


        #if UNITY_EDITOR
        

        // protected override void OnSceneGUI(SceneView _)
        // {
        //     var prevColor = Handles.color;
        //     Handles.color = this.cursorColor;
        //     var trans = transform;
        //     
        //     CenterPointHandle(trans);
        //     CornerPointHandle(trans);
        //     
        //     Handles.color = prevColor;
        // }

        public void DrawAndUpdate()
        {
            var prevColor = Handles.color;
            Handles.color = this.cursorColor;

            this.centerPos = CenterPointHandle(this.centerPos);
            CornerPointHandle(this.centerPos, this.size, out this.centerPos, out this.size);

            Handles.color = prevColor;
        }

        private static Vector3 CenterPointHandle(Vector3 centerPos)
        {
            var handlePos = new Vector3(centerPos.x, BoxUpperHeight, centerPos.z);
            EditorGUI.BeginChangeCheck();

            // 中心点ハンドル
            handlePos = Slider2D(handlePos);
            // X軸ハンドル
            handlePos = Handles.Slider(handlePos, Vector3.right);
            // Z軸ハンドル
            handlePos = Handles.Slider(handlePos, Vector3.forward);
            
            if (EditorGUI.EndChangeCheck())
            {
                centerPos = new Vector3(handlePos.x, BoxCenterHeight, handlePos.z);
            }

            return centerPos;
        }

        private static void CornerPointHandle(Vector3 centerPos, Vector3 size, out Vector3 nextCenterPos, out Vector3 nextSize)
        {
            // var pos = trans.position;
            // var size = trans.localScale;
            var prevPosMax = AreaMax(centerPos, size);
            var prevPosMin = AreaMin(centerPos, size);
            
            EditorGUI.BeginChangeCheck();
            
            // 最大点のハンドル
            var nextPosMax = Slider2D(prevPosMax);

            // 最小点のハンドル
            var minHandlePos = new Vector3(prevPosMin.x, BoxUpperHeight, prevPosMin.z);
            var minHandlePosNext = Slider2D(minHandlePos);
            var nextPosMin = new Vector3(minHandlePosNext.x, BoxBottomHeight, minHandlePosNext.z);
            
            // Xが最小、Zが最大の点のハンドル
            var minMaxHandlePos = new Vector3(nextPosMin.x, BoxUpperHeight, nextPosMax.z);
            var minMaxHandlePosNext = Slider2D(minMaxHandlePos);
            nextPosMin = new Vector3(minMaxHandlePosNext.x, nextPosMin.y, nextPosMin.z);
            nextPosMax = new Vector3(nextPosMax.x, nextPosMax.y, minMaxHandlePosNext.z);
            
            // Xが最大、Zが最小の点のハンドル
            var maxMinHandlePos = new Vector3(nextPosMax.x, BoxUpperHeight, nextPosMin.z);
            var maxMinHandlePosNext = Slider2D(maxMinHandlePos);
            nextPosMin = new Vector3(nextPosMin.x, nextPosMin.y, maxMinHandlePosNext.z);
            nextPosMax = new Vector3(maxMinHandlePosNext.x, nextPosMax.y, nextPosMax.z);

            nextCenterPos = centerPos;
            nextSize = size;
            if (EditorGUI.EndChangeCheck())
            {
                (nextPosMin, nextPosMax) = SwapMinMaxIfReversed(nextPosMin, nextPosMax);
                (nextCenterPos, nextSize) = CalcTransFromArea(nextPosMax, nextPosMin); 
            }
        }

        private static Vector3 Slider2D(Vector3 sliderPos)
        {
            return Handles.Slider2D(
                sliderPos, Vector3.up, Vector3.forward,
                Vector3.right, HandleUtility.GetHandleSize(sliderPos) * slider2DHandleSize,
                Handles.SphereHandleCap, 15
            );
        }

        #endif

        private static (Vector3 center, Vector3 size) CalcTransFromArea(Vector3 areaMax, Vector3 areaMin)
        {
            var center = (areaMax + areaMin) / 2.0f;
            var size = areaMax - areaMin;
            return (center, size);
        }

        private (Vector3 min, Vector3 max) CalcMinMaxFromTrans()
        {
            return (this.centerPos - this.size * 0.5f, this.centerPos + this.size * 0.5f);
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
