using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 交差点の輪郭線を生成します。 </summary>
    internal class RnmContourGeneratorIntersection : IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            foreach (var inter in model.Intersections)
            {
                // 交差点の外側に位置するWayをここに列挙します。
                var outsideWaysCollectors = new IRnWayCollector[]
                {
                    new Sidewalk(inter), // 歩道と交差点の歩道の端。歩道から、交差点のカーブ部分が得られます。交差点の歩道の端から、一部隣接する道路の端が得られます。
                    new CarBorder(inter), // 車道のボーダー。車道と接する部分が得られます。
                    new NeighborSidewalkEdges(inter), // 隣接する道路の歩道の端。交差点自身の歩道の端では得られない歩道端を隣接道路から取得します。
                    new OutsideBorderEdges(inter), // 交差点ボーダーのうち外側に位置するもの。歩道がない箇所では代わりにこちらが使われるようにします。
                };

                // 交差点ごとの輪郭線を作ります。
                var calc = new RnmContourCalculator();
                foreach (var collector in outsideWaysCollectors)
                {
                    calc.AddRangeLine(collector.Collect());
                }

                var contour = calc.Calculate();
                contours.Add(contour);
            }

            return contours;
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

        /// <summary> 交差点の歩道と、歩道の端を返します </summary>
        private class Sidewalk : IRnWayCollector
        {
            private readonly RnIntersection inter;

            public Sidewalk(RnIntersection inter)
            {
                this.inter = inter;
            }

            public IEnumerable<RnWay> Collect()
            {
                // 歩道の外側
                var walks = inter.SideWalks.Select(sw => sw.OutsideWay);

                // 歩道の端にある線
                var walksEdge =
                    inter.SideWalks
                        .SelectMany(w => new[] { w.StartEdgeWay, w.EndEdgeWay })
                        .Where(w => w != null);

                return walks.Concat(walksEdge);
            }
        }

        /// <summary>
        /// 交差点について、nonBorderEdgeのうち外側のものを返します。
        /// すなわち、nonBorderEdgeのうち、歩道のinsideWayと重ならず、StartEdgeWayともEndEdgeWayとも重ならないものを返します。 </summary>
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
                    
                    // 条件: StartEdgeWayともEndEdgeWayとも重ならない
                    var neighborEdges = inter.Neighbors.Select(n => n.Road).Where(r => r != null)
                        .SelectMany(r => r.SideWalks).SelectMany(sw => sw.EdgeWays).Where(ew => ew != null).SelectMany(e => e.Vertices);
                    var nonBorder3 = nonBorder2.SelectMany(nb => nb.SubtractSeparate(neighborEdges, 1));

                    foreach (var nb3 in nonBorder3) yield return new RnWay(new RnLineString(nb3.Vertices.Select(v => new RnPoint(v))));
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
    }
}