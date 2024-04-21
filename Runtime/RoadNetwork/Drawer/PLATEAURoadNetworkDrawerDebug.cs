using PLATEAU.Util;
using System;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Drawer
{
    [Serializable]
    public class PLATEAURoadNetworkDrawerDebug
    {
        [SerializeField] private bool showNormal = true;

        [SerializeField] private bool showLineCenter = true;


        public void Draw(PLATEAURoadNetwork network)
        {
            if (!network)
                return;
            foreach (var l in network.Lanes)
            {
                if (showLineCenter)
                {
                    DebugUtil.DrawArrows(l.centerLine, color: Color.yellow);
                    foreach (var c in l.centerLine.Select((v, i) => new { v, i }))
                    {
                        DebugUtil.DrawString(c.i.ToString(), c.v + Vector3.up * c.i * 0.2f);
                        //DebugTextDrawer.DrawText3D(c.v, c.i.ToString());
                    }
                }

                foreach (var i in Enumerable.Range(0, l.vertices.Count))
                {
                    var v = l.vertices[i];
                    var n = l.GetOutsizeNormal(i).normalized;
                    if (showNormal)
                    {
                        Debug.DrawLine(v, v + n * 0.3f, color: Color.yellow);
                    }
                }

                foreach (var way in l.ways)
                {
                    DebugUtil.DrawArrows(way.vertices.Select(x => x.PutY(x.y + 0.3f)), false, color: l.isValid ? Color.green : Color.blue, arrowColor: way.isRightSide ? Color.cyan : Color.blue);


                }

                foreach (var edge in l.edges)
                {
                    DebugUtil.DrawArrows(edge.vertices.Select(x => x.PutY(x.y + 0.5f)), false, color: Color.red, arrowSize: 0f);

                }
            }
        }
    }
}