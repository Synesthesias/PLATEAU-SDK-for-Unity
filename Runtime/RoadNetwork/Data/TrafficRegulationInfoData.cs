using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// 交通規制情報
    /// </summary>
    [Serializable, RoadNetworkSerializeData(typeof(TrafficRegulationInfoData))]
    public class TrafficRegulationInfoData : IPrimitiveData
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
        public RnID<RoadNetworkDataLineString> line;
        public TrafficLight trafficLight;
        public bool bHasStopSign;
    }

    /// <summary>
    /// 信号機の電球（概念）
    /// 今の信号機は何色ですかと聞かれたらこれを返す
    /// </summary>
    [System.Serializable]
    public class TrafficLight
    {
        public LightBulb[] LightBulbs => lightBulbs;
        [SerializeField] private LightBulb[] lightBulbs;
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
