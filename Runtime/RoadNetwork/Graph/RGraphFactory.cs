using PLATEAU.RoadNetwork.Mesh;
using System;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork.Graph
{
    [Serializable]
    public class RGraphFactory
    {
        // --------------------
        // start:フィールド
        // --------------------
        public bool reductionOnCreate = true;
        public float mergeCellSize = 0.2f;
        public int mergeCellLength = 2;
        public float removeMidPointTolerance = 0.3f;
        // --------------------
        // end:フィールド
        // --------------------

        public RGraph CreateGraph(List<SubDividedCityObject> cityObjects)
        {
            var graph = RGraphEx.Create(cityObjects);
            if (reductionOnCreate)
                Reduction(graph);
            return graph;
        }

        public void Reduction(RGraph graph)
        {
            graph.VertexReduction(mergeCellSize, mergeCellLength, removeMidPointTolerance);
            graph.EdgeReduction();
        }
    }
}