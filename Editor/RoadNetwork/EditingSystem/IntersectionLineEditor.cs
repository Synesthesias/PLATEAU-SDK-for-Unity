using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util.GeoGraph;
using System;
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
        private RnWay[] shapes;
        private RoadNetworkEditTarget target;
        private RnWay mouseHoveredLine;
        private RnWay selectedLine;
        private LineEditState state = LineEditState.LineNotSelected;
        private SplineCreateHandles splineHandle;

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
            shapes = intersection.Edges.Where(e => e.Road == null).Select(n => n.Border).ToArray();
            
            switch (state)
            {
                case LineEditState.LineNotSelected:
                    DrawIntersectionShapes();
                    bool isNewLineClicked = CheckLineClicked();
                    if (isNewLineClicked)
                    {
                        state = LineEditState.LineSelected;
                        StartEditingLine();
                    }
                    break;
                case LineEditState.LineSelected:
                    DrawLine(selectedLine, new Color(1f, 0.4f, 0.3f));
                    splineHandle.HandleSceneGUI();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
        }

        /// <summary> 線の編集を開始します </summary>
        private void StartEditingLine()
        {
            var spline = new Spline();
            foreach (var point in selectedLine.Points.Select(p => p.Vertex))
            {
                spline.Add(new BezierKnot(point));
            }

            var splineCore = new SplineEditorCore(spline);
            splineHandle = new SplineCreateHandles(splineCore, this);
            splineHandle.BeginCreateSpline(spline.Knots.First().Position);
        }

        /// <summary> 線の編集を確定したとき、それを道路ネットワークに適用します。 </summary>
        public void OnSplineCreated(Spline createdSpline)
        {
            state = LineEditState.LineNotSelected;
            
            var targetIntersection = (target.SelectedRoadNetworkElement as EditorData<RnIntersection>)?.Ref;
            
            // 交差点のEdgeの線に対して適用するのと同じく、対応するSidewalkを探して同じように適用します。
            // Sidewalkへ適用しないと、白線は変わるのに歩道の形は変わらなくなります。
            var correspondSidewalkInside =
                targetIntersection
                    ?.SideWalks
                    .Select(sw => sw.InsideWay)
                    .FirstOrDefault(way => way.IsSameLineSequence(selectedLine));
            
            var positions = createdSpline.Knots.Select(k => new RnPoint(k.Position)).ToArray();
            var line = new RnLineString(positions);
            
            // 交差点のEdgeに線を適用します。
            selectedLine.LineString = line;
            
            // 歩道に線に適用します。
            if (correspondSidewalkInside != null)
            {
                correspondSidewalkInside.LineString = line;
            }
            
            
            var reproduceTarget = new RrTargetRoadBases(target.RoadNetwork, new[]{targetIntersection});
            new RoadReproducer().Generate(reproduceTarget, CrosswalkFrequency.All);
            
            selectedLine = null;
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

        
    }
}