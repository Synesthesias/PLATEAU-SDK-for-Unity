using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{
    public class GeoGraphDoubleLinkedEdgeList
    {

        public class Vertex
        {
            public Vector2 V { get; set; }
            public int IncidentEdgeIndex { get; set; } = -1;
        }

        public class HalfEdge
        {
            public int OriginVertexIndex { get; set; } = -1;
            public int TwinEdgeIndex { get; set; } = -1;
            public int IncidentFaceIndex { get; set; } = -1;
            public int NextEdgeIndex { get; set; } = -1;
            public int PrevEdgeIndex { get; set; } = -1;
        }


        public class Face
        {
            public int OuterComponentEdgeIndex { get; set; } = -1;
            public int InnerComponentEdgeIndex { get; set; } = -1;
        }

        public List<Vertex> Vertices { get; set; } = new List<Vertex>();
        public List<HalfEdge> HalfEdges { get; set; } = new List<HalfEdge>();

        public List<Face> Faces { get; set; } = new List<Face>();
    }
}