using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 交差点
    /// </summary>
    public class RoadNetworkNode
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnID<RoadNetworkDataNode> MyId { get; set; }

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

            var bStart = nb.Border.GetPoint(0);
            var bEnd = nb.Border.GetPoint(nb.Border.Count - 1);

            var line1 = new LineSegment2D(aStart.Vertex.Xz(), bStart.Vertex.Xz());
            var line2 = new LineSegment2D(aEnd.Vertex.Xz(), bEnd.Vertex.Xz());
            if (line1.TrySegmentIntersection(line2))
            {
                (aStart, bStart) = (bStart, aStart);
                line1 = new LineSegment2D(aStart.Vertex.Xz(), bStart.Vertex.Xz());
                line2 = new LineSegment2D(aEnd.Vertex.Xz(), bEnd.Vertex.Xz());
            }

            var leftWay = new RoadNetworkWay(RoadNetworkLineString.Create(new[] { aStart, bStart }));
            var rightWay = new RoadNetworkWay(RoadNetworkLineString.Create(new[] { aEnd, bEnd }));
            return new RoadNetworkTrack(leftWay, rightWay);
        }
    }
}