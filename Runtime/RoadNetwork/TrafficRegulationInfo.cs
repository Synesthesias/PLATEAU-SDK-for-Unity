using PLATEAU.RoadNetwork.Data;
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

    /// <summary>
    /// 信号制御器
    /// </summary>
    public class TrafficSignalLightController
    {
        public TrafficSignalLightController(string id, RoadNetworkNode node, in Vector3 position)
        {
            SelfId = id;
            CorrespondingNode = node;
            this.position = position;
        }

        // RnIDに変換予定？　AVNEWだと文字列
        public string SelfId { get; private set; } = string.Empty;
        public RoadNetworkNode CorrespondingNode { get; private set; } = null;


        public bool GapImpedanceFlag { get; private set; } = false;
        public TrafficSignalControlPattern ControlPatternData { get; private set; } = new TrafficSignalControlPattern();
        public List<TrafficSignalLight> SignalLights { get; private set; } = new List<TrafficSignalLight>();

        public Vector3 position;
    }

    /// <summary>
    /// 信号灯器
    /// </summary>
    public class TrafficSignalLight
    {
        public TrafficSignalLight(TrafficSignalLightController controller, in Vector3 position)
        {
            this.controller = controller;
            this.position = position;
        }

        public void SetStatus(TrafficSignalLightBulb.Status status)
        {

        }

        /// <summary>
        /// 対応する停止線
        /// </summary>
        public StopLine stopLine;

        public Vector3 position;

        TrafficSignalLightController controller;
    }

    public class TrafficSignalControlPattern
    {
        public DateTime StartTime { get; private set; } = DateTime.MinValue;
        public string ControlPatternId { get; private set; } = "_undefind";
        public List<TrafficSignalPhase> Phases { get; private set; } = new List<TrafficSignalPhase>();
        public TrafficLightSignalOffset Offset { get; private set; } = null;
    }

    public class TrafficSignalPhase
    {
        public float SplitSeconds { get; set; }
        public int EnterableVehicleType { get; set; }
        public Dictionary<TrafficSignalLight, RoadNetworkLink> DirectionMap { get; set; }
    }

    /// <summary>
    /// 信号灯器の点灯をずらす
    /// </summary>
    public class TrafficLightSignalOffset
    {
        public TrafficSignalLight ReferenceSignalLight { get; set; }
        public float Seconds { get; set; }
    }

    /////////////////////////////////////////
    
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
        public TrafficSignalLight trafficLight;
        public bool bHasStopSign;
    }

    /// <summary>
    /// 信号機の電球（概念）
    /// 信号機は何色ですかと聞かれたらこれを返す
    /// クラス名がTrafficSignalLightと混じって分かりずらいので変えたい
    /// </summary>
    [System.Serializable]
    public class TrafficSignalLightBulb
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
