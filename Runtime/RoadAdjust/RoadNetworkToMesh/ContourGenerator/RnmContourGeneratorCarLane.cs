using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 車道の輪郭線を生成します。レーンは分割します。
    /// </summary>
    internal class RnmContourMeshGeneratorCarLane : IRnmContourMeshGenerator
    {
        public RnmContourMeshList Generate(RnModel model)
        {
            var contourMeshList = new RnmContourMeshList();
            foreach (var road in model.Roads)
            {
                var contours = GenerateCarLane(road);
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;
                var contourMeshes = new RnmContourMeshList
                (
                    contours.Select(c => new RnmContourMesh(targetObj, c))
                );
                contourMeshList.AddRange(contourMeshes);
            }

            return contourMeshList;
        }

        public IEnumerable<RnmContour> GenerateCarLane(RnRoad road)
        {
            // 車道
            foreach (var lane in road.MainLanes)
            {
                var calc = new RnmContourCalculator(RnmMaterialType.CarLane);
                var lines = new List<IEnumerable<Vector3>>
                {
                    lane.GetBorder(RnLaneBorderType.Next),
                    lane.GetBorder(RnLaneBorderType.Prev),
                    lane.RightWay,
                    lane.LeftWay
                };
                calc.AddRangeLine(lines.Where(l => l != null));
                yield return calc.Calculate();
            }
            
            // MainLanesの外にあるが、歩道に隣接していない部分
            var left = road.GetMergedSideWay(RnDir.Left);
            var right = road.GetMergedSideWay(RnDir.Right);
            var sides = new List<IEnumerable<Vector3>> { left, right };
            var walks = road.SideWalks;
            var selfSideEdges = road.SideWalks
                .SelectMany(s => s.EdgeWays);
            var neighborSideEdges = road
                .GetNeighborRoads()
                .SelectMany(r => r.SideWalks)
                .SelectMany(s => s.EdgeWays);
            var sideEdges = neighborSideEdges.Concat(selfSideEdges).ToArray();
            foreach (var side in sides)
            {
                if (walks.Any(w => AreTouching(side, w.OutsideWay))) continue;
                if (walks.Any(w => AreTouching(side, w.InsideWay))) continue;
                var touchingEdges = sideEdges.Where(e => AreTouching(side, e)).ToArray();
                if(touchingEdges.Length == 0) continue;
                var calc = new RnmContourCalculator(RnmMaterialType.CarLane);
                calc.AddRangeLine(new [] { side }.Concat(touchingEdges));
                yield return calc.Calculate();
            }
        }
        
        private bool AreTouching(IEnumerable<Vector3> a, IEnumerable<Vector3> bArg)
        {
            var b = bArg.ToArray();
            return a.Any(p => b.Any(p2 => Vector3.Distance(p, p2) < 1f));
        }
    }
}