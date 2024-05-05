using System;

namespace PLATEAU.RoadNetwork.Data
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
        public static readonly RnLineStringId Undefined = new RnLineStringId(RoadNetworkID<RoadNetworkDataLineString>.Undefined);
        private readonly RoadNetworkID<RoadNetworkDataLineString> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnLineStringId -> RoadNetworkID<RoadNetworkLineString>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkDataLineString>(RnLineStringId id) => id.id;

        // RoadNetworkID<RoadNetworkLineString> -> RnLineStringIdの型変換
        public static implicit operator RnLineStringId(RoadNetworkID<RoadNetworkDataLineString> id) => new(id);

        public RnLineStringId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkDataLineString>(id);
        }

        public RnLineStringId(RoadNetworkID<RoadNetworkDataLineString> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnLinkId
    {
        public static readonly RnLinkId Undefined = new RnLinkId(RoadNetworkID<RoadNetworkDataLink>.Undefined);
        private readonly RoadNetworkID<RoadNetworkDataLink> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnLinkId -> RoadNetworkID<RoadNetworkLink>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkDataLink>(RnLinkId id) => id.id;

        // RoadNetworkID<RoadNetworkLink> -> RnLinkIdの型変換
        public static implicit operator RnLinkId(RoadNetworkID<RoadNetworkDataLink> id) => new(id);

        public RnLinkId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkDataLink>(id);
        }

        public RnLinkId(RoadNetworkID<RoadNetworkDataLink> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnBlockId
    {
        public static readonly RnBlockId Undefined = new RnBlockId(RoadNetworkID<RoadNetworkDataBlock>.Undefined);
        private readonly RoadNetworkID<RoadNetworkDataBlock> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnBlockId -> RoadNetworkID<RoadNetworkBlock>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkDataBlock>(RnBlockId id) => id.id;

        // RoadNetworkID<RoadNetworkBlock> -> RnBlockIdの型変換
        public static implicit operator RnBlockId(RoadNetworkID<RoadNetworkDataBlock> id) => new(id);

        public RnBlockId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkDataBlock>(id);
        }

        public RnBlockId(RoadNetworkID<RoadNetworkDataBlock> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnTrackId
    {
        public static readonly RnTrackId Undefined = new RnTrackId(RoadNetworkID<RoadNetworkDataTrack>.Undefined);
        private readonly RoadNetworkID<RoadNetworkDataTrack> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnTrackId -> RoadNetworkID<RoadNetworkTrack>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkDataTrack>(RnTrackId id) => id.id;

        // RoadNetworkID<RoadNetworkTrack> -> RnTrackIdの型変換
        public static implicit operator RnTrackId(RoadNetworkID<RoadNetworkDataTrack> id) => new(id);

        public RnTrackId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkDataTrack>(id);
        }

        public RnTrackId(RoadNetworkID<RoadNetworkDataTrack> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnNodeId
    {
        public static readonly RnNodeId Undefined = new RnNodeId(RoadNetworkID<RoadNetworkDataNode>.Undefined);
        private readonly RoadNetworkID<RoadNetworkDataNode> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnNodeId -> RoadNetworkID<RoadNetworkNode>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkDataNode>(RnNodeId id) => id.id;

        // RoadNetworkID<RoadNetworkNode> -> RnNodeIdの型変換
        public static implicit operator RnNodeId(RoadNetworkID<RoadNetworkDataNode> id) => new(id);

        public RnNodeId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkDataNode>(id);
        }

        public RnNodeId(RoadNetworkID<RoadNetworkDataNode> id)
        {
            this.id = id;
        }
    }

    public readonly struct RnLaneId
    {
        public static readonly RnLaneId Undefined = new RnLaneId(RoadNetworkID<RoadNetworkDataLane>.Undefined);
        private readonly RoadNetworkID<RoadNetworkDataLane> id;

        public bool IsValid => id.IsValid;

        public int Id => id.Id;

        // RnLaneId -> RoadNetworkID<RoadNetworkDataLane>の型変換
        public static implicit operator RoadNetworkID<RoadNetworkDataLane>(RnLaneId id) => id.id;

        // RoadNetworkID<RoadNetworkDataLane> -> RnLaneIdの型変換
        public static implicit operator RnLaneId(RoadNetworkID<RoadNetworkDataLane> id) => new(id);

        public RnLaneId(int id)
        {
            this.id = new RoadNetworkID<RoadNetworkDataLane>(id);
        }

        public RnLaneId(RoadNetworkID<RoadNetworkDataLane> id)
        {
            this.id = id;
        }
    }
}