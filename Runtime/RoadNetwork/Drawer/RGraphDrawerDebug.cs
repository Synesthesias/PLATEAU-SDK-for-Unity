using PLATEAU.CityGML;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Drawer
{
    [Serializable]
    public class RGraphDrawerDebug
    {
        // --------------------
        // start:フィールド
        // --------------------
        public bool visible = false;

        [Serializable]
        public class PolygonOption : DrawOption
        {

        }
        public PolygonOption polygonOption = new PolygonOption();
        [Serializable]
        public class EdgeOption : DrawOption
        {

        }
        public EdgeOption edgeOption = new EdgeOption();

        [Serializable]
        public class VertexOption : DrawOption
        {

        }
        public VertexOption vertexOption = new VertexOption();


        // --------------------
        // end:フィールド
        // --------------------

        private void DrawArrows(IEnumerable<Vector3> vertices
            , bool isLoop = false
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , Color? arrowColor = null
            , float duration = 0f
            , bool depthTest = true)
        {
            DebugEx.DrawArrows(vertices, isLoop, arrowSize, arrowUp, color, arrowColor, duration, depthTest);
        }
        private void DrawString(string text, Vector3 worldPos, Vector2? screenOffset = null, Color? color = null, int? fontSize = null)
        {
            DebugEx.DrawString(text, worldPos, screenOffset, color, fontSize);
        }

        private void DrawLine(Vector3 start, Vector3 end, Color? color = null)
        {
            Debug.DrawLine(start, end, color ?? Color.white);
        }

        private void DrawArrow(
            Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? bodyColor = null
            , Color? arrowColor = null
            , float duration = 0f
            , bool depthTest = true)
        {
            DebugEx.DrawArrow(start, end, arrowSize, arrowUp, bodyColor, arrowColor, duration, depthTest);
        }


        private void Draw(VertexOption v, RVertex vertex)
        {
            if (v.visible == false)
                return;
        }

        private void Draw(EdgeOption e, REdge edge)
        {
            if (e.visible == false)
                return;

            DrawLine(edge.V0.Position, edge.V1.Position, e.color);
        }

        private void Draw(PolygonOption p, RPolygon polygon)
        {
            if (p.visible == false)
                return;

            if (polygon.Visible == false)
                return;

            foreach (var e in polygon.Edges)
            {
                Draw(edgeOption, e);
            }
        }

        public void Draw(RGraph graph)
        {
            if (visible == false)
                return;
            if (graph == null)
                return;

            foreach (var p in graph.Polygons)
                Draw(polygonOption, p);
        }
    }
}