using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// ��ʋK�����
    /// </summary>
    [Serializable, RoadNetworkSerializeData(typeof(TrafficRegulationInfoData))]
    public class TrafficRegulationInfoData : IPrimitiveData
    {
        /// <summary>
        /// ��~��
        /// </summary>
        public StopLine stopLine;
        /// <summary>
        /// ���x����
        /// </summary>
        public float speedLimit;
        /// <summary>
        /// �D�擹�H
        /// </summary>
        public RnID<RoadNetworkDataLineString> yields;
    }

    /// <summary>
    /// ��~��
    /// </summary>
    [System.Serializable]
    public class StopLine
    {
        public RnID<RoadNetworkDataLineString> line;
        public TrafficLight trafficLight;
        public bool bHasStopSign;
    }

    /// <summary>
    /// �M���@�̓d���i�T�O�j
    /// ���̐M���@�͉��F�ł����ƕ����ꂽ�炱���Ԃ�
    /// </summary>
    [System.Serializable]
    public class TrafficLight
    {
        public LightBulb[] LightBulbs => lightBulbs;
        [SerializeField] private LightBulb[] lightBulbs;
    }

    /// <summary>
    /// �M���@�̓d��
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
