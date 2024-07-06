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

        // 道路か歩道かどうか
        [field: SerializeField]
        public bool IsRoad { get; set; }

        public bool visible = true;

        public RoadNetworkTranMesh(PLATEAUCityObjectGroup cityObjectGroup, bool isRoad, int lodLevel, List<Vector3> vertices)
        {
            CityObjectGroup = cityObjectGroup;
            IsRoad = isRoad;
            LodLevel = lodLevel;
            Vertices = vertices;
        }
    }
}