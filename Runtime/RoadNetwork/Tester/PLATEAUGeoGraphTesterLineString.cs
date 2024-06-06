using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;
using static PLATEAU.RoadNetwork.PLATEAUGeoGraphTester;

namespace PLATEAU.RoadNetwork
{
    public class PLATEAUGeoGraphTesterLineString : MonoBehaviour
    {
        [Serializable]
        public class EdgeBorderTestParam
        {
            public bool enable = false;
            public float allowSumAngle = 20f;
            public GeoGraph2D.DebugFindOppositeOption op = new GeoGraph2D.DebugFindOppositeOption();
        }
        public EdgeBorderTestParam edgeBorderTest = new EdgeBorderTestParam();

        private IEnumerable<Transform> GetChildren(Transform self)
        {
            for (var i = 0; i < self.childCount; i++)
                yield return self.GetChild(i);
        }

        public List<Vector2> GetVertices()
        {
            return GetChildren(transform).Select(v => v.position.Xy()).ToList();
        }

        /// <summary>
        /// この順番を逆にする
        /// </summary>
        public void ReverseChildrenSibling()
        {
            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).SetAsFirstSibling();
        }

        private void EdgeBorderTest(EdgeBorderTestParam p)
        {
            if (p.enable == false)
                return;
            var vertices = GetVertices();
            var edgeIndices = GeoGraph2D.FindMidEdge(GetVertices(), p.allowSumAngle, p.op);

            void DrawLine(IEnumerable<int> ind, Color color)
            {
                DebugEx.DrawArrows(ind.Select(i => vertices[i].Xya()), color: color);

            }
            DrawLine(Enumerable.Range(0, edgeIndices[0] + 1), Color.green);
            DrawLine(edgeIndices, Color.red);
            DrawLine(Enumerable.Range(edgeIndices.Last(), vertices.Count - edgeIndices.Last()), Color.green);
        }

        public void OnDrawGizmos()
        {
            EdgeBorderTest(edgeBorderTest);
        }

    }
}