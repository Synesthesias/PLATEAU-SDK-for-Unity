using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 交差点の線の形状を編集します。
    /// </summary>
    internal class IntersectionLineEditor : ICreatedSplineReceiver
    {
        private RoadNetworkEditTarget target;
        private IEditTargetLine mouseHoveredLine;
        private IEditTargetLine selectedLine;
        private LineEditState state = LineEditState.LineNotSelected;
        private SplineEditorCore splineCore;

        private enum LineEditState
        {
            LineNotSelected, LineSelected
        }

        public IntersectionLineEditor(RoadNetworkEditTarget target)
        {
            this.target = target;
        }
        
        /// <summary> 毎フレームのシーンビューへの描画 </summary>
        public void Draw()
        {
            var intersectionEdit = target.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            var intersection = intersectionEdit?.Ref;
            if (intersection == null) return;
            
            
            switch (state)
            {
                case LineEditState.LineNotSelected:
                    var lines = IEditTargetLine.ComposeFrom(intersection);
                    DrawIntersectionShapes(lines);
                    bool isNewLineClicked = CheckLineClicked();
                    if (isNewLineClicked)
                    {
                        state = LineEditState.LineSelected;
                        StartEditingLine();
                    }
                    break;
                case LineEditState.LineSelected:
                    DrawPreviewLine();
                    SplineEditorHandles.HandleSceneGUI(splineCore);
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        OnSplineCreated(splineCore.Spline);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
        }

        /// <summary> 線の編集を開始します </summary>
        private void StartEditingLine()
        {
            var spline = new Spline();
            foreach (var point in selectedLine.Line)
            {
                spline.Add(new BezierKnot(point));
            }

            splineCore = new SplineEditorCore(spline);
        }

        /// <summary> 線の編集を確定したとき、それを道路ネットワークに適用します。 </summary>
        public void OnSplineCreated(Spline createdSpline)
        {
            state = LineEditState.LineNotSelected;
            
            var targetIntersection = (target.SelectedRoadNetworkElement as EditorData<RnIntersection>)?.Ref;

            selectedLine.Apply(targetIntersection, createdSpline);
            
            var reproduceTarget = new RrTargetRoadBases(target.RoadNetwork, new[]{targetIntersection});
            new RoadReproducer().Generate(reproduceTarget, CrosswalkFrequency.All);
            
            selectedLine = null;
        }
        

        private void DrawIntersectionShapes(IEditTargetLine[] lines)
        {
            mouseHoveredLine = GetMouseHoveredLine(lines);
            foreach (var line in lines)
            {
                bool isMouseHovered = line == mouseHoveredLine;
                if (!isMouseHovered && line is EditTargetTrack)
                {
                    // マウスホバーでないトラックの線は、ここではなくIntersectionTrackEditorで描画します。
                    continue;
                }
                var color = isMouseHovered ? Color.red : Color.cyan;
                
                // マウスホバーの線が他の線と埋もれないように少し位置を上げます。
                var drawingLine = isMouseHovered ? line.Line.Select(l => l + Vector3.up * 0.01f) : line.Line;
                DrawLine(drawingLine.ToArray(), color);
            }
        }

        private void DrawLine(Vector3[] line, Color color)
        {
            var drawer = new LaneLineDrawerSolid(line.ToList(), color, LaneLineDrawMethod.Handles);
            drawer.Draw();
        }

        private void DrawPreviewLine()
        {
            var line = splineCore.Spline.Knots.Select(k => k.Position).Select(f => new Vector3(f.x, f.y, f.z)).ToList();
            var drawer = new LaneLineDrawerSolid(line, new Color(1f, 0.4f, 0.3f), LaneLineDrawMethod.Handles);
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

        private IEditTargetLine GetMouseHoveredLine(IEditTargetLine[] lines)
        {
            var mouseScreenPos = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(mouseScreenPos);
            float minDist = float.MaxValue;
            IEditTargetLine nearestLine = null;
            foreach (var line in lines)
            {
                var positions = line.Line;
                for (int i = 0; i < positions.Length - 1; i++)
                {
                    var p1 = positions[i];
                    var p2 = positions[i + 1];
                    var distance = LineUtil.CheckHit(new LineUtil.Line(p1, p2), 2f, ray, out var closestPoint,
                        out var closestPoint2);
                    if (distance >= 0f && distance < minDist)
                    {
                        minDist = distance;
                        nearestLine = line;
                    }
                }
            }

            return nearestLine;
        }

        private interface IEditTargetLine
        {
            public void Apply(RnIntersection intersection, Spline createdSpline);

            public Vector3[] Line { get;}

            public static IEditTargetLine[] ComposeFrom(RnIntersection intersection)
            {
                var shapes = intersection
                    .Edges
                    .Where(e => e.Road == null)
                    .Select(n => n.Border)
                    .Select(b => new EditTargetShape(b));
                var tracks = intersection
                    .Tracks
                    .Select(t => new EditTargetTrack(t));
                return shapes.Cast<IEditTargetLine>().Concat(tracks).ToArray();
            }
        }

        private class EditTargetTrack : IEditTargetLine
        {
            public RnTrack Track { get; set; }
            
            public EditTargetTrack(RnTrack track)
            {
                Track = track;
            }

            public void Apply(RnIntersection intersection, Spline createdSpline)
            {
                Track.Spline = createdSpline;
            }
            
            public Vector3[] Line
            {
                get => Track.Spline.Knots.Select(k => k.Position).Select(f => new Vector3(f.x, f.y, f.z)).ToArray();
            }
        }

        /// <summary> 交差点で編集対象となる線 </summary>
        private class EditTargetShape : IEditTargetLine
        {
            private RnWay Way { get; set; }
            
            public EditTargetShape(RnWay way)
            {
                Way = way;
            }


            /// <summary> 編集した線を道路ネットワークに適用します。 </summary>
            public void Apply(RnIntersection intersection, Spline createdSpline)
            {
                // 交差点のEdgeの線に対して適用するのと同じく、対応するSidewalkを探して同じように適用します。
                // Sidewalkへ適用しないと、白線は変わるのに歩道の形は変わらなくなります。
                var correspondSidewalkInside =
                    intersection
                        ?.SideWalks
                        .Select(sw => sw.InsideWay)
                        .FirstOrDefault(way => way.IsSameLineSequence(way));

                var positions = createdSpline.Knots.Select(k => new RnPoint(k.Position)).ToArray();
                var line = new RnLineString(positions);

                // 交差点のEdgeに線を適用します。
                Way.LineString = line;

                // 歩道に線に適用します。
                if (correspondSidewalkInside != null)
                {
                    correspondSidewalkInside.LineString = line;
                }
            }
            
            public Vector3[] Line{
                get => Way.LineString.Points.Select(p => p.Vertex).Select(v => new Vector3(v.x, v.y, v.z)).ToArray();
            }
        }
    }
}
