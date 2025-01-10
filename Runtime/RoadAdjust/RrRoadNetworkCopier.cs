using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;

namespace PLATEAU.RoadAdjust
{
    /// <summary>
    /// 道路ネットワークのうち、<see cref="RoadReproducer"/>が使う部分をコピーします。
    /// RrはRoadReproducの略です。
    /// </summary>
    public class RrRoadNetworkCopier
    {
        /// <summary>
        /// 必要部分をディープコピーした道路ネットワークを返します。
        /// コピー元の<see cref="RnRoadBase"/>とコピー先を対応させる辞書をout引数で返します。
        /// </summary>
        public RnModel Copy(RnModel src, out Dictionary<RnRoadBase, RnRoadBase> outRoadSrcToDst)
        {
            // src→dstの辞書を構築しながら、srcをdstに置き換えてコピーします。
            
            // Pointsのコピー
            var points = new Dictionary<RnPoint, RnPoint>();
            foreach (var srcPoint in src.CollectAllLineStrings().SelectMany(ls => ls.Points).Distinct())
            {
                var dstPoint = new RnPoint(srcPoint.Vertex);
                points.Add(srcPoint, dstPoint);
            }
            
            // LineStringsのコピー
            var lineStrings = new Dictionary<RnLineString, RnLineString>();
            foreach (var srcLineStr in src.CollectAllLineStrings())
            {
                var dstLineStr = new RnLineString(srcLineStr.Points.Select(p => points[p]));
                lineStrings.Add(srcLineStr, dstLineStr);
            }
                
            // Wayのコピー
            var ways = new Dictionary<RnWay, RnWay>(); 
            foreach (var srcWay in src.CollectAllWays())
            {
                var lineStr = lineStrings[srcWay.LineString];
                ways.Add(srcWay, new RnWay(lineStr, srcWay.IsReversed, srcWay.IsReverseNormal));
            }
            
            // Laneのコピー
            var lanes = new Dictionary<RnLane, RnLane>();
            foreach (var srcLane in src.CollectAllLanes())
            {
                var dstLeftWay = srcLane.LeftWay == null ? null : ways[srcLane.LeftWay];
                var dstRightWay = srcLane.RightWay == null ? null : ways[srcLane.RightWay];
                var dstPrevBorder = srcLane.PrevBorder == null ? null : ways[srcLane.PrevBorder];
                var dstNextBorder = srcLane.NextBorder == null ? null : ways[srcLane.NextBorder];
                
                var dstLane = new RnLane(dstLeftWay, dstRightWay, dstPrevBorder, dstNextBorder);
                dstLane.IsReverse = srcLane.IsReverse;
                
                lanes.Add(srcLane, dstLane);
            }

            // Roadのうち接続でない部分をコピー
            var roads = new Dictionary<RnRoad, RnRoad>();
            var roadBases = new Dictionary<RnRoadBase, RnRoadBase>();
            foreach (var srcRoad in src.Roads)
            {
                var dstRoad = new RnRoad(srcRoad.TargetTrans);
                foreach (var srcLane in srcRoad.MainLanes)
                {
                    var dstLane = lanes[srcLane];
                    dstRoad.AddMainLane(dstLane);
                }

                if (srcRoad.MedianLane != null)
                {
                    var dstMedianLane = lanes[srcRoad.MedianLane];
                    dstRoad.SetMedianLane(dstMedianLane);
                }
                roads.Add(srcRoad, dstRoad);
                roadBases.Add(srcRoad, dstRoad);
            }
            
            // LaneのParentをコピー
            foreach(var (srcLane, dstLane) in lanes)
            {
                dstLane.Parent = roads[srcLane.Parent];
            }
            
            
            // Intersectionのうち接続でない部分をコピー
            var inters = new Dictionary<RnIntersection, RnIntersection>();
            foreach(var srcInter in src.Intersections)
            {
                var dstInter = new RnIntersection(srcInter.TargetTrans);
                dstInter.SetIsEmptyIntersection(srcInter.IsEmptyIntersection);
                
                inters.Add(srcInter, dstInter);
                roadBases.Add(srcInter, dstInter);
            }
            
            // Intersection Edgeのコピー
            var neighbors = new Dictionary<RnNeighbor, RnNeighbor>();
            foreach (var srcEdge in src.Intersections.SelectMany(i => i.Edges))
            {
                var dstEdge = new RnNeighbor();
                dstEdge.Border = ways[srcEdge.Border];
                dstEdge.Road = srcEdge.Road == null || !roadBases.ContainsKey(srcEdge.Road) ? null : roadBases[srcEdge.Road];
                neighbors.Add(srcEdge, dstEdge);
            }

            // Trackのコピー
            var tracks = new Dictionary<RnTrack, RnTrack>();
            foreach (var srcTrack in src.Intersections.SelectMany(i => i.Tracks))
            {
                var dstSpline = new Spline();
                dstSpline.Copy(srcTrack.Spline);
                var dstTrack = new RnTrack(ways[srcTrack.FromBorder], ways[srcTrack.ToBorder], dstSpline,
                    srcTrack.TurnType);
                tracks.Add(srcTrack, dstTrack);
            }

            // Intersectionのうち接続部分をコピー
            foreach (var (srcInter, dstInter) in inters)
            {
                // edges
                List<RnNeighbor> dstEdges = new();
                foreach (var srcEdge in srcInter.Edges)
                {
                    dstEdges.Add(neighbors[srcEdge]);
                }
                dstInter.ReplaceEdges(dstEdges);
                
                //tracks
                foreach (var srcTrack in srcInter.Tracks)
                {
                    dstInter.TryAddOrUpdateTrack(tracks[srcTrack]);
                }
                
            }
            
            // Roadのうち接続部分をコピー
            foreach (var (srcRoad, dstRoad) in roads)
            {
                var prev = srcRoad.Prev == null ? null : roadBases[srcRoad.Prev];
                var next = srcRoad.Next == null ? null : roadBases[srcRoad.Next];
                dstRoad.SetPrevNext(prev, next);
            }
            
            // Sidewalkのコピー
            var sidewalks = new Dictionary<RnSideWalk, RnSideWalk>();
            foreach(var srcSidewalk in roadBases.Keys.SelectMany(rb => rb.SideWalks))
            {
                var dstParentRoad = srcSidewalk.ParentRoad == null ? null : roadBases[srcSidewalk.ParentRoad];
                var dstOutside = srcSidewalk.OutsideWay == null ? null : ways[srcSidewalk.OutsideWay];
                var dstInside = srcSidewalk.InsideWay == null ? null : ways[srcSidewalk.InsideWay];
                var dstStartEdge = srcSidewalk.StartEdgeWay == null ? null : ways[srcSidewalk.StartEdgeWay];
                var dstEndEdge = srcSidewalk.EndEdgeWay == null ? null : ways[srcSidewalk.EndEdgeWay];
                var laneType = srcSidewalk.LaneType;
                var dstSidewalk = new RnSideWalk();
                dstSidewalk.SetParent(dstParentRoad);
                dstSidewalk.SetSideWays(dstOutside, dstInside);
                dstSidewalk.SetStartEdgeWay(dstStartEdge);
                dstSidewalk.SetEndEdgeWay(dstEndEdge);
                dstSidewalk.LaneType = laneType;
                sidewalks.Add(srcSidewalk, dstSidewalk);
            }
            
            // Sidewalkの代入
            foreach(var (srcRoadBase, dstRoadBase) in roadBases)
            {
                foreach (var srcSidewalk in srcRoadBase.SideWalks)
                {
                    dstRoadBase.AddSideWalk(sidewalks[srcSidewalk]);
                }
            }
            
            
            // 道路ネットワークのコピー
            var dst = new RnModel();
            foreach (var dstRoadBase in roadBases.Values)
            {
                dst.AddRoadBase(dstRoadBase);
            }

            outRoadSrcToDst = roadBases;

            return dst;
        }
    }
}