using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork.Data
{

    [Serializable]
    public class RoadNetworkDataLane : IPrimitiveData
    {
        // 自分自身を表すId
        [SerializeField] private RnLaneId myId;

        // 構成する頂点
        // ポリゴン的に時計回りの順に格納されている
        [SerializeField]
        private List<RnPointId> vertices = new List<RnPointId>();

        [SerializeField]
        private RnLineStringId leftWayId = RnLineStringId.Undefined;

        [SerializeField]
        private RnLineStringId rightWayId = RnLineStringId.Undefined;

        // 連結しているレーン
        [SerializeField] private SerializableHashSet<RnLaneId> nextLaneIds = new SerializableHashSet<RnLaneId>();

        [SerializeField] private SerializableHashSet<RnLaneId> prevLaneIds = new SerializableHashSet<RnLaneId>();

        // 他レーンとの境界線
        [SerializeField]
        private List<RnLineStringId> borderIds = new List<RnLineStringId>();

        // 中央線
        [SerializeField]

        public List<Vector3> centerLine = new List<Vector3>();

        public RnLaneId MyId => myId;

        // 孤立状態(どこともつながっていない)
        public bool IsIsolated => borderIds.Count == 0;

        // 不完全な状態かどうか
        public bool isPartial = false;


        public RoadNetworkDataLane(RnLaneId myId)
        {
            this.myId = myId;
        }
    }

}