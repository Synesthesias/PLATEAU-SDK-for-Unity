using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
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
    [Serializable]
    public class TrafficSignalLightController : ARnParts<TrafficSignalLightController>
    {
        /// <summary>
        /// デシリアライズ用
        /// </summary>
        public TrafficSignalLightController() { }

        public TrafficSignalLightController(string id, RnIntersection node, in Vector3 position)
            : base()
        {
            Parent = node;
            //this.Position = position;
        }

        // デバッグ用
        public string DebugId { get=> "t_" + Parent.DebugMyId; }

        // RnIntersection
        public RnRoadBase Parent { get; private set; } = null;

        public RnIntersection CorrespondingNode { get => Parent as RnIntersection; }


        //public bool GapImpedanceFlag { get; private set; } = false;
        public List<TrafficSignalLight> TrafficLights { get; private set; } = new List<TrafficSignalLight>();
        public List<TrafficSignalControllerPattern> SignalPatterns { get; private set; } = new List<TrafficSignalControllerPattern>();

        public Vector3 Position { get => Parent.GetCenter(); }
    }

    /// <summary>
    /// 信号灯器
    /// </summary>
    public class TrafficSignalLight : ARnParts<TrafficSignalLight>
    {
        /// <summary>
        /// デシリアライズ用
        /// </summary>
        public TrafficSignalLight() { }

        public TrafficSignalLight(TrafficSignalLightController controller, in Vector3 position)
        {
            this.Parent = controller;
            //this.position = position;
        }

        public void SetStatus(TrafficSignalLightBulb.Status status)
        {

        }

        public TrafficSignalLightController Parent { get; private set; }

        public RnRoadBase Road { get; private set; }

        public string LaneType { get; private set; }
        public float Distance { get; private set; }

        ///// <summary>
        ///// 対応する停止線
        ///// </summary>
        //public StopLine stopLine;

        public Vector3 Position { get=>Vector3.zero; }

    }

    /// <summary>
    /// 信号制御のパターン
    /// 開始時刻、制御パターンID、フェーズのリスト、信号灯のオフセットを持つ
    /// </summary>
    public class TrafficSignalControllerPattern : ARnParts<TrafficSignalControllerPattern>
    {
        /// <summary>
        /// デシリアライズ用
        /// </summary>
        public TrafficSignalControllerPattern() { }


        public TrafficSignalLightController Parent { get; private set; }
        public List<TrafficSignalControllerPhase> Phases { get; private set; } = new List<TrafficSignalControllerPhase>();
        public float OffsetSeconds { get; private set; } = 0;
        public TrafficSignalLight OffsetTrafficLight { get; private set; } = null;
        public OffsetRelationType OffsetType { get; private set; } = OffsetRelationType.Absolute;
        public DateTime StartOffsets { get; private set; } = DateTime.MinValue;

        public string ControlPatternId { get; private set; } = "_undefind";

    }

    /// <summary>
    /// 信号制御のフェーズ
    /// 信号制御のパターンに含まれる要素
    /// </summary>
    public class TrafficSignalControllerPhase : ARnParts<TrafficSignalControllerPhase>
    {
        /// <summary>
        /// デシリアライズ用
        /// </summary>
        public TrafficSignalControllerPhase() { }

        public TrafficSignalControllerPhase(string id)
        {
            this.Name = id;
        }
        public TrafficSignalControllerPattern Parent { get; private set; }
        public int Order { get; private set; }
        public float Split { get; set; }
        public int EnterableVehicleTypeMask { get; set; }

        /// <summary>
        /// 青信号時に通過出来る道路のID
        /// 注意　交差点のIDではない
        /// </summary>
        public List<RnRoadBase> BlueRoadPairs { get; set; }

        /// <summary>
        /// 青信号時に通過出来る道路のID
        /// 注意　交差点のIDではない
        /// </summary>
        public List<RnRoadBase> YellowRoadPairs { get; set; }

        /// <summary>
        /// 青信号時に通過出来る道路のID
        /// 注意　交差点のIDではない
        /// </summary>
        public List<RnRoadBase> RedRoadPairs { get; set; }

        //public Dictionary<TrafficSignalLight, RnRoad> DirectionMap { get; set; }

        public string Name { get; set; }
    }

    ///// <summary>
    ///// 信号灯器の点灯をずらす
    ///// </summary>
    //public class TrafficLightSignalOffset
    //{
    //    public TrafficSignalLight ReferenceSignalLight { get; set; }
    //    public float Seconds { get; set; }
    //}
    
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
        public RnID<RnDataLineString> yields;
    }

    /// <summary>
    /// 停止線
    /// </summary>
    [System.Serializable]
    public class StopLine
    {
        public RnDataLineString line;
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
