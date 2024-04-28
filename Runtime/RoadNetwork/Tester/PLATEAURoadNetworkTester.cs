using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.RoadNetwork
{
    [RequireComponent(typeof(PLATEAURoadNetwork))]
    public class PLATEAURoadNetworkTester : MonoBehaviour
    {
        public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();

        public List<PLATEAUCityObjectGroup> tmp = new List<PLATEAUCityObjectGroup>();

        private PLATEAURoadNetwork Network => GetComponent<PLATEAURoadNetwork>();

        [SerializeField] private bool targetAll = false;

        [SerializeField]
        private PLATEAURoadNetworkDrawerDebug drawer = new PLATEAURoadNetworkDrawerDebug();

        [SerializeField]
        public List<PLATEAUCityObjectGroup> geoTestTargets = new List<PLATEAUCityObjectGroup>();

        [SerializeField] private bool showGeoTest = false;

        public void OnDrawGizmos()
        {
            drawer.Draw(Network);

            if (showGeoTest)
            {
                var vertices = geoTestTargets
                    .Select(x => x.GetComponent<MeshCollider>())
                    .Where(x => x)
                    .SelectMany(x => x.sharedMesh.vertices.Select(a => a.Xz()))
                    .ToList();
                var convex = GeoGraph2d.ComputeConvexVolume(vertices);
                DebugUtil.DrawArrows(convex.Select(x => x.Xay()));
            }
        }

        public void Draw(PLATEAUCityObjectGroup cityObjectGroup)
        {
            var collider = cityObjectGroup.GetComponent<MeshCollider>();
            var cMesh = collider.sharedMesh;
            var isClockwise = GeoGraph2d.IsClockwise(cMesh.vertices.Select(v => new Vector2(v.x, v.y)));
            if (isClockwise)
            {
                DebugUtil.DrawArrows(cMesh.vertices.Select(v => v + Vector3.up * 0.2f));
            }
            else
            {
                DebugUtil.DrawArrows(cMesh.vertices.Reverse().Select(v => v + Vector3.up * 0.2f));
            }
        }

        public void CreateNetwork()
        {
            if (targetAll)
            {
                var allTargets = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                    .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                    .ToList();

                Network.CreateNetwork(allTargets);
            }
            else
            {
                // èdï°ÇÕîrèúÇ∑ÇÈ
                targets = targets.Distinct().ToList();
                Network.CreateNetwork(targets);
            }
        }
    }
}