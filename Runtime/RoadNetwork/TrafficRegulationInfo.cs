﻿using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 交通規制の生成補助クラス
    /// </summary>
    public class TrafficRegulationInfoCreateHelper
    {

    }

    public class SignalController
    {
        public List<SignalLight> signalLights = new List<SignalLight>();
        public List<SignalStepSet> signalStepSets = new List<SignalStepSet>();

        public Vector3 position;
    }

    public class SignalLight
    {
        public void SetStatus(TrafficLight.Status status)
        {

        }

        /// <summary>
        /// 対応する停止線
        /// </summary>
        public StopLine stopLine;

        public Vector3 position;
    }

    public class SignalStepSet 
    {
        public float TimeOffset = 0;
        public List<SignalStep> steps;
    }

    public class SignalStep
    {
        public float DuringStep = 0;

        public struct SignalPattern
        {
            TrafficLight.Status status;
            List<SignalLight> lights;
        }
        public List<SignalPattern> patterns;
    }

    /// <summary>
    /// 交通規制情報
    /// </summary>
    public class TrafficRegulationInfo
    {
        /// <summary>
        /// 停止線
        /// </summary>
        public StopLine stopLine;
        /// <summary>
        /// 速度制限
        /// </summary>
        public float speedLimit;
        /// <summary>
        /// 優先道路
        /// </summary>
        public RnID<RoadNetworkDataLineString> yields;
    }

    /// <summary>
    /// 停止線
    /// </summary>
    [System.Serializable]
    public class StopLine
    {
        public RoadNetworkDataLineString line;
        public TrafficLight trafficLight;
        public bool bHasStopSign;
    }

    /// <summary>
    /// 信号機の電球（概念）
    /// 信号機は何色ですかと聞かれたらこれを返す
    /// </summary>
    [System.Serializable]
    public class TrafficLight
    {
        public LightBulb[] LightBulbs => lightBulbs;
        [SerializeField] private LightBulb[] lightBulbs;

        public enum Status
        {
            // 0
            Undefind = 0,

            // 0x0000000X
            Stop            = 0x0001,
            Attention       = 0x00000002,
            Go              = 0x0003,

            // 0x000000X0
            Flashing        = 0x00000010,

            // 0x00000X00
            BlueArrow       = 0x000000100,
            YellowArrow     = 0x000000200,

            //...

            // 0x0X000000
            UserDefind1 = 0x01000000,
            UserDefind2 = 0x02000000,
            UserDefind3 = 0x03000000,
            UserDefind4 = 0x04000000,
            UserDefind5 = 0x05000000,
            UserDefind6 = 0x06000000,
            //...
        }
    }

    /// <summary>
    /// 信号機の電球
    /// </summary>
    [System.Serializable]
    public class LightBulb
    {
        public BulbType Type => type;

        public BulbColor Color => color;

        public BulbStatus Status => status;

        [SerializeField] private BulbType type;
        [SerializeField] private BulbColor color;
        [SerializeField] private BulbStatus status;

        public LightBulb(BulbType type, BulbColor color, BulbStatus status)
        {
            this.type = type;
            this.color = color;
            this.status = status;
        }

        /// <summary>
        /// Type of each bulb.
        /// </summary>
        public enum BulbType
        {
            ANY_CIRCLE_BULB = 0,
            RED_BULB = 1,
            YELLOW_BULB = 2,
            GREEN_BULB = 3,
            LEFT_ARROW_BULB = 4,
            RIGHT_ARROW_BULB = 5,
            UP_ARROW_BULB = 6,
            DOWN_ARROW_BULB = 7,
            DOWN_LEFT_ARROW_BULB = 8,
            DOWN_RIGHT_ARROW_BULB = 9,
            CROSS_BULB = 10,
        }

        /// <summary>
        /// Bulb lighting status.
        /// </summary>
        public enum BulbStatus
        {
            SOLID_OFF = 0,        // Lights off.
            SOLID_ON = 1,        // Lights on.
            FLASHING = 2,        // Lights on every flashSec.
        }

        /// <summary>
        /// Bulb lighting color.
        /// </summary>
        public enum BulbColor
        {
            RED = 0,
            YELLOW = 1,
            GREEN = 2,
            WHITE = 3,
        }
    }
}
