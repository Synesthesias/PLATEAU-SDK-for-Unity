using System.Collections.Generic;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 範囲選択画面のカーソルを描画します。
    /// </summary>
    internal class AreaSelectorCursor : BoxGizmoDrawer
    {
        public const float BoxY = 15f;
        private const float slider2DHandleSizeWorld = 300f;
        private const float handleSizeMinScreenSpace = 15f;
        private const float handleSizeMaxScreenSpace = 30f;
        private const float lineWidth = 2;
        private static readonly Color handleColor = new Color(1f, 72f / 255f, 0f);
        private static readonly Color selectOutlineColor = new Color(1f, 72f / 255f, 0f);
        private static readonly Color selectFaceColor = new Color(250f / 255, 160f / 255f, 75f / 255f, 0.3f);
        private static readonly Color transparentColor = new Color(0f, 0f, 0f, 0f);

        public AreaSelectorCursor()
        {
            this.CenterPos = new Vector3(0, BoxY, 0);
            this.Size = new Vector3(1000, 1, 1000);
            LineWidth = lineWidth;
            BoxColor = selectOutlineColor;
        }

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
            return selected;
        }

        /// <summary>
        /// カーソルで選択中の緯度経度の範囲を返します。
        /// </summary>
        public Extent GetExtent(int coordinateZoneID, PlateauVector3d referencePoint)
        {
            using var geoReference = GeoReference.Create(referencePoint, 1.0f, CoordinateSystem.EUN, coordinateZoneID);
            var (min, max) = CalcMinMaxFromTrans();
            // 直交座標を（緯度・経度・高さ）に変換します。
            var geoMin = geoReference.Unproject(min.ToPlateauVector());
            var geoMax = geoReference.Unproject(max.ToPlateauVector());
            var extent = new Extent(geoMin, geoMax);
            return extent;
        }

#if UNITY_EDITOR
        

        /// <summary>
        /// ハンドルと選択範囲を描画します。
        /// </summary>
        public override void DrawSceneGUI()
        {
            var prevColor = Handles.color;
            Handles.color = handleColor;

            // 中央と四隅のドラッグ可能なハンドルを描画します。
            this.CenterPos = CenterPointHandle(this.CenterPos);
            CornerPointHandle(this.CenterPos, this.Size, out this.CenterPos, out this.Size);
            
            // 範囲内を半透明の四角で塗りつぶします。
            var min = AreaMin;
            var max = AreaMax;
            var rectVerts = new []
            {
                min, new Vector3(min.x, min.y, max.z),
                max, new Vector3(max.x, max.y, min.z)
            };
            // 四角の中を塗りつぶしますが、輪郭線は完全に透明にします。なぜなら、四角の輪郭線の表示は親クラスが行うためです。
            Handles.DrawSolidRectangleWithOutline(rectVerts, selectFaceColor, transparentColor);

            Handles.color = prevColor;
        }

        /// <summary>
        /// 選択範囲中心の、ドラッグ可能なハンドルを描画します。
        /// </summary>
        private static Vector3 CenterPointHandle(Vector3 centerPos)
        {
            var handlePos = new Vector3(centerPos.x, BoxY, centerPos.z);
            EditorGUI.BeginChangeCheck();

            // 中心点ハンドル
            handlePos = Slider2D(handlePos);
            // X軸ハンドル
            handlePos = Handles.Slider(handlePos, Vector3.right);
            // Z軸ハンドル
            handlePos = Handles.Slider(handlePos, Vector3.forward);
            
            if (EditorGUI.EndChangeCheck())
            {
                centerPos = new Vector3(handlePos.x, BoxY, handlePos.z);
            }

            return centerPos;
        }

        /// <summary>
        /// 選択範囲の四隅の、ドラッグ可能なハンドルを描画します。
        /// </summary>
        private static void CornerPointHandle(Vector3 centerPos, Vector3 size, out Vector3 nextCenterPos, out Vector3 nextSize)
        {
            var prevPosMax = CalcAreaMax(centerPos, size);
            var prevPosMin = CalcAreaMin(centerPos, size);
            
            EditorGUI.BeginChangeCheck();
            
            // 最大点のハンドル
            var nextPosMax = Slider2D(prevPosMax);

            // 最小点のハンドル
            var minHandlePos = new Vector3(prevPosMin.x, BoxY, prevPosMin.z);
            var nextPosMin = Slider2D(minHandlePos);
            
            // Xが最小、Zが最大の点のハンドル
            var minMaxHandlePos = new Vector3(nextPosMin.x, BoxY, nextPosMax.z);
            var minMaxHandlePosNext = Slider2D(minMaxHandlePos);
            nextPosMin = new Vector3(minMaxHandlePosNext.x, BoxY, nextPosMin.z);
            nextPosMax = new Vector3(nextPosMax.x, BoxY, minMaxHandlePosNext.z);
            
            // Xが最大、Zが最小の点のハンドル
            var maxMinHandlePos = new Vector3(nextPosMax.x, BoxY, nextPosMin.z);
            var maxMinHandlePosNext = Slider2D(maxMinHandlePos);
            nextPosMin = new Vector3(nextPosMin.x, BoxY, maxMinHandlePosNext.z);
            nextPosMax = new Vector3(maxMinHandlePosNext.x, BoxY, nextPosMax.z);

            nextCenterPos = centerPos;
            nextSize = size;
            if (EditorGUI.EndChangeCheck())
            {
                (nextPosMin, nextPosMax) = SwapMinMaxIfReversed(nextPosMin, nextPosMax);
                (nextCenterPos, nextSize) = CalcTransFromArea(nextPosMax, nextPosMin); 
            }
        }

        /// <summary>
        /// ドラッグ可能な丸を描画します。
        /// </summary>
        private static Vector3 Slider2D(Vector3 sliderPos)
        {
            // 丸の大きさを決めます。
            // 基本はワールド座標で slider2DHandleSizeWorld の大きさにします。
            // ただし、スクリーン座標で最小・最大の範囲外であれば、その範囲内に収めます。
            var cam = Camera.current;
            float distance = Vector3.Distance(sliderPos, cam.transform.position);
            float meterPerPixel = Vector3.Distance(
                                      cam.ViewportToWorldPoint(new Vector3(1, 0.5f, distance)),
                                      cam.ViewportToWorldPoint(new Vector3(0, 0.5f, distance))
                                  )
                                  / cam.pixelWidth;
            float handleSizeScreenSpace = slider2DHandleSizeWorld / meterPerPixel;
            float clampedSizeScreenSpace =
                Mathf.Clamp(handleSizeScreenSpace, handleSizeMinScreenSpace, handleSizeMaxScreenSpace);
            float handleSizeWorldSpace = clampedSizeScreenSpace * meterPerPixel;
            
            // 丸を描画します。
            return Handles.Slider2D(
                sliderPos, Vector3.up, Vector3.forward,
                Vector3.right, handleSizeWorldSpace,
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
            return (this.CenterPos - this.Size * 0.5f, this.CenterPos + this.Size * 0.5f);
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
