using PLATEAU.RoadNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.Runtime.RoadNetwork
{
    public class _RoadNetworkLane : MonoBehaviour
    {
        public LaneShapeInfo laneShapeInfo;
    }

    public struct LaneShapeInfo
    {
        public LineStrings lineStrings;
    }

    public struct Way
    {
        public Node node;
    }

    public struct Node
    {
        public Vector3 position;
    }
}
