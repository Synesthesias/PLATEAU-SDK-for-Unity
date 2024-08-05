using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.RoadNetwork.Factory
{
    [Serializable]
    public class RoadNetworkTranMesh
    {
        [field: SerializeField]
        public PLATEAUCityObjectGroup CityObjectGroup { get; set; }

        [field: SerializeField]
        public int LodLevel { get; set; }

        [field: SerializeField]
        public List<Vector3> Vertices { get; set; }

        [field: SerializeField]
        public RRoadTypeMask RoadType { get; set; }

        public bool visible = true;

        public RoadNetworkTranMesh(PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask roadType, int lodLevel, List<Vector3> vertices)
        {
            CityObjectGroup = cityObjectGroup;
            RoadType = roadType;
            LodLevel = lodLevel;
            Vertices = vertices;
        }
    }
}