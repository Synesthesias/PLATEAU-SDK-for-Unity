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

            public int nest = 0;

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
            static List<Vector2> Reduction(IReadOnlyList<Vector2> srcVertices, int nest, AxisPlane plane)
            {
                if (nest <= 0)
                    return srcVertices.ToList();

                var vertices = srcVertices.ToList();
                Vector2 GetEdgeNormal(int x)
                {
                    x += vertices.Count;
                    return -RnEx.GetEdgeNormal(vertices[x % vertices.Count], vertices[(x + 1) % vertices.Count]);
                }

                Vector2 GetVertexNormal(int x)
                {
                    return (GetEdgeNormal(x) + GetEdgeNormal(x - 1)).normalized;
                }

                var ret = vertices.ToList();
                ret = new List<Vector2>();

                Dictionary<int, (float minLen, int index, float offset, Vector2 inter)> minLenDic = new();

                void Check(int srcIndex, int dstIndex, Vector2 inter)
                {
                    var srcV = vertices[srcIndex];
                    var dir = (inter - srcV);
                    var len = dir.magnitude;

                    var en = GetEdgeNormal(srcIndex);

                    var offset = Vector2.Dot(en, dir);
                    if (minLenDic.TryGetValue(srcIndex, out var minLen) == false)
                    {
                        minLenDic[srcIndex] = (len, dstIndex, offset, inter);
                    }
                    else if (len < minLen.minLen)
                    {
                        minLenDic[srcIndex] = (len, dstIndex, offset, inter);
                    }
                }
                for (var i = 0; i < vertices.Count; ++i)
                {
                    var vn1 = GetVertexNormal(i);
                    var halfRay1 = new Ray2D(vertices[i], vn1);
                    for (var j = i + 1; j < vertices.Count; ++j)
                    {
                        var vn2 = GetVertexNormal(j);

                        var halfRay2 = new Ray2D(vertices[j], vn2);
                        if (halfRay2.CalcIntersection(halfRay1, out var inter, out var t1, out var t2) == false)
                            continue;

                        Check(i, j, inter);
                        Check(j, i, inter);
                    }
                }

                List<Vector2> Move(List<Vector2> vertices, float delta)
                {
                    var points = new List<Vector2>();
                    for (var i = 0; i < vertices.Count; ++i)
                    {
                        var e0 = GetEdgeNormal(i);
                        var e1 = GetEdgeNormal(i - 1);
                        var dd = e0 + e1 * (1f - Vector2.Dot(e0, e1));
                        points.Add(vertices[i] + dd * delta);
                    }

                    return points;
                }

                if (minLenDic.Where(x =>
                    {
                        var key = x.Key;
                        var val = x.Value.index;
                        return minLenDic[val].index == key;
                    }).TryFindMinElement(x => x.Value.offset, out var e))
                {
                    ret = Move(vertices, e.Value.offset);
                    var (from, to) = (e.Key, e.Value.index);
                    if (from > to)
                        (to, from) = (from, to);

                    var range = ret.GetRange(from, to - from);
                    ret.RemoveRange(from, to - from);

                    if (GeoGraph2D.CalcPolygonArea(range) > GeoGraph2D.CalcPolygonArea(ret))
                    {
                        ret = range;
                    }

                    DebugEx.DrawString($"{nest}", e.Value.inter.ToVector3(plane));

                    DebugEx.DrawArrow(vertices[e.Key].ToVector3(plane), e.Value.inter.ToVector3(plane));
                    DebugEx.DrawArrow(vertices[e.Value.index].ToVector3(plane), e.Value.inter.ToVector3(plane));

                    return Reduction(ret, nest - 1, plane);
                }

                return vertices;
            }

            var vertices = GetVertices().Skip(1).ToList();

            if (GeoGraph2D.IsClockwise(vertices) == false)
                vertices.Reverse();
            var newVertices = Reduction(vertices, param.nest, plane);
            var lines = GeoGraph2D.ExtractSelfCrossing(newVertices, x => x, ((p1, p2, p3, p4, inter, arg6, arg7) => inter), isLoop: true);

            if (param.onlyMaxArea)
            {
                if (lines.TryFindMaxElement(GeoGraph2D.CalcPolygonArea, out var poly))
                {
                    var srcArea = GeoGraph2D.CalcPolygonArea(vertices);
                    var dstArea = GeoGraph2D.CalcPolygonArea(poly);

                    DebugEx.DrawString($"{(int)(100 * dstArea / srcArea)}%", Vector2Ex.Centroid(vertices).ToVector3(plane));

                    DebugEx.DrawArrows(poly.Select(v => v.ToVector3(plane)), isLoop: true, color: color);

                    //var after = Reduction(l, -param.offset);
                    //DebugEx.DrawArrows(after.Select(v => v.ToVector3(LineString.plane)), isLoop: true, color: LineString.color);

                    var edgeIndices = GeoGraph2D.FindMidEdge(poly, param.allowAngle, param.skipAngle, param.op);
                    void DrawLine(IEnumerable<int> ind, Color color)
                    {
                        DebugEx.DrawArrows(ind.Select(i => poly[i].ToVector3(plane)), color: color);
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