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

        // Laneの頂点の内側を向くベクトルの中央点を表示する
        [SerializeField] private bool showInsideNormalMidPoint = false;

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
                    }
                }

                foreach (var i in Enumerable.Range(0, l.vertices.Count))
                {
                    var v = l.vertices[i];
                    var n = l.GetVertexNormal(i).normalized;
                    if (showNormal)
                    {
                        Debug.DrawLine(v, v + n * 0.3f, color: Color.yellow);
                    }

                    if (showInsideNormalMidPoint)
                    {
                        if (l.HalfLineIntersectionXz(new Ray(v - n * 0.01f, -n), out var intersection))
                        {
                            DebugUtil.DrawArrow(v, (v + intersection) * 0.5f);
                        }
                    }

                }

                foreach (var way in l.ways)
                {
                    DebugUtil.DrawArrows(way.vertices.Select(x => x.PutY(x.y + 0.3f)), false, color: l.isValid ? Color.green : Color.blue, arrowColor: way.isRightSide ? Color.cyan : Color.blue);


                }

                foreach (var border in l.borders)
                {
                    DebugUtil.DrawArrows(border.vertices.Select(x => x.PutY(x.y + 0.5f)), false, color: Color.red, arrowSize: 0f);

                }
            }
        }
    }
}