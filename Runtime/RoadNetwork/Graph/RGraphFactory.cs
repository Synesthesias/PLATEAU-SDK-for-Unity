using PLATEAU.RoadNetwork.CityObject;
using System;
using System.Collections.Generic;
using System.Linq;
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
        // LOD1モデルは高さ情報がないため、高さの許容誤差を設定する
        public float lod1HeightTolerance = 1.5f;
        public bool useCityObjectOutline = true;

        [Header("Optimize")] public bool modifySideWalkShape = true;
        // --------------------
        // end:フィールド
        // --------------------

        public RGraph CreateGraph(List<SubDividedCityObject> cityObjects)
        {
            var tmp = cityObjects.Where(x => x.CityObjectGroup != null)
                .Select(x => (x, x.CityObjectGroup.transform.localToWorldMatrix)).ToList();
            return CreateGraph(tmp);
        }

        public RGraph CreateGraph(List<(SubDividedCityObject cityObjects, Matrix4x4 mat)> cityObjects)
        {
            var graph = RGraphEx.Create(cityObjects, useCityObjectOutline);

            if (reductionOnCreate)
            {
                graph.Optimize(mergeCellSize, mergeCellLength, removeMidPointTolerance, lod1HeightTolerance);

                if (modifySideWalkShape)
                {
                    graph.ModifySideWalkShape();
                }
            }

            return graph;
        }
    }
}