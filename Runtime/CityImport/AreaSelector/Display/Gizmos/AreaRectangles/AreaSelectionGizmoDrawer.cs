using PLATEAU.Dataset;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos.AreaRectangles
{
    /// <summary>
    /// 範囲選択用の箱型ギズモを表示します。
    /// </summary>
    internal sealed class AreaSelectionGizmoDrawer : BoxGizmoDrawer
    {
        private const float CenterPositionY = 15f;
        private const float RayCastMaxDistance = 100000.0f;
        private readonly MeshCode meshCode = new();
        private readonly Color isLeftMouseButtonMovedColor = Color.white;
        private readonly Color isLeftMouseAndShiftButtonMovedColor = Color.red;
        private Vector3 trackingStartedMousePosition;
        public Vector3 AreaSelectionMin { get; private set; }
        public Vector3 AreaSelectionMax { get; private set; }

        public void SetUp()
        {
            this.LineWidth = 2.0f;
        }
        
        public void SetTrackingStartedPosition(Vector2 mousePosition)
        {
            #if UNITY_EDITOR
            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out var hit, RayCastMaxDistance))
            {
                this.trackingStartedMousePosition = hit.point;
            }
            #endif
        }

        public void UpdateAreaSelectionGizmo(Vector2 mousePosition, bool isLeftMouseButtonMoved)
        {
            #if UNITY_EDITOR
            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out var hit, RayCastMaxDistance))
            {
                var isRightSideMousePosition = this.trackingStartedMousePosition.x < hit.point.x;
                double minX = isRightSideMousePosition ? this.trackingStartedMousePosition.x : hit.point.x;
                double maxX = isRightSideMousePosition ? hit.point.x : this.trackingStartedMousePosition.x;

                var isUpperSideMousePosition = this.trackingStartedMousePosition.z < hit.point.z;
                double minZ = isUpperSideMousePosition ? this.trackingStartedMousePosition.z : hit.point.z;
                double maxZ = isUpperSideMousePosition ? hit.point.z : this.trackingStartedMousePosition.z;
            
                var centerPosition = new Vector3((float)(minX + maxX) / 2.0f, CenterPositionY, (float)(minZ + maxZ) / 2.0f);
                var size = new Vector3((int)(maxX - minX), 1, (int)(maxZ - minZ));

                this.BoxColor = isLeftMouseButtonMoved ? this.isLeftMouseButtonMovedColor : this.isLeftMouseAndShiftButtonMovedColor;
                this.AreaSelectionMin = new Vector3((float)minX, CenterPositionY, (float)minZ);
                this.AreaSelectionMax = new Vector3((float)maxX, CenterPositionY, (float)maxZ);
                Init(centerPosition, size, this.meshCode);
            }
            #endif
        }

        public void ClearAreaSelectionGizmo()
        {
            Init(Vector3.zero, Vector3.zero, this.meshCode);
        }
    }
}
