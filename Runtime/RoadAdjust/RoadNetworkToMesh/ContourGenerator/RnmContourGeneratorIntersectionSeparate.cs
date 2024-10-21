using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 交差点の輪郭線を生成します。レーンは分割します。
    /// </summary>
    internal class RnmContourMeshGeneratorIntersectionSeparate: IRnmContourMeshGenerator
    {
        public RnmContourMeshList Generate(RnModel model)
        {
            var cMeshes = new RnmContourMeshList();
            foreach (var inter in model.Intersections)
            {
                var targetObj = inter.TargetTran == null ? null : inter.TargetTran.gameObject;
                foreach (var c in GenerateContours(inter))
                {
                    cMeshes.Add(new RnmContourMesh(targetObj, c));
                }
            }
            return cMeshes;
        }

        public IEnumerable<RnmContour> GenerateContours(RnIntersection inter)
        {
            yield return CarContour(inter);
            foreach (var sideWalk in inter.SideWalks)
            {
                yield return SidewalkContour(sideWalk);
            }

            foreach (var outer in NonSidewalkOuters(inter))
            {
                yield return outer;
            }
        }

        /// <summary> 車道 </summary>
        private RnmContour CarContour(RnIntersection inter)
        {
            var edges = inter.Edges.Select(e => e.Border);
            var calc = new RnmContourCalculator(RnmMaterialType.CarLane);
            calc.AddRangeLine(edges);
            return calc.Calculate();
        }

        /// <summary> 歩道 </summary>
        private RnmContour SidewalkContour(RnSideWalk sideWalk)
        {
            var calc = new RnmContourCalculator(RnmMaterialType.SideWalk);
            var lines = new List<IEnumerable<Vector3>> { sideWalk.InsideWay, sideWalk.OutsideWay};
            lines.AddRange(sideWalk.EdgeWays);
            calc.AddRangeLine(lines.Where(l => l != null));
            return calc.Calculate();
        }

        /// <summary> ネットワーク上では歩道判定でないが車道の外側の部分 </summary>
        private IEnumerable<RnmContour> NonSidewalkOuters(RnIntersection inter)
        {
            var outsideBorderEdges = new OutsideBorderEdges(inter).Collect().ToArray();
            var neighborSidewalkEdges = new NeighborSidewalkEdges(inter).Collect().ToArray();
            var sidewalkEdges = inter.SideWalks.SelectMany(w => w.EdgeWays);
            var sumSideEdges = neighborSidewalkEdges.Concat(sidewalkEdges).ToArray();
            foreach (var border in outsideBorderEdges)
            {
                var matchingSideEdges = new HashSet<RnWay>();
                foreach (var sideEdge in sumSideEdges)
                {
                    if (ContourGeneratorCommon.NearestDist(border, sideEdge) < 1f)
                    {
                        matchingSideEdges.Add(sideEdge);
                    }
                }

                if (matchingSideEdges.Count > 0)
                {
                    var calc = new RnmContourCalculator(RnmMaterialType.SideWalk);
                    var lines = new List<IEnumerable<Vector3>> { border };
                    lines.AddRange(matchingSideEdges);
                    calc.AddRangeLine(lines);
                    yield return calc.Calculate();
                }
            }
        }
        
        
        /// <summary>
        /// 交差点について、nonBorderEdgeのうち歩道と重ならないものを返します。 </summary>
        private class OutsideBorderEdges : IRnWayCollector
        {
            private readonly RnIntersection inter;

            public OutsideBorderEdges(RnIntersection inter)
            {
                this.inter = inter;
            }

            public IEnumerable<RnWay> Collect()
            {
                foreach (var nonBorder in inter.Edges.Where(e => !e.IsBorder).Select(e => e.Border))
                {
                    // bool isBorderOutside = true;
                    // 条件: 歩道のinsideWayと重ならない
                    var insideWays = inter.SideWalks.Select(sw => sw.InsideWay).SelectMany(w => w.Vertices);
                    var nonBorder2 = new RnmLine(nonBorder).SubtractSeparate(insideWays, 1);
                    foreach (var b in nonBorder2)
                        yield return new RnWay(new RnLineString(b.Vertices.Select(v => new RnPoint(v))));

                    // 条件: StartEdgeWayともEndEdgeWayとも重ならない
                    // var neighborEdges = inter.Neighbors.Select(n => n.Road).Where(r => r != null)
                    //     .SelectMany(r => r.SideWalks).SelectMany(sw => sw.EdgeWays).Where(ew => ew != null).SelectMany(e => e.Vertices);
                    // var nonBorder3 = nonBorder2.SelectMany(nb => nb.SubtractSeparate(neighborEdges, 1));
                    //
                    // foreach (var nb3 in nonBorder3) yield return new RnWay(new RnLineString(nb3.Vertices.Select(v => new RnPoint(v))));
                }
            }
        }
        
        /// <summary> 交差点に隣接する道路の歩道の端を取得します。 </summary>
        private class NeighborSidewalkEdges : IRnWayCollector
        {
            private const float NearThreshold = 1f;
            private readonly RnIntersection inter;

            public NeighborSidewalkEdges(RnIntersection inter)
            {
                this.inter = inter;
            }

            public IEnumerable<RnWay> Collect()
            {
                // 隣接する道路について
                foreach (var neighbor in inter.Neighbors.Where(n => n.Road != null))
                {
                    // 隣の道路のもっとも近く、十分な近さである StartEdgeWay, EndEdgeWay を選択
                    var carBoarders = new CarBorder(inter).Collect().ToArray();
                    var nSidewalks = neighbor.Road.SideWalks;
                    var nStarts = nSidewalks.Select(sw => sw.StartEdgeWay).Where(w => w != null).ToArray();
                    var nEnds = nSidewalks.Select(sw => sw.EndEdgeWay).Where(w => w != null).ToArray();
                    if (nStarts.Length != 0)
                    {
                        int nearestID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(nStarts, neighbor.Border);
                        var nearest = nStarts[nearestID];
                        float minDist = carBoarders.Min(cb =>
                            ContourGeneratorCommon.NearestDist(nearest, cb));
                        if (minDist < NearThreshold)
                        {
                            yield return nearest;
                        }
                    }

                    if (nEnds.Length != 0)
                    {
                        int nearestID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(nEnds, neighbor.Border);
                        var nearest = nEnds[nearestID];
                        float minDist = carBoarders.Min(cb =>
                            ContourGeneratorCommon.NearestDist(nearest, cb));
                        if (minDist < NearThreshold)
                        {
                            yield return nEnds[nearestID];
                        }
                    }
                }
            }
        }
        
        /// <summary> 交差点の車道のボーダーを返します </summary>
        private class CarBorder : IRnWayCollector
        {
            private readonly RnIntersection inter;

            public CarBorder(RnIntersection inter)
            {
                this.inter = inter;
            }

            public IEnumerable<RnWay> Collect()
            {
                return inter.Edges
                    .Where(e => e.IsBorder)
                    .Select(e => e.Border);
            }
        }
    }
}