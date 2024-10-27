using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Voronoi;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork.Tester
{
    public class PLATEAUTesterVoronoiDiagram : MonoBehaviour
    {
        public AxisPlane plane = AxisPlane.Xz;
        public float sphereRadius = 0.1f;

        [Serializable]
        public class BeachLineTestParam
        {
            public bool enable = false;
            // 走査線のY座標
            public float lineY;
            // 描画範囲
            public float rangeX = 10f;
            // 放物線描く時の点計算間隔
            public float parabolaLineInterval = 0.1f;
        }
        [SerializeField]
        BeachLineTestParam beachLineTestParam = new();

        private void BeachLineTest(BeachLineTestParam p)
        {
            if (p.enable == false)
                return;
            var tr = transform;
            var lines = Enumerable.Range(0, tr.childCount)
                .Select(i => tr.GetChild(i))
                .Where(c => c.gameObject.activeInHierarchy)
                .Select(c => c.GetComponent<PLATEAUGeoGraphTesterLineString>())
                .Where(s => s != null)
                .ToList();

            var vertices = lines
                .SelectMany(l =>
                {
                    return l.GetVertices3D();
                })
                .ToList();
            foreach (var v in vertices.Select(a => new Vector2d(a.ToVector2(plane))))
            {
                if (RnVoronoiEx.CalcBeachLine(v, p.lineY, out var parabola) == false)
                    continue;

                IEnumerable<Vector3> CalcPoints()
                {
                    for (var x = -p.rangeX; x <= p.rangeX; x += p.parabolaLineInterval)
                    {
                        var y = parabola.GetY(x);
                        yield return new Vector2(x, (float)y).ToVector3(plane);
                    }
                }

                foreach (var e in GeoGraphEx.GetEdges(CalcPoints(), false))
                {
                    DebugEx.DrawLine(e.Item1, e.Item2, Color.red);
                }
            }

            var st = new Vector2(-p.rangeX, p.lineY);
            var en = new Vector2(p.rangeX, p.lineY);
            DebugEx.DrawLine(st.ToVector3(plane), en.ToVector3(plane), color: Color.green);
        }

        [Serializable]
        public class LerpLineSegmentsVoronoiTestParam
        {
            public bool enable = false;
            public bool showWay = true;
            public float interval = 1;
            public bool showAllEdge = false;
            public bool method = false;
            public float sphereSize = 0.1f;
            public float lineLength = 100f;
        }
        public LerpLineSegmentsVoronoiTestParam lerpSegmentsVoronoiTest = new();

        private void VoronoiTest(LerpLineSegmentsVoronoiTestParam param)
        {
            if (param.enable == false)
                return;

            var childIndex = 0;
            var tr = transform;

            var lines = Enumerable.Range(0, tr.childCount)
                .Select(i => tr.GetChild(i))
                .Where(c => c.gameObject.activeInHierarchy)
                .Select(c => c.GetComponent<PLATEAUGeoGraphTesterLineString>())
                .Where(s => s != null)
                .ToList();

            var vertices = lines
                .SelectMany(l =>
                {
                    var ls = RnLineString.Create(l.GetVertices3D(), false);
                    if (param.interval > 1)
                        ls.Refine(param.interval);
                    return ls.Points.Select(v => new { line = l, v = v.Vertex });
                })
                .ToList();

            var voronoiData = RnVoronoiEx.CalcVoronoiData(vertices, v => new Vector2d(v.v.ToVector2(plane)));

            for (var i = 0; i < vertices.Count; ++i)
            {
                var v = vertices[i];
                DebugEx.DrawSphere(v.v, param.sphereSize);
                DebugEx.DrawString($"{i}", v.v, color: Color.red, fontSize: 20);
            }

            Dictionary<Vector3, HashSet<int>> drawn = new();

            void DrawPoint(Vector3 x, int a)
            {
                if (drawn.TryGetValue(x, out var p) == false)
                {
                    drawn[x] = new HashSet<int> { a };
                    DebugEx.DrawSphere(x, param.sphereSize, color: Color.green);
                }
                drawn[x].Add(a);
            }
            var colors = new List<int>();
            Dictionary<Vector3, int> edgeCount = new();
            foreach (var e in voronoiData.Edges)
            {
                var color = DebugEx.GetDebugColor(childIndex++, 16);

                if (param.showAllEdge == false && e.LeftSitePoint.line == e.RightSitePoint.line)
                    continue;


                var c = e.LeftSitePoint.line.GetHashCode() ^ e.RightSitePoint.line.GetHashCode();
                var index = colors.IndexOf(c);
                if (index < 0)
                {
                    index = colors.Count;
                    colors.Add(c);
                }
                color = DebugEx.GetDebugColor(index, 16);

                // 完全な直線の場合サイトポイントの中間点(2等分線だから)
                var mid = new Vector2d(((e.LeftSitePoint.v + e.RightSitePoint.v) * 0.5f).ToVector2(plane));
                var st = Vector3.zero;
                var en = Vector3.zero;
                var d = e.Direction * param.lineLength;
                if (e.Start == null && e.End == null)
                {
                    st = (mid - d).ToVector2().ToVector3(plane);
                    en = (mid + d).ToVector2().ToVector3(plane);
                }
                else
                {
                    st = (e.Start ?? (e.End.Value - d)).ToVector2().ToVector3(plane);
                    en = (e.End ?? (e.Start.Value + d)).ToVector2().ToVector3(plane);

                }

                DebugEx.DrawLine(st, en, color);
                var p = (st + en) * 0.5f;
                var n = (en - st).ToVector2(plane).normalized.Rotate(90).ToVector3(plane) * 0.1f;

                var x = edgeCount.TryGetValue(p, out var l) ? l + 1 : 1;
                edgeCount[p] = x;
                DebugEx.DrawString($"{e.LeftSiteIndex}", p + n * x, color: Color.blue, fontSize: 20);
                DebugEx.DrawString($"{e.RightSiteIndex}", p - n * x, color: Color.blue, fontSize: 20);

                DrawPoint(st, 0);
                DrawPoint(en, 0);
                //DebugEx.DrawString($"E", en);
                //DebugEx.DrawString($"S", st);
            }

            if (param.showWay)
            {
                for (var i = 0; i < lines.Count; ++i)
                {
                    var color = DebugEx.GetDebugColor(i, lines.Count);
                    DebugEx.DrawArrows(lines[i].GetVertices3D(), false, arrowSize: 0.1f, color: color,
                        arrowUp: Vector3.forward);
                }
            }
        }
        private IEnumerable<Transform> GetChildren(Transform self)
        {
            for (var i = 0; i < self.childCount; i++)
            {
                var child = self.GetChild(i);
                if (child.gameObject.activeInHierarchy == false)
                    continue;
                yield return child;
            }
        }

        public List<Vector2> GetVertices()
        {
            return GetChildren(transform).Select(v => v.position.ToVector2(plane)).ToList();
        }

        public List<Vector3> GetVertices3D()
        {
            return GetChildren(transform).Select(v => v.position).ToList();
        }

        public void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy)
                return;
            foreach (var v in GetVertices())
            {
                Gizmos.DrawSphere(v.ToVector3(plane), sphereRadius);
            }

            BeachLineTest(beachLineTestParam);
            VoronoiTest(lerpSegmentsVoronoiTest);
        }
    }
}