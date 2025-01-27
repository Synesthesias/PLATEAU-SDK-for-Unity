using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod{

    /// <summary>
    /// 線が複数あり、その中から線を1つ選択して編集します。
    /// </summary>
    internal class LineSelectEdit
    {
        private LineEditState state = LineEditState.LineNotSelected;
        private IEditTargetLine mouseHoveredLine;
        private IEditTargetLine selectedLine;
        private SplineEditorHandles splineEditorHandles; // スプライン編集のUI
        private ICreatedLineReceiver createdLineReceiver;
        
        
        private enum LineEditState
        {
            LineNotSelected, LineSelected
        }

        public LineSelectEdit(ICreatedLineReceiver createdLineReceiver)
        {
            this.createdLineReceiver = createdLineReceiver;
        }

        public void Draw(IEditTargetLine[] lines)
        {
            switch (state)
            {
                case LineEditState.LineNotSelected:
                    
                    DrawLines(lines);
                    bool isNewLineClicked = CheckLineClicked();
                    if (isNewLineClicked)
                    {
                        state = LineEditState.LineSelected;
                        StartEditingLine();
                    }

                    break;
                case LineEditState.LineSelected:
                    DrawPreviewLine();
                    splineEditorHandles.HandleSceneGUI();

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

            var splineCore = new SplineEditorCore(spline);
            splineEditorHandles = new SplineEditorHandles(splineCore, () =>
                {
                    OnSplineCreated(splineEditorHandles.Core.Spline);
                },
                OnLineEditCanceled
            );
        }
        
        private void DrawLines(IEditTargetLine[] lines)
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
                var drawingLine = isMouseHovered ? line.Line.Select(l => l + Vector3.up * 0.05f) : line.Line;
                DrawLine(drawingLine.ToArray(), color);
            }
        }
        
        private void DrawPreviewLine()
        {
            var line = splineEditorHandles.Core.Spline.Knots.Select(k => k.Position).Select(f => new Vector3(f.x, f.y, f.z)).ToList();
            var drawer = new LaneLineDrawerSolid(line, new Color(1f, 0.4f, 0.3f), LaneLineDrawMethod.Handles);
            drawer.Draw();
        }
        
        private void DrawLine(Vector3[] line, Color color)
        {
            var drawer = new LaneLineDrawerSolid(line.ToList(), color, LaneLineDrawMethod.Handles);
            drawer.Draw();
        }

        public void OnSplineCreated(Spline createdSpline)
        {
            state = LineEditState.LineNotSelected;
            createdLineReceiver.OnLineCreated(createdSpline, selectedLine);
            selectedLine = null;
        }

        private void OnLineEditCanceled()
        {
            state = LineEditState.LineNotSelected;
            selectedLine = null;
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
        
        private bool CheckLineClicked()
        {
            if (mouseHoveredLine == null) return false;
            if (!LineUtil.IsMouseDown()) return false;
            bool isNewLineClicked = selectedLine != mouseHoveredLine;
            selectedLine = mouseHoveredLine;
            return isNewLineClicked;
        }

        public interface ICreatedLineReceiver
        {
            public void OnLineCreated(Spline createdSpline, IEditTargetLine targetLine);
        }
            
    }
    
    /// <summary>
    /// 線の編集において、編集対象の線です。
    /// 線の編集を確定したときの処理をサブクラスで書き分けます。
    /// </summary>
    internal interface IEditTargetLine
    {
        public void Apply(RnRoadBase roadBase, Spline createdSpline);

        public Vector3[] Line { get; }

        public static IEditTargetLine[] ComposeFromIntersection(RnIntersection intersection)
        {
            var shapes = intersection
                .Edges
                .Where(e => e.Road == null)
                .Select(n => n.Border)
                .Select(b => new EditTargetIntersectionShape(b));
            var tracks = intersection
                .Tracks
                .Select(t => new EditTargetTrack(t));
            return shapes.Cast<IEditTargetLine>().Concat(tracks).ToArray();
        }
    }
}