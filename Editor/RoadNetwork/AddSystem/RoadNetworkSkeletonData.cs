using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork
{
    internal struct ExtensibleRoadEdge
    {
        public RnRoadGroup road;
        public bool isPrev;
        public Vector3 center;
        public Vector3 forward;

        public ExtensibleRoadEdge(RnRoadGroup road, bool isPrev, Vector3 center, Vector3 forward)
        {
            this.road = road;
            this.isPrev = isPrev;
            this.center = center;
            this.forward = forward;
        }
    }

    internal struct ExtensibleIntersectionEdge
    {
        public RnIntersection intersection;
        public RnNeighbor neighbor;
        public int index;
        public Vector3 center;
        public Vector3 forward;

        public ExtensibleIntersectionEdge(RnIntersection intersection, RnNeighbor neighbor, int index, Vector3 center, Vector3 forward)
        {
            this.intersection = intersection;
            this.neighbor = neighbor;
            this.index = index;
            this.center = center;
            this.forward = forward;
        }
    }

    internal class RnIntersectionSkeleton
    {
        public Vector3 Position { get; }
        public RnIntersection Intersection { get; }
        public List<ExtensibleIntersectionEdge> ExtensibleEdges { get; private set; }

        public RnIntersectionSkeleton(RnIntersection intersection)
        {
            Intersection = intersection;
            Reconstruct();
        }

        public void Reconstruct()
        {
            ExtensibleEdges = FindExtensibleEdges(Intersection);
        }

        /// <summary>
        /// 道路が追加可能なエッジを探す
        /// </summary>
        /// <param name="intersection"></param>
        /// <returns></returns>
        private static List<ExtensibleIntersectionEdge> FindExtensibleEdges(RnIntersection intersection)
        {
            var extensibleEdges = new List<ExtensibleIntersectionEdge>();
            foreach (var edge in intersection.Edges)
            {
                if (edge.IsBorder) continue;

                var border = edge.Border;
                // 歩道の外輪郭となる線分（＝insideWayと線分を共有しない線分）を探す
                for (var i = 0; i < border.Points.Count() - 1; i++)
                {
                    var point1 = border.Points.ElementAt(i);
                    var point2 = border.Points.ElementAt(i + 1);
                    var isLineExposed = true;
                    foreach (var sideWalk in intersection.SideWalks)
                    {
                        var insideWay = sideWalk.InsideWay;
                        if (insideWay == null) continue;

                        for (int index = 0; index < insideWay.Points.Count() - 1; index++)
                        {
                            var insidePoint1 = insideWay.Points.ElementAt(index);
                            var insidePoint2 = insideWay.Points.ElementAt(index + 1);
                            if ((insidePoint1.Vertex == point1.Vertex && insidePoint2.Vertex == point2.Vertex) ||
                                (insidePoint1.Vertex == point2.Vertex && insidePoint2.Vertex == point1.Vertex))
                            {
                                isLineExposed = false;
                                break;
                            }
                        }

                        if (!isLineExposed)
                            break;
                    }

                    if (!isLineExposed)
                        continue;

                    extensibleEdges.Add(new ExtensibleIntersectionEdge(
                        intersection,
                        edge,
                        i,
                        (point1.Vertex + point2.Vertex) / 2,
                        -Vector3.Cross(point1.Vertex - point2.Vertex, Vector3.up).normalized));
                }
            }
            return extensibleEdges;
        }
    }

    internal class RnRoadSkeleton
    {
        public Spline Spline { get; private set; }
        public RnRoad Road { get; }
        public RnRoadGroup RoadGroup { get; private set; }
        public List<ExtensibleRoadEdge> ExtensibleEdges { get; private set; }

        public RnRoadSkeleton(RnRoad road)
        {
            Road = road;
            Reconstruct();
        }

        /// <summary>
        /// 道路のスケルトンを再構築
        /// </summary>
        public void Reconstruct()
        {
            RoadGroup = Road.CreateRoadGroup();
            Spline = CreateCenterSpline(RoadGroup);
            ExtensibleEdges = FindExtensibleEdge(RoadGroup, Spline);
        }

        public static Spline CreateCenterSpline(RnRoadGroup road)
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

        public static List<ExtensibleRoadEdge> FindExtensibleEdge(RnRoadGroup road, Spline spline)
        {
            var extensibleEdges = new List<ExtensibleRoadEdge>();

            if (spline.Knots.Count() == 0)
                return extensibleEdges;

            if (road.PrevIntersection == null)
                extensibleEdges.Add(new ExtensibleRoadEdge(road, true, spline.Knots.First().Position, -spline.EvaluateTangent(0f)));
            if (road.NextIntersection == null)
                extensibleEdges.Add(new ExtensibleRoadEdge(road, false, spline.Knots.Last().Position, -spline.EvaluateTangent(1f)));

            return extensibleEdges;
        }
    }

    internal class RoadNetworkSkeletonData
    {
        public List<RnIntersectionSkeleton> Intersections { get; private set; }
        public List<RnRoadSkeleton> Roads { get; private set; }

        public RoadNetworkSkeletonData(PLATEAURnStructureModel structureModel)
        {
            Roads = structureModel.RoadNetwork.Roads.Select(x => new RnRoadSkeleton(x)).ToList();

            Intersections = structureModel.RoadNetwork.Intersections.Select(x => new RnIntersectionSkeleton(x)).ToList();
        }

        /// <summary>
        /// 隣接交差点も含めて道路のスケルトンを再構築
        /// </summary>
        public void ReconstructIncludeNeighbors(RnRoad road)
        {
            var roadSkeleton = Roads.FirstOrDefault(x => x.Road == road);
            if (roadSkeleton == null)
            {
                roadSkeleton = new RnRoadSkeleton(road);
                Roads.Add(roadSkeleton);
            }
            else
            {
                roadSkeleton?.Reconstruct();
            }
            foreach (var intersection in new List<RnIntersection> { roadSkeleton.RoadGroup.PrevIntersection, roadSkeleton.RoadGroup.NextIntersection })
            {
                Reconstruct(intersection);
            }
        }

        /// <summary>
        /// 隣接道路も含めて交差点のスケルトンを再構築
        /// </summary>
        public void ReconstructIncludeNeighbors(RnIntersection intersection)
        {
            var intersectionSkeleton = Intersections.FirstOrDefault(x => x.Intersection == intersection);
            if (intersectionSkeleton == null)
            {
                intersectionSkeleton = new RnIntersectionSkeleton(intersection);
                Intersections.Add(intersectionSkeleton);
            }
            else
            {
                intersectionSkeleton?.Reconstruct();
            }
            foreach (var road in intersection.GetNeighborRoads())
            {
                Reconstruct((RnRoad)road);
            }
        }

        /// <summary>
        /// 道路のスケルトンを再構築
        /// </summary>
        /// <param name="road"></param>
        public void Reconstruct(RnRoad road)
        {
            var roadSkeleton = Roads.FirstOrDefault(x => x.Road == road);
            if (roadSkeleton == null)
            {
                roadSkeleton = new RnRoadSkeleton(road);
                Roads.Add(roadSkeleton);
            }
            else
            {
                roadSkeleton?.Reconstruct();
            }
        }

        /// <summary>
        /// 交差点のスケルトンを再構築
        /// </summary>
        /// <param name="intersection"></param>
        public void Reconstruct(RnIntersection intersection)
        {
            if (intersection == null)
                return;

            var intersectionSkeleton = Intersections.FirstOrDefault(x => x.Intersection == intersection);
            if (intersectionSkeleton == null)
            {
                intersectionSkeleton = new RnIntersectionSkeleton(intersection);
                Intersections.Add(intersectionSkeleton);
            }
            else
            {
                intersectionSkeleton?.Reconstruct();
            }
        }
    }
}
