using PlasticPipe.PlasticProtocol.Messages;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Serialization;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace PLATEAU.RoadNetwork
{
    public class PLATEAUGeoGraphTester : MonoBehaviour
    {
        [SerializeField] private bool showConvexVolume = false;
        public List<PLATEAUCityObjectGroup> geoTestTargets = new List<PLATEAUCityObjectGroup>();

        [Serializable]
        public class LerpLineTest
        {
            public bool enable = false;
            public PLATEAUCityObjectGroup target;
            public int indexA = 0;
            public int indexB = 1;
            public float p = 0.5f;
        }

        public LerpLineTest lerpLineTest = new LerpLineTest();

        [Serializable]
        public class LerpLineSegmentsTest
        {
            public bool enable = false;
            public bool showWay = true;
            public int indexA = 0;
            public int indexB = 1;
            public float p = 0.5f;

            public GeoGraph2D.DebugOption op = new GeoGraph2D.DebugOption();
        }
        public LerpLineSegmentsTest lerpSegmentsTest = new LerpLineSegmentsTest();

        [Serializable]
        public class ParabolaTestParam
        {
            public bool enable = false;
            public float b = 1f;
            public float p = 0.5f;
            public Ray2D ray => new Ray2D(rayOrig, rayDirection);
            public Vector2 rayOrig;
            public Vector2 rayDirection;
            public Vector2 pos;
            public float minX = 0f;
            public float maxX = 10f;
            public float deltaX = 0.1f;
        }
        public ParabolaTestParam parabolaTest = new ParabolaTestParam();
        private IEnumerable<Transform> GetChildren(Transform self)
        {
            for (var i = 0; i < self.childCount; i++)
                yield return self.GetChild(i);
        }

        private List<Vector2> GetVertices(Transform self)
        {
            return GetChildren(self).Select(v => v.position.Xy()).ToList();
        }

        private void LerpSegmentsTest()
        {
            var param = lerpSegmentsTest;
            if (param.enable == false)
                return;
            if (param.showWay)
            {
                foreach (var item in GetChildren(transform).Select((v, i) => new { v, i }))
                {
                    var color = DebugEx.GetDebugColor(item.i, 8);
                    DebugEx.DrawArrows(GetChildren(item.v).Select(v => v.position), false, arrowSize: 0.1f, color: color, arrowUp: Vector3.forward);
                }
            }

            var tr = transform;
            for (var i = 0; i < tr.childCount - 1; i += 2)
            {
                var left = tr.GetChild(i);
                var right = tr.GetChild(i + 1);
                var leftVertices = GetVertices(left);
                var rightVertices = GetVertices(right);
                var segments = GeoGraph2D.GetInnerLerpSegments(leftVertices, rightVertices, param.p, param.op);
                foreach (var seg in segments.Select((v, i) => new { v, i }))
                {
                    DebugEx.DrawLineSegment2D(seg.v.Segment, color: DebugEx.GetDebugColor(seg.i, 16));

                }
            }
        }

        private void ParabolaTest()
        {
            if (parabolaTest.enable == false)
                return;

            var p = parabolaTest.p;
            var y = parabolaTest.b;
            var p2 = p * p;
            var a = 2 * p - 1;
            var b = 2 * y * p2;
            var c0 = p2 * y * y;

            // 以下の二次方程式になる
            // ay^2 + 2by + c0 + p^2x^2 = 0

            //var c0 = 2 * p2 * y0;
            //var c1 = 4 * p4 * y02 - 4 * d * p2 * y02;
            //var c2 = -4 * d * p2;
            // y = (c0 ± √(c1 + c2*x^2)) / 2d

            var rangeX = Mathf.Sqrt(((b * b / (4 * a)) - c0) / p2);

            float GetY(float x)
            {
                if (Mathf.Abs(a) < GeoGraph2D.Epsilon)
                    return x * x / (2 * y) + y / 2f;
                var c = c0 + p * p * x * x;
                var d = b * b - 4 * a * c;
                var ret = (b - Mathf.Sqrt(d)) / (2 * a);
                return ret;
            }

            var pos = parabolaTest.pos;
            var ray = parabolaTest.ray;
            var border = GeoGraph2D.GetBorderParabola(ray, parabolaTest.pos, parabolaTest.p);
            var minX = border.RangeX.HasValue ? -border.RangeX.Value : parabolaTest.minX;
            for (var x0 = -border.RangeX ?? parabolaTest.minX; x0 <= (border.RangeX ?? parabolaTest.maxX); x0 += parabolaTest.deltaX)
            {
                var y0 = GetY(x0);
                var x1 = x0 + parabolaTest.deltaX;
                var y1 = GetY(x1);
                var p0 = border.GetPoint(x0);
                var p1 = border.GetPoint(x1);
                //Debug.DrawLine(new Vector3(x0, y0), new Vector3(x1, y1));
                Debug.DrawLine(p0, p1);
            }

            Debug.DrawLine(ray.origin - ray.direction * 10, ray.origin + ray.direction * 10);
            Debug.DrawLine(pos, pos.Xya() + Vector3.forward);

            //Debug.DrawLine(Vector3.left * 10, Vector3.right * 10, Color.red);
            //Debug.DrawLine(Vector3.zero, Vector3.up, Color.green);
        }

        public void OnDrawGizmos()
        {
            LerpSegmentsTest();
            ParabolaTest();
            if (showConvexVolume)
            {
                var vertices = geoTestTargets
                    .Select(x => x.GetComponent<MeshCollider>())
                    .Where(x => x)
                    .SelectMany(x => x.sharedMesh.vertices.Select(a => a.Xz()))
                    .ToList();
                var convex = GeoGraph2D.ComputeConvexVolume(vertices);
                DebugEx.DrawArrows(convex.Select(x => x.Xay()));
            }

            if ((lerpLineTest?.enable ?? false) && lerpLineTest.target)
            {
                var p = lerpLineTest;
                var vertices = p.target.GetComponent<MeshCollider>().sharedMesh.vertices;

                Vector2 GetVertex(int index)
                {
                    return vertices[(index + vertices.Length) % vertices.Length].Xz();
                }

                Vector2 GetEdge(int index)
                {
                    return (GetVertex(index + 1) - GetVertex(index)).normalized;
                }

                var rayA = new Ray2D(GetVertex(p.indexA), GetEdge(p.indexA));
                var rayB = new Ray2D(GetVertex(p.indexA), -GetEdge(p.indexA - 1));

                DebugEx.DrawArrow(GetVertex(p.indexA).Xay(), GetVertex(p.indexA + 1).Xay(), bodyColor: Color.red);
                DebugEx.DrawArrow(GetVertex(p.indexA).Xay(), GetVertex(p.indexA - 1).Xay(), bodyColor: Color.blue);

                var ray = GeoGraph2D.LerpRay(rayA, rayB, p.p);
                Debug.DrawRay(ray.origin.Xay(), ray.direction.Xay(), Color.green);
            }
        }

    }
}