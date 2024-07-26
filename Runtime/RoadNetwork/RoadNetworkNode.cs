using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }
}