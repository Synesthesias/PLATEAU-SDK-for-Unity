using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 道路の輪郭線を生成します。道路レーンはまとめます。 </summary>
    internal class RnmContourMeshGeneratorRoadCombine : IRnmContourMeshGenerator
    {

        /// <summary> 車道の点と歩道の点が対応しているとみなす距離のしきい値 </summary>
        // private const float CarWalkMatchDistThreshold = 5.8f; // 歩道と車道の細かい差を拾わない程度には大きく、歩道から大きく形が外れた車道の形状を大雑把に拾う程度には小さくした、経験則から来る数値
        
        
        public RnmContourMeshList Generate(RnModel model)
        {
            var cMeshes = new RnmContourMeshList();
            // 道路ごとに輪郭を追加します。
            foreach (var road in model.Roads)
            {
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;

                var carLanes = new RnmContourMeshGeneratorCarLane().GenerateCarLane(road);
                var sideWalks = new RnmContourMeshGeneratorSidewalk().GenerateSidewalks(road);
                var cMesh = new RnmContourMesh(targetObj, carLanes.Concat(sideWalks));
                cMeshes.Add(cMesh);
            }

            return cMeshes;            
        }
        
    }
}