using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static PLATEAU.Util.GeoGraph.GeoGraphDoubleLinkedEdgeList;

namespace PLATEAU.RoadNetwork.Drawer
{
    [Serializable]
    public class RGraphDrawerDebug
    {
        // --------------------
        // start:フィールド
        // --------------------
        public bool visible = false;

        public bool onlySelectedCityObjectGroupVisible = true;

        public RRoadTypeMask showFaceType = RRoadTypeMask.All;

        [Flags]
        public enum RPartsFlag
        {
            Vertex = 1 << 0,
            Edge = 1 << 1,
            Face = 1 << 2,
            All = ~0
        }
        public RPartsFlag showId = 0;

        [Serializable]
        public class FaceOption : DrawOption
        {
            public bool showOutline = true;
            public RRoadTypeMask showOutlineMask = RRoadTypeMask.Road;
            public bool showCityObjectOutline = false;
            public bool showOutlineLoop = false;
        }
        public FaceOption faceOption = new FaceOption();
        [Serializable]
        public class EdgeOption : DrawOption
        {
        }
        public EdgeOption edgeOption = new EdgeOption();

        [Serializable]
        public class VertexOption : DrawOption
        {
            public int size = 10;
            public DrawOption neighborOption = new DrawOption { visible = false, color = Color.yellow };
            public bool showPos = false;
            public bool showEdgeCount = false;
        }
        public VertexOption vertexOption = new VertexOption();

        // --------------------
        // end:フィールド
        // --------------------


        public HashSet<RFace> TargetFaces { get; } = new();
        public HashSet<REdge> TargetEdges { get; } = new();
        public HashSet<RVertex> TargetVertices { get; } = new();


        private class DrawWork
        {
            public RGraph graph;

            public HashSet<RVertex> visitedVertices = new HashSet<RVertex>();

            public HashSet<REdge> visitedEdges = new HashSet<REdge>();

            public HashSet<RFace> visitedFaces = new HashSet<RFace>();

        }

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

        public static void DrawLines(
            IEnumerable<Vector3> vertices
            , bool isLoop = false
            , Color? color = null
            , float duration = 0f
            , bool depthTest = true)
        {
            DebugEx.DrawLines(vertices, isLoop, color, duration, depthTest);
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


        private IEnumerable<PLATEAUCityObjectGroup> GetSelectedCityObjectGroups()
        {
            return Selection.gameObjects.Select(go => go.GetComponent<PLATEAUCityObjectGroup>()).Where(cog => cog != null);
        }

        private void Draw(VertexOption op, RVertex vertex, DrawWork work)
        {
            if (op.visible == false)
                return;

            if (work.visitedVertices.Contains(vertex) == false)
                return;

            work.visitedVertices.Add(vertex);

            var text = $"●";
            if (op.showEdgeCount)
                text += $"{vertex.Edges.Count}";

            if (showId.HasFlag(RPartsFlag.Vertex))
                text += $"[{vertex.DebugMyId}]";

            if (op.showPos)
                text += $"({vertex.Position.x:F2},{vertex.Position.z:F2})";

            DrawString(text, vertex.Position, fontSize: op.size);

            if (TargetVertices.Contains(vertex))
            {
                if (op.neighborOption.visible)
                {
                    //foreach (var b in vertex.GetNeighborVertices())
                    //{
                    //    DrawLine(vertex.Position, b.Position, op.neighborOption.color);
                    //}

                    foreach (var e in vertex.Edges)
                    {
                        DrawLine(e.V0.Position, e.V1.Position, op.neighborOption.color);
                    }
                }
            }
        }

        private void Draw(EdgeOption op, REdge edge, DrawWork work)
        {
            if (work.visitedEdges.Contains(edge))
                return;
            work.visitedEdges.Add(edge);

            if (TargetEdges.Contains(edge) == false)
                return;
            if (op.visible)
            {
                DrawLine(edge.V0.Position, edge.V1.Position, op.color);
                if (showId.HasFlag(RPartsFlag.Edge))
                    DrawString($"[{edge.DebugMyId}]", (edge.V0.Position + edge.V1.Position) / 2);
            }

            Draw(vertexOption, edge.V0, work);
            Draw(vertexOption, edge.V1, work);
        }

        private void Draw(FaceOption op, RFace face, DrawWork work)
        {
            if (op.visible == false)
                return;

            if (face.Visible == false)
                return;

            if (TargetFaces.Contains(face) == false)
                return;

            if (face.RoadTypes.HasAnyFlag(showFaceType) == false)
                return;

            if (work.visitedFaces.Contains(face))
                return;

            work.visitedFaces.Add(face);
            if (onlySelectedCityObjectGroupVisible)
            {
#if UNITY_EDITOR
                var show = GetSelectedCityObjectGroups().Any(cog => face.CityObjectGroup == cog);
                if (show == false)
                    return;
#endif
            }

            var vertices = op.showCityObjectOutline ? work.graph.ComputeOutlineVerticesByCityObjectGroup(face.CityObjectGroup, op.showOutlineMask)
                : face.ComputeOutlineVertices();

            bool isLoop = true;
            if (vertices.Count > 1)
            {
                if ((vertices[0].Edges.Any(e => e.V0 == vertices[^1] || e.V1 == vertices[^1])) == false)
                {
                    isLoop = false;
                }

            }

            if (showId.HasFlag(RPartsFlag.Face))
            {
                var center = vertices.Aggregate(Vector3.zero, (a, v) => v.Position + a) / vertices.Count;
                DrawString($"F[{face.DebugMyId}]", center);
            }

            if (op.showOutline)
            {
                DrawLines(vertices.Select(v => v.Position), op.showOutlineLoop, color: isLoop ? Color.white : Color.red);
                foreach (var v in vertices)
                {
                    Draw(vertexOption, v, work);
                }
            }
            else
            {
                foreach (var e in face.Edges)
                {
                    Draw(edgeOption, e, work);
                }
            }
        }

        public void Draw(RGraph graph)
        {
            if (visible == false)
                return;
            if (graph == null)
                return;
            var work = new DrawWork { graph = graph };
            foreach (var p in graph.Faces)
                Draw(faceOption, p, work);
        }
    }
}