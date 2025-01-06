using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 交差点の線の形状を編集します。
    /// </summary>
    internal class IntersectionLineEditor
    {
        private SplineEditorCore splineCore;
        // private RnTrack[] tracks;
        private RnWay[] shapes;
        private RoadNetworkEditTarget target;
        private RnWay mouseHoveredLine;
        private RnWay selectedLine;
        private LineEditState state = LineEditState.LineNotSelected;

        private enum LineEditState
        {
            LineNotSelected, LineSelected
        }

        public IntersectionLineEditor(RoadNetworkEditTarget target)
        {
            this.target = target;
            // tracks = intersection.Tracks.ToArray();
            
        }
        
        
        public void Draw()
        {
            var intersectionEdit = target.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            var intersection = intersectionEdit?.Ref;
            if (intersection == null) return;
            
            shapes = intersection.Edges.Where(e => e.Road == null).Select(n => n.Border).ToArray();
            switch (state)
            {
                case LineEditState.LineNotSelected:
                    DrawIntersectionShapes();
                    bool isNewLineClicked = CheckLineClicked();
                    if (isNewLineClicked) state = LineEditState.LineSelected;
                    break;
                case LineEditState.LineSelected:
                    DrawLine(selectedLine, new Color(1f, 0.4f, 0.3f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
        }

        private void DrawIntersectionShapes()
        {
            if (shapes == null)
            {
                Debug.Log("shapes is null.");
                return;
            }

            mouseHoveredLine = GetMouseHoveredLine();
            foreach (var shape in shapes)
            {
                var color = shape == mouseHoveredLine ? Color.red : Color.cyan;
                DrawLine(shape, color);
            }
        }

        private void DrawLine(RnWay shape, Color color)
        {
            var drawer = new LaneLineDrawerSolid(shape.ToList(), color, LaneLineDrawMethod.Handles);
            drawer.Draw();
        }

        private bool CheckLineClicked()
        {
            if (mouseHoveredLine == null) return false;
            if (!LineUtil.IsMouseDown()) return false;
            bool isNewLineClicked = selectedLine != mouseHoveredLine;
            selectedLine = mouseHoveredLine;
            return isNewLineClicked;
        }

        private RnWay GetMouseHoveredLine()
        {
            var mouseScreenPos = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(mouseScreenPos);
            float minDist = float.MaxValue;
            RnWay nearestShape = null;
            foreach (var shape in shapes)
            {
                for (int i = 0; i < shape.Count - 1; i++)
                {
                    var p1 = shape[i];
                    var p2 = shape[i + 1];
                    var distance = LineUtil.CheckHit(new LineUtil.Line(p1, p2), 2f, ray, out var closestPoint,
                        out var closestPoint2);
                    if (distance >= 0f && distance < minDist)
                    {
                        minDist = distance;
                        nearestShape = shape;
                    }
                }
            }

            return nearestShape;
        }

        /// <summary> pからLineまでの距離 </summary>
        private (float distance, Vector3 neighborPoint) DistanceFromPointToLine(Vector3 p, RnLineString line)
        {
            if (line.Count == 0) return (0, p);
            if (line.Count == 1) return ((p - line[0]).magnitude, line[0]);
            float minDist = float.MaxValue;
            Vector3 nearestNeighbor = Vector3.zero;
            for(int i=0; i<line.Count-1; i++)
            {
                var (dist, neighbor) = DistanceFromPointToLineSegment(p, line[i], line[i+1]);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestNeighbor = neighbor;
                }
            }

            return (minDist, nearestNeighbor);
        }
        
        /// <summary> 点pから線分abまでの距離 </summary>
        private (float distance, Vector3 neighborPoint) DistanceFromPointToLineSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            // 参考 : https://qiita.com/deltaMASH/items/e7ffcca78c9b75710d09
            var ap = p - a;
            var ab = b - a;
            var ba = a - b;
            var bp = p - b;
            if (Vector3.Dot(ap, ab) < 0) return (ap.magnitude, a);
            if (Vector3.Dot(bp, ba) < 0) return (bp.magnitude, b);
            var aiNorm = Vector3.Dot(ap, ab) / ab.magnitude;
            var neighbor = a + ab / ab.magnitude * aiNorm;
            var dist = (p - neighbor).magnitude;
            return (dist, neighbor);
        }
        
        

        // private void SetTargetLine()
        // {
            // splineCore = new SplineEditorCore();
        // }
        
    }
}