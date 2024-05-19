using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    public class PLATEAUGeoGraphTester : MonoBehaviour
    {
        [SerializeField] private bool showConvexVolume = false;
        public List<PLATEAUCityObjectGroup> geoTestTargets = new List<PLATEAUCityObjectGroup>();

        [Serializable]
        public class LerpLineTest
        {
            public bool showLerpLineTest = false;
            public PLATEAUCityObjectGroup target;
            public int indexA = 0;
            public int indexB = 1;
            public float p = 0.5f;
        }

        public LerpLineTest lerpLineTest = new LerpLineTest();

        public void OnDrawGizmos()
        {
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

            if ((lerpLineTest?.showLerpLineTest ?? false) && lerpLineTest.target)
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