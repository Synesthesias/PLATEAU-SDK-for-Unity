﻿using PLATEAU.RoadNetwork.CityObject;
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
        public float mergeCellSize = 0.5f;
        public int mergeCellLength = 4;
        public float removeMidPointTolerance = 0.3f;
        // LOD1モデルは高さ情報がないため、高さの許容誤差を設定する
        public float lod1HeightTolerance = 1.5f;
        public bool useCityObjectOutline = true;
        // --------------------
        // end:フィールド
        // --------------------

        public RGraph CreateGraph(List<SubDividedCityObject> cityObjects)
        {
            var graph = RGraphEx.Create(cityObjects, useCityObjectOutline);

            if (reductionOnCreate)
                Reduction(graph);
            return graph;
        }

        public void Reduction(RGraph graph)
        {
            graph.Optimize(mergeCellSize, mergeCellLength, removeMidPointTolerance, lod1HeightTolerance);
        }
    }
}