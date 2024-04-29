using PLATEAU.Runtime.RoadNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 交通規制情報
    /// </summary>
    [System.Serializable]
    public struct TrafficRegulationInfo
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
        public RoadNetworkID<RoadNetworkLineString> yields;
    }

    [System.Serializable]
    public struct StopLine
    {
        public RoadNetworkID<RoadNetworkLineString> line;
        public TrafficLight trafficLight;
        public bool bHasStopSign;
    }

    [System.Serializable]
    public struct TrafficLight
    {
        public LightBulb[] LightBulbs => lightBulbs;
        [SerializeField] private LightBulb[] lightBulbs;
    }


    [System.Serializable]
    public struct LightBulb
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
