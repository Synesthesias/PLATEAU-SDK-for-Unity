using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Tester
{
    public class PLATEAUGeoGraphTesterLineString : MonoBehaviour
    {
        public bool visible = true;
        public bool visibleNormal = false;
        public Color color = Color.white;
        public AxisPlane axis = AxisPlane.Xy;
        [Serializable]
        public class EdgeBorderTestParam
        {
            public bool enable = false;
            public float allowAngle = 20f;
            public float skipAngle = 20f;
            public GeoGraph2D.DebugFindOppositeOption op = new GeoGraph2D.DebugFindOppositeOption();
        }
        public EdgeBorderTestParam edgeBorderTest = new EdgeBorderTestParam();

        [Serializable]
        public class TunTypeTestParam
        {
            public bool enable = false;
            public RnTurnType turnType;
        }
        public TunTypeTestParam turnTypeTest = new TunTypeTestParam();

        [Serializable]
        public class RoadSliceTestParam
        {
            public bool enable = false;
            public long rnRoadId = -1;
            public AxisPlane plane = AxisPlane.Xz;
            public bool exec = false;
            public bool exec2 = false;
        }
        [FormerlySerializedAs("lineStringIntersectionTest")] public RoadSliceTestParam roadIntersectionTest = new RoadSliceTestParam();

        private IEnumerable<Transform> GetChildren(Transform self)
        {
            for (var i = 0; i < self.childCount; i++)
            {
                var child = self.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                    yield return self.GetChild(i);
            }
        }

        public List<Vector2> GetVertices()
        {
            return GetChildren(transform).Select(v => v.position.ToVector2(axis)).ToList();
        }

        public List<Vector3> GetVertices3D()
        {
            return GetChildren(transform).Select(v => v.position).ToList();
        }

        /// <summary>
        /// この順番を逆にする
        /// </summary>
        public void ReverseChildrenSibling()
        {
            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).SetAsFirstSibling();
        }

        private void EdgeBorderTest(EdgeBorderTestParam p)
        {
            if (p.enable == false)
                return;
            var vertices = GetVertices();
            var edgeIndices = GeoGraph2D.FindMidEdge(GetVertices(), p.allowAngle, p.skipAngle, p.op);

            void DrawLine(IEnumerable<int> ind, Color color)
            {
                DebugEx.DrawArrows(ind.Select(i => vertices[i].ToVector3(axis)), color: color);
            }

            DrawLine(Enumerable.Range(0, edgeIndices[0] + 1), Color.green);
            DrawLine(edgeIndices, Color.red);
            DrawLine(Enumerable.Range(edgeIndices.Last(), vertices.Count - edgeIndices.Last()), Color.green);
        }

        private void TurnTypeTest(TunTypeTestParam p)
        {
            if (p.enable == false)
                return;
            var vertices = GetVertices();
            if (vertices.Count < 3)
            {
                Debug.LogWarning("頂点数が足りません");
                return;
            }
            var d1 = vertices[1] - vertices[0];
            var d2 = vertices[2] - vertices[1];
            var type = RnTurnTypeEx.GetTurnType(d1, d2);
            DebugEx.DrawString($"{type}", vertices[0].ToVector3(axis));
        }

        private void RoadSliceTest(RoadSliceTestParam p)
        {
            var exec = p.exec;
            p.exec = false;
            var exec2 = p.exec2;
            p.exec2 = false;
            if (p.enable == false)
                return;
            var target = GameObject.FindAnyObjectByType<PLATEAURnStructureModel>();
            var plateauRnStructureModel = target;
            if (!plateauRnStructureModel || plateauRnStructureModel.RoadNetwork == null)
                return;

            var edges = GeoGraphEx.GetEdges(GetVertices3D(), false).Select(s => new LineSegment3D(s.Item1, s.Item2)).ToList();

            var roads = plateauRnStructureModel.RoadNetwork.Roads.Where(r =>
                p.rnRoadId < 0 || r.GetDebugMyIdOrDefault() == p.rnRoadId).ToList();
            foreach (var road in roads)
            {
                var isCrossed = false;
                foreach (var segment in edges)
                {
                    var res = road.GetLaneCrossPoints(segment);
                    if (res == null)
                        continue;
                    foreach (var v in res.TargetLines.SelectMany(w => w.Intersections))
                    {
                        DebugEx.DrawSphere(v.v, 0.3f, Color.red);
                        DebugEx.DrawString($"{v.index}", v.v);
                        isCrossed = true;
                    }

                    if (exec && isCrossed)
                    {
                        plateauRnStructureModel.RoadNetwork.SliceRoadHorizontal(road, segment);
                    }
                }

                if (exec2 && edges.Count >= 3 && isCrossed)
                {
                    plateauRnStructureModel.RoadNetwork.SliceRoadHorizontalAndConvert2Intersection(road, edges[0], edges[2]);
                }
            }

            var intersections = plateauRnStructureModel.RoadNetwork.Intersections.Where(r =>
                p.rnRoadId < 0 || r.GetDebugMyIdOrDefault() == p.rnRoadId).ToList();
            foreach (var road in intersections)
            {
                var isCrossed = false;
                foreach (var segment in edges)
                {
                    var res = road.GetEdgeCrossPoints(segment);
                    if (res == null)
                        continue;
                    foreach (var v in res.TargetLines.SelectMany(w => w.Intersections))
                    {
                        DebugEx.DrawSphere(v.v, 0.3f, Color.red);
                        DebugEx.DrawString($"{v.index}", v.v);
                        isCrossed = true;
                    }

                    if (exec && isCrossed)
                    {
                        var sliceRes = plateauRnStructureModel.RoadNetwork.SliceIntersectionHorizontal(road, segment);
                        DebugEx.Log(sliceRes.Result);
                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (visible)
            {
                DebugEx.DrawArrows(GetVertices().Select(v => v.ToVector3(axis)), color: color);

                if (visibleNormal)
                {
                    var vertices = GetVertices3D();
                    for (var i = 0; i < vertices.Count - 1; i++)
                    {
                        var v = vertices[i].PutNormal(axis, 0);
                        var next = vertices[(i + 1) % vertices.Count].PutNormal(axis, 0);
                        var p = Vector3.Lerp(v, next, 0.5f);

                        var a = axis.NormalVector();
                        var n = Vector3.Cross(a, next - v).normalized;
                        DebugEx.DrawArrow(p, p + n, bodyColor: Color.blue);
                    }
                }
            }

            EdgeBorderTest(edgeBorderTest);
            TurnTypeTest(turnTypeTest);
            RoadSliceTest(roadIntersectionTest);
        }

    }
}