﻿using PLATEAU.RoadNetwork.CityObject;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Graph
{
    [Serializable]
    public class RGraphFactory
    {
        // --------------------
        // start:フィールド
        // --------------------
        public bool reductionOnCreate = true;
        public float mergeCellSize = 0.5f;
        public int mergeCellLength = 4;
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
            graph.Optimize(mergeCellSize, mergeCellLength, removeMidPointTolerance);
        }
    }
}