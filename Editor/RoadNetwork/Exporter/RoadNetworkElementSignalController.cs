using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    [System.Serializable]
    public class RoadNetworkElementSignalController : RoadNetworkElement
    {
        public static readonly string IDPrefix = "SignalController";

        public RoadNetworkElementNode Node;

        public List<RoadNetworkElementSignalLight> SignalLights = new List<RoadNetworkElementSignalLight>();

        public RoadNetworkElementSignalController OffsetController;

        public int OffsetType;

        public int OffsetValue;

        public List<(string StartTime, List<RoadNetworkElementSignalStep> SignalSteps)> SignalPatterns = new List<(string, List<RoadNetworkElementSignalStep>)>();

        public int PatternIndex;

        //public List<string> StartTime;

        public RoadNetworkElementSignalController(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
        }

        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

        public string GetNode()
        {
            return Node.ID;
        }

        public string GetSignalLights()
        {
            if (SignalLights.Count == 0)
            {
                return "";
            }

            var ret = SignalLights[0].ID;

            for (int i = 1; i < SignalLights.Count; i++)
            {
                ret += ":" + SignalLights[i].ID;
            }

            return ret;
        }

        public int GetPatternNum()
        {
            return SignalPatterns.Count;
        }

        public string GetPatternID()
        {
            if (SignalPatterns.Count == 0 || SignalPatterns[PatternIndex].SignalSteps.Count == 0)
            {
                return "";
            }

            var ret = SignalPatterns[PatternIndex].SignalSteps[0].PatternID;

            for (int i = 1; i < SignalPatterns.Count; i++)
            {
                ret += ":" + SignalPatterns[PatternIndex].SignalSteps[i].PatternID;
            }

            return ret;
        }

        public int GetCycleLen()
        {
            int cycle = 0;

            SignalPatterns[PatternIndex].SignalSteps.ForEach(x => cycle += x.Duration);

            return cycle;
        }

        public int GetPhaseNum()
        {
            return SignalPatterns[PatternIndex].SignalSteps.Count;
        }

        public string GetStartTime()
        {
            if (SignalPatterns.Count == 0)
            {
                return "";
            }

            var ret = SignalPatterns[0].StartTime;

            for (int i = 1; i < SignalPatterns.Count; i++)
            {
                ret += ":" + SignalPatterns[i].StartTime;
            }

            return ret;
        }

        public GeoJSON.Net.Geometry.Position GetGeometory()
        {
            return Node.GetGeometory();
        }
    }
}