using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュに変換します。
    /// </summary>
    public class RoadNetworkToMesh
    {
        private RnModel model;
        
        public RoadNetworkToMesh(RnModel model)
        {
            if (model == null)
            {
                Debug.LogError("道路ネットワークがありません。");
            }
            this.model = model;
        }

        public void Generate()
        {
            var contours = new ContourGenerator().Generate(model);
            var obj = new GameObject("RoadNetworkToMeshDebug");
            var comp = obj.AddComponent<PLATEAURoadNetworkToMeshDebug>();
            comp.Init(contours);
        }
        
        
        
    }
}