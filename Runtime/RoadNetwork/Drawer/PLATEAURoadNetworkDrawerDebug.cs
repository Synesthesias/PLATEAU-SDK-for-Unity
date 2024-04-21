using PLATEAU.Util;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Drawer
{
    public class PLATEAURoadNetworkDrawerDebug
    {
        public PLATEAURoadNetwork Network { get; }
        public PLATEAURoadNetworkDrawerDebug(PLATEAURoadNetwork network)
        {
            Network = network;
        }

        public void Draw()
        {
            if (!Network)
                return;
            foreach (var l in Network.Lanes)
            {
                var c = l.GetDrawCenterPoint();
                // Debug.DrawLine(c, c + Vector3.up * 1);
                // foreach (var con in l.Connected)
                //     DebugUtil.DrawArrow(c, con.GetDrawCenterPoint());

                foreach (var way in l.ways)
                {
                    DebugUtil.DrawArrows(way.vertices.Select(x => x.PutY(x.y + 0.3f)), false, color: Color.green);
                }

                foreach (var edge in l.edges)
                {
                    DebugUtil.DrawArrows(edge.vertices.Select(x => x.PutY(x.y + 0.5f)), false, color: Color.red);

                }
            }
        }
    }
}