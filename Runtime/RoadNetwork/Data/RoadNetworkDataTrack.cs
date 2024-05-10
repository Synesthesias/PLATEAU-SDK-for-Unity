using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkTrack))]
    public class RoadNetworkDataTrack : IPrimitiveData
    {
        //識別用ID：TrackID
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkTrack.MyId))]

        public RnID<RoadNetworkDataTrack> MyId { get; set; }

        //接続上流リンク識別用ID：LinkID
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkTrack.NextLink))]

        public RnID<RoadNetworkDataLink> NextLink { get; set; }

        //接続上流レーン識別用ID：LaneID
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkTrack.NextLane))]

        public RnID<RoadNetworkDataLane> NextLane { get; set; }

        //接続上流リンク下流端からの距離[m] : double
        //public double distanceFromNextLink;

        //接続下流リンク識別用ID : LinkID

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkTrack.PrevLink))]

        public RnID<RoadNetworkDataLink> PrevLink { get; set; }

        //接続下流レーン識別用ID: LaneID

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkTrack.PrevLane))]

        public RnID<RoadNetworkDataLane> PrevLane { get; set; }
        //接続下流リンク下流端からの距離[m] : double
        //public double distanceFromPrevLink;

        //トラック長[m] : double
        //public double length;

        //リンク容量[pcu / h / lane]: double
        //自由流速度[km / h] : double
        //ジャム密度[台 / km] : double
        //飽和交通流率[pcu / 青1h / lane] : double
        //上流接続部から下流接続部をつなぐ補間点 : LineStrings

    }
}