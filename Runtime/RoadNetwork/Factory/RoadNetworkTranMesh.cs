using PLATEAU.CityInfo;
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

        public bool visible = true;

        public RoadNetworkTranMesh(PLATEAUCityObjectGroup cityObjectGroup, int lodLevel, List<Vector3> vertices)
        {
            CityObjectGroup = cityObjectGroup;
            LodLevel = lodLevel;
            Vertices = vertices;
        }
    }
}