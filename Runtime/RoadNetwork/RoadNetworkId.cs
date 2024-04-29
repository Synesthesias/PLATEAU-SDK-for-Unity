using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public struct RoadNetworkID<TPrimDataType> where TPrimDataType : IPrimitiveData
    {
        // 不正値
        public static RoadNetworkID<TPrimDataType> Undefined => new RoadNetworkID<TPrimDataType>(-1);

        // Listのindexアクセスがintなのでuintじゃなくてintにしておく
        private int id;


        public int Id => id - 1;

        // 
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

        // int型への暗黙の型変換
        public static implicit operator int(RnLineStringId id) => id.Id;
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

        // int型への暗黙の型変換
        public static implicit operator int(RnLinkId id) => id.Id;
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

        // int型への暗黙の型変換
        public static implicit operator int(RnBlockId id) => id.Id;

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

        // int型への暗黙の型変換
        public static implicit operator int(RnTrackId id) => id.Id;
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

        // int型への暗黙の型変換
        public static implicit operator int(RnNodeId id) => id.Id;
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

        // int型への暗黙の型変換
        public static implicit operator int(RnLaneId id) => id.Id;

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