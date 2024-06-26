using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 交差点
    /// </summary>
    public class RoadNetworkNode : ARoadNetworkParts<RoadNetworkNode>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RoadNetworkModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 隣接情報
        public List<RoadNetworkNeighbor> Neighbors { get; set; } = new List<RoadNetworkNeighbor>();


        private List<RoadNetworkTrack> tracks = new List<RoadNetworkTrack>();

        // 車線
        public IReadOnlyList<RoadNetworkTrack> Tracks => tracks;

        // 信号制御器
        public TrafficSignalLightController SignalController { get; set; } = null;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public RoadNetworkNode() { }

        public RoadNetworkNode(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        public void AddTrack(RoadNetworkTrack track)
        {
            if (tracks.Contains(track))
                return;

            track.ParentNode = this;
            tracks.Add(track);
        }

        public void AddTracks(IEnumerable<RoadNetworkTrack> tracks)
        {
            foreach (var track in tracks)
                AddTrack(track);
        }

        public void RemoveTrack(RoadNetworkTrack link)
        {
            if (tracks.Remove(link))
                link.ParentNode = null;
        }

        public Vector3 GetCenterPoint()
        {
            var ret = Neighbors.SelectMany(n => n.Border.Vertices).Aggregate(Vector3.zero, (a, b) => a + b);
            var cnt = Neighbors.Sum(n => n.Border.Count);
            return ret / cnt;
        }

        /// <summary>
        /// a,bを繋ぐ経路を計算する
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public RoadNetworkTrack CalcTrackWay(RoadNetworkLink a, RoadNetworkLink b)
        {
            if (a == b)
                return null;
            var na = Neighbors.FirstOrDefault(n => n.Link == a);
            var nb = Neighbors.FirstOrDefault(n => n.Link == b);
            if (na == null || nb == null)
                return null;
            if (na.Border.Count < 2 || nb.Border.Count < 2)
                return null;

            var aStart = na.Border.GetPoint(0);
            var aEnd = na.Border.GetPoint(na.Border.Count - 1);

            var aLane = na.GetConnectedLane();
            if (aLane == null)
                return null;

            var bLane = nb.GetConnectedLane();
            if (bLane == null)
                return null;


            aLane.TryGetBorderNormal(na.Border, out var aLeftPos, out var aLeftNormal, out var aRightPos, out var aRightNormal);
            bLane.TryGetBorderNormal(nb.Border, out var bLeftPos, out var bLeftNormal, out var bRightPos, out var bRightNormal);

            var rightSp = new Spline
            {
                new BezierKnot(aLeftPos, 10 * aLeftNormal, 10 *aLeftNormal),
                new BezierKnot(bRightPos, 10 *bRightNormal, 10 *bRightNormal)
            };

            var leftSp = new Spline
            {
                new BezierKnot(aRightPos, 10 *aRightNormal, 10 *aRightNormal),
                new BezierKnot(bLeftPos, 10 *bLeftNormal, 10 *bLeftNormal)
            };
            DebugEx.DrawArrow(aLeftPos, aLeftPos + aLeftNormal * 2, bodyColor: Color.magenta);
            DebugEx.DrawArrow(bLeftPos, bLeftPos + bLeftNormal * 2, bodyColor: Color.magenta);
            DebugEx.DrawArrow(bRightPos, bRightPos + bRightNormal * 2, bodyColor: Color.magenta);
            DebugEx.DrawArrow(aRightPos, aRightPos + aRightNormal * 2, bodyColor: Color.magenta);
            //var bStart = nb.Border.GetPoint(0);
            //var bEnd = nb.Border.GetPoint(nb.Border.Count - 1);

            //var line1 = new LineSegment2D(aStart.Vertex.Xz(), bStart.Vertex.Xz());
            //var line2 = new LineSegment2D(aEnd.Vertex.Xz(), bEnd.Vertex.Xz());
            //if (line1.TrySegmentIntersection(line2))
            //{
            //    (aStart, bStart) = (bStart, aStart);
            //    line1 = new LineSegment2D(aStart.Vertex.Xz(), bStart.Vertex.Xz());
            //    line2 = new LineSegment2D(aEnd.Vertex.Xz(), bEnd.Vertex.Xz());
            //}

            var rates = Enumerable.Range(0, 10).Select(i => 1f * i / (9)).ToList();
            var leftWay = new RoadNetworkWay(RoadNetworkLineString.Create(rates.Select(t =>
            {
                leftSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
            var rightWay = new RoadNetworkWay(RoadNetworkLineString.Create(rates.Select(t =>
            {
                rightSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
            return new RoadNetworkTrack(leftWay, rightWay);
        }
    }
}