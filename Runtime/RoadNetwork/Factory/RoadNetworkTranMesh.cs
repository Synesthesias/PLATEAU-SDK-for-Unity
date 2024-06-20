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
        public List<Vector3> Vertices { get; set; }


        public RoadNetworkTranMesh(PLATEAUCityObjectGroup cityObjectGroup, float epsilon = 0.1f)
        {
            CityObjectGroup = cityObjectGroup;
            Vertices = BuildVertices(cityObjectGroup, epsilon);
        }

        public RoadNetworkTranMesh(PLATEAUCityObjectGroup cityObjectGroup, List<Vector3> vertices)
        {
            CityObjectGroup = cityObjectGroup;
            Vertices = vertices;
        }

        private static List<Vector3> BuildVertices(PLATEAUCityObjectGroup cityObjectGroup, float epsilon = 0.1f)
        {
            if (!cityObjectGroup)
                return new List<Vector3>();
            var mesh = cityObjectGroup.GetComponent<MeshCollider>();
            if (!mesh)
                return new List<Vector3>();
            return GeoGraph2D.ComputeMeshOutlineVertices(mesh.sharedMesh, v => v.Xz(), epsilon);
        }
    }
}