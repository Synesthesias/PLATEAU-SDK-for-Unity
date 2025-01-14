using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork
{
    internal class RnIntersectionSkeleton
    {
        public RnIntersectionSkeleton(RnIntersection intersection)
        {
            Intersection = intersection;
        }

        public Vector3 Position { get; }
        public RnIntersection Intersection { get; }
    }

    internal class RnRoadSkeleton
    {
        public RnRoadSkeleton(RnRoadGroup road)
        {
            Spline = CreateCenterSpline(road);
            Road = road;
        }

        public Spline Spline { get; private set; }
        public RnRoadGroup Road { get; }

        public void UpdateSpline()
        {
            Road.TryCreateSimpleSpline(out var spline, out var width);
            Spline = spline;
        }

        private static Spline CreateCenterSpline(RnRoadGroup road)
        {
            road.TryCreateSimpleSpline(out var spline, out var width);
            List<BezierKnot> knots = new List<BezierKnot>();
            BezierKnot prevKnot = new BezierKnot();
            foreach (var knot in spline.Knots)
            {
                // 重複しているノットがあるので削除
                if (prevKnot.Position.x == knot.Position.x &&
                    prevKnot.Position.z == knot.Position.z)
                    continue;

                var newKnot = knot;
                newKnot.Position += new float3(0f, 0f, 0f);
                knots.Add(newKnot);

                prevKnot = knot;
            }
            spline.Knots = knots.ToArray();

            return spline;
        }
    }

    internal class RoadNetworkSkeletonData
    {
        public List<RnIntersectionSkeleton> Intersections { get; private set; }
        public List<RnRoadSkeleton> Roads { get; private set; }

        public RoadNetworkSkeletonData(PLATEAURnStructureModel structureModel)
        {
            Roads = structureModel.RoadNetwork.Roads.Select(x => new RnRoadSkeleton(x.CreateRoadGroup())).ToList();

            Intersections = structureModel.RoadNetwork.Intersections.Select(x => new RnIntersectionSkeleton(x)).ToList();
        }

        internal void UpdateData(RnRoadGroup roadGroup)
        {
            var roadSkeleton = Roads.FirstOrDefault(x => x.Road == roadGroup);
            roadSkeleton?.UpdateSpline();
        }
    }
}
