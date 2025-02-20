﻿using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnTrack))]
    public class RnDataTrack
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataWay> FromBorder { get; set; }

        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataWay> ToBorder { get; set; }

        // スプライン
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public Spline Spline { get; set; }

        /// <summary>
        /// 曲がり具合
        /// </summary>
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnTurnType TurnType { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }

    [Serializable, RoadNetworkSerializeData(typeof(RnIntersectionEdge))]
    public class RnDataIntersectionEdge
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 他レーンとの境界線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnIntersectionEdge.Border))]
        public RnID<RnDataWay> Border { get; set; }

        // Road
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Road { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }

    [Serializable, RoadNetworkSerializeData(typeof(RnIntersection))]
    public class RnDataIntersection : RnDataRoadBase
    {
        // 隣接情報
        [field: SerializeField]
        [RoadNetworkSerializeMember("edges")]
        public List<RnDataIntersectionEdge> Edges { get; set; } = new List<RnDataIntersectionEdge>();

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public bool IsEmptyIntersection { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataTrafficLightController> SignalController { get; set; }


        public IEnumerable<RnDataIntersectionEdge> Neighbors => Edges.Where(e => e.Road.IsValid);

        // 車線
        [field: SerializeField, RoadNetworkSerializeMember("tracks")]
        public List<RnDataTrack> Tracks { get; set; } = new();
    }
}