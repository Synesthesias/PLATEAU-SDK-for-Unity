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
            BuildVertices(epsilon);
        }

        public bool BuildVertices(float epsilon = 0.1f)
        {
            if (!CityObjectGroup)
                return false;
            var mesh = CityObjectGroup.GetComponent<MeshCollider>();
            if (!mesh)
                return false;
            Vertices = GeoGraph2D.ComputeMeshOutlineVertices(mesh.sharedMesh, v => v.Xz(), epsilon);
            return true;
        }
    }
}