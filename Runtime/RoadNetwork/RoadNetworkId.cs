using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public struct RoadNetworkID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {
        // 不正値
        public static RoadNetworkID<TPrimDataType> Undefined => new RoadNetworkID<TPrimDataType>(-1);

        // Listのindexアクセスがintなのでuintじゃなくてintにしておく
        // structなので初期値は基本0. その時に不正値扱いにするために0は不正値とする
        private int id;

        public int Id => id - 1;

        // 有効なIdかどうか
        public bool IsValid => id > 0;

        // int型への暗黙の型変換
        public static implicit operator int(RoadNetworkID<TPrimDataType> id) => id.Id;

        public RoadNetworkID(int id)
        {
            this.id = id + 1;
        }
    }
    /*
     * 使う側でいちいちGenericで指定するの長すぎて辛いので別名定義用
     * C#10.0じゃないとglobal usingが使えないので
     */

    public readonly struct RnPointId
    {
        public static readonly RnPointId Undefined = new RnPointId(RoadNetworkID<RoadNetworkPoint>.Undefined);
        private readonly RoadNetworkID<RoadNetworkPoint> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // int型への暗黙の型変換
        public static implicit operator int(RnPointId id) => id.Id;

        // RnPointId -> RoadNetworkID<RoadNetworkPoint>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkPoint>(RnPointId id) => id.id;

        // RoadNetworkID<RoadNetworkPoint> -> RnPointIdの型変換
        public static implicit operator RnPointId(RoadNetworkID<RoadNetworkPoint> id) => new(id);

        public RnPointId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkPoint>(id);
        }

        public RnPointId(RoadNetworkID<RoadNetworkPoint> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnLineStringId
    {
        public static readonly RnLineStringId Undefined = new RnLineStringId(RoadNetworkID<RoadNetworkLineString>.Undefined);
        private readonly RoadNetworkID<RoadNetworkLineString> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnLineStringId -> RoadNetworkID<RoadNetworkLineString>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkLineString>(RnLineStringId id) => id.id;

        // RoadNetworkID<RoadNetworkLineString> -> RnLineStringIdの型変換
        public static implicit operator RnLineStringId(RoadNetworkID<RoadNetworkLineString> id) => new(id);

        public RnLineStringId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkLineString>(id);
        }

        public RnLineStringId(RoadNetworkID<RoadNetworkLineString> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnLinkId
    {
        public static readonly RnLinkId Undefined = new RnLinkId(RoadNetworkID<RoadNetworkLink>.Undefined);
        private readonly RoadNetworkID<RoadNetworkLink> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnLinkId -> RoadNetworkID<RoadNetworkLink>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkLink>(RnLinkId id) => id.id;

        // RoadNetworkID<RoadNetworkLink> -> RnLinkIdの型変換
        public static implicit operator RnLinkId(RoadNetworkID<RoadNetworkLink> id) => new(id);

        public RnLinkId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkLink>(id);
        }

        public RnLinkId(RoadNetworkID<RoadNetworkLink> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnBlockId
    {
        public static readonly RnBlockId Undefined = new RnBlockId(RoadNetworkID<RoadNetworkBlock>.Undefined);
        private readonly RoadNetworkID<RoadNetworkBlock> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnBlockId -> RoadNetworkID<RoadNetworkBlock>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkBlock>(RnBlockId id) => id.id;

        // RoadNetworkID<RoadNetworkBlock> -> RnBlockIdの型変換
        public static implicit operator RnBlockId(RoadNetworkID<RoadNetworkBlock> id) => new(id);

        public RnBlockId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkBlock>(id);
        }

        public RnBlockId(RoadNetworkID<RoadNetworkBlock> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnTrackId
    {
        public static readonly RnTrackId Undefined = new RnTrackId(RoadNetworkID<RoadNetworkTrack>.Undefined);
        private readonly RoadNetworkID<RoadNetworkTrack> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnTrackId -> RoadNetworkID<RoadNetworkTrack>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkTrack>(RnTrackId id) => id.id;

        // RoadNetworkID<RoadNetworkTrack> -> RnTrackIdの型変換
        public static implicit operator RnTrackId(RoadNetworkID<RoadNetworkTrack> id) => new(id);

        public RnTrackId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkTrack>(id);
        }

        public RnTrackId(RoadNetworkID<RoadNetworkTrack> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnNodeId
    {
        public static readonly RnNodeId Undefined = new RnNodeId(RoadNetworkID<RoadNetworkNode>.Undefined);
        private readonly RoadNetworkID<RoadNetworkNode> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnNodeId -> RoadNetworkID<RoadNetworkNode>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkNode>(RnNodeId id) => id.id;

        // RoadNetworkID<RoadNetworkNode> -> RnNodeIdの型変換
        public static implicit operator RnNodeId(RoadNetworkID<RoadNetworkNode> id) => new(id);

        public RnNodeId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkNode>(id);
        }

        public RnNodeId(RoadNetworkID<RoadNetworkNode> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnLaneId
    {
        public static readonly RnLaneId Undefined = new RnLaneId(RoadNetworkID<RoadNetworkLane>.Undefined);
        private readonly RoadNetworkID<RoadNetworkLane> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnLaneId -> RoadNetworkID<RoadNetworkLane>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkLane>(RnLaneId id) => id.id;

        // RoadNetworkID<RoadNetworkLane> -> RnLaneIdの型変換
        public static implicit operator RnLaneId(RoadNetworkID<RoadNetworkLane> id) => new(id);

        public RnLaneId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkLane>(id);
        }

        public RnLaneId(RoadNetworkID<RoadNetworkLane> id)
        {
            this.id = id;
        }
    }
}