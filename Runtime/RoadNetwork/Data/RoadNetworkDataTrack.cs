namespace PLATEAU.RoadNetwork.Data
{
    public class RoadNetworkDataTrack : IPrimitiveData
    {
        //識別用ID：TrackID
        public RnTrackId id;

        //接続上流リンク識別用ID：LinkID
        public RnLinkId nextLinkId;

        //接続上流レーン識別用ID：LaneID
        public RnLaneId nextLaneId;

        //接続上流リンク下流端からの距離[m] : double
        //public double distanceFromNextLink;

        //接続下流リンク識別用ID : LinkID
        public RnLinkId prevLinkId;
        //接続下流レーン識別用ID: LaneID
        public RnLaneId prevLaneId;
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