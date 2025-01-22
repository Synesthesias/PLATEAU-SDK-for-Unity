using NetTopologySuite.Operation.Valid;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.RoadNetwork.Util.StraightSkeleton;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;
using Vector2 = UnityEngine.Vector2;

namespace PLATEAU.RoadNetwork.Tester
{
    public class PLATEAUTesterStraightSkeleton : PLATEAURnTesterLineStringHolder
    {
        [Serializable]
        public class FindMidEdgeTestParam
        {
            public bool enable = false;

            public float offset = 0.1f;

            public bool onlyMaxArea = false;


            public float allowAngle = 20f;
            public float skipAngle = 20f;
            public GeoGraph2D.DebugFindOppositeOption op = new GeoGraph2D.DebugFindOppositeOption();
        }


        public FindMidEdgeTestParam skeletonTest;


        public void Test(FindMidEdgeTestParam param)
        {
            if (!param.enable)
                return;
            var vertices = GetVertices();


            static List<Vector2> Reduction(IReadOnlyList<Vector2> srcVertices, float offset)
            {
                var vertices = srcVertices.ToList();
                Vector2 GetEdgeNormal(int x)
                {
                    x += vertices.Count;
                    return RnEx.GetEdgeNormal(vertices[x % vertices.Count], vertices[(x + 1) % vertices.Count]);
                }

                var ret = vertices.ToList();
                ret = new List<Vector2>();

                offset = Mathf.Abs(offset);
                var delta = offset;
                for (var i = 0; i < vertices.Count; ++i)
                {
                    var e0 = GetEdgeNormal(i);
                    var e1 = GetEdgeNormal(i - 1);
                    var dd = e0 + e1 * (1f - Vector2.Dot(e0, e1));
                    ret.Add(vertices[i] - dd * delta);
                }
                //while (false)
                //{


                //    var removeIndices = new List<(int index, Vector2 after)>();


                //    var stopDelta = float.MaxValue;
                //    var stopIndex = -1;
                //    for (var i = 0; i < vertices.Count; ++i)
                //    {
                //        for (var j = i + 1; j < vertices.Count; ++j)
                //        {
                //            var prevIndex = (i - 1 + vertices.Count) % vertices.Count;
                //            var nowSeg = new LineSegment2D(vertices[i], ret[i]);
                //            var prevSeg = new LineSegment2D(vertices[j], ret[j]);
                //            if (nowSeg.TrySegmentIntersection(prevSeg, out var intersection) == false)
                //                continue;

                //            var e2 = GetEdgeNormal(i);
                //            //removeIndices.Add(new(prevIndex, intersection - backN * offset));

                //            var nowV = nowSeg.End - intersection;
                //            var preV = prevSeg.End - intersection;
                //            var d = Vector2.Dot(e2, prevSeg.Direction) * delta;
                //            stopDelta = Mathf.Min(d, stopDelta);
                //            stopIndex = i;
                //        }

                //    }
                //    removeIndices = removeIndices.OrderByDescending(x => x.index).ToList();
                //    foreach (var i in removeIndices)
                //    {
                //        vertices.RemoveAt(i.index);
                //        vertices[i.index % vertices.Count] = i.after;
                //    }
                //}



                return ret;
            }
            var newVertices = vertices.ToList();
            newVertices = Reduction(newVertices, param.offset);
            var lines = GeoGraph2D.ExtractSelfCrossing(newVertices, x => x, ((p1, p2, p3, p4, inter, arg6, arg7) => inter), isLoop: true);

            if (param.onlyMaxArea)
            {
                if (lines.TryFindMaxElement(GeoGraph2D.CalcPolygonArea, out var l))
                {
                    DebugEx.DrawArrows(l.Select(v => v.ToVector3(plane)), isLoop: true, color: color);

                    var after = Reduction(l, -param.offset);
                    //DebugEx.DrawArrows(after.Select(v => v.ToVector3(LineString.plane)), isLoop: true, color: LineString.color);

                    var edgeIndices = GeoGraph2D.FindMidEdge(l, param.allowAngle, param.skipAngle, param.op);
                    void DrawLine(IEnumerable<int> ind, Color color)
                    {
                        DebugEx.DrawArrows(ind.Select(i => l[i].ToVector3(plane)), color: color);
                    }

                    DrawLine(edgeIndices, Color.red);
                }

                //DrawLine(Enumerable.Range(0, edgeIndices[0] + 1), Color.green);
                //DrawLine(Enumerable.Range(edgeIndices.Last(), vertices.Count - edgeIndices.Last()), Color.green);
            }
            else
            {

                foreach (var l in lines)
                {
                    DebugEx.DrawArrows(l.Select(v => v.ToVector3(plane)), isLoop: true, color: color);
                }
            }

        }


        public void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy)
                return;
            Test(skeletonTest);
        }
    }
}