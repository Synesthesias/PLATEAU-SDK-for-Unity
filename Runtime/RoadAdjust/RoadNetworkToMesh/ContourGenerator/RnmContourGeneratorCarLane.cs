using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
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
            int carLaneCount = road.MainLanes.Count;
            for (int i = 0; i < carLaneCount; i++)
            {
                var lane = road.MainLanes[i];
                var calc = new RnmContourCalculator(RnmMaterialType.RoadCarLane);
                bool reverse = lane.IsReverse;
                float laneUvLeft = ((float)i) / carLaneCount;
                float laneUvRight = (((float)i) + 1) / carLaneCount;
                float nextUvY = reverse ? 0 : 1;
                float prevUvY = reverse ? 1 : 0;
                
                // Next側の境界を追加します。
                var nextBorder = lane.GetBorder(RnLaneBorderType.Next);
                if (nextBorder != null)
                {
                    var (uvX1, uvX2) = nextBorder.IsReversed ? (laneUvRight, laneUvLeft):(laneUvLeft, laneUvRight);
                    calc.AddLine(nextBorder, new Vector2(uvX1, nextUvY), new Vector2(uvX2,nextUvY));
                }
                
                // Prev側の境界を追加します。
                var prevBorder = lane.GetBorder(RnLaneBorderType.Prev);
                if (prevBorder != null)
                {
                    var (uvX1, uvX2) = prevBorder.IsReversed ? (laneUvRight, laneUvLeft):(laneUvLeft, laneUvRight);
                    calc.AddLine(prevBorder, new Vector2(uvX1, prevUvY), new Vector2(uvX2, prevUvY));
                }
                
                // 右側の境界を追加します。
                var rightWay = lane.RightWay;
                if (rightWay != null)
                {
                    var uvX = lane.IsReverse ? laneUvLeft : laneUvRight;
                    calc.AddLine(rightWay, new Vector2(uvX, prevUvY), new Vector2(uvX, nextUvY));
                }
                
                // 左側の境界を追加します。
                var leftWay = lane.LeftWay;
                if (leftWay != null)
                {
                    var uvX = lane.IsReverse ? laneUvRight : laneUvLeft;
                    calc.AddLine(leftWay, new Vector2(uvX, prevUvY), new Vector2(uvX, nextUvY));
                }

                yield return calc.Calculate();
            }
            
            // MainLanesの外にあるが、歩道に隣接していない部分
            var left = road.GetMergedSideWay(RnDir.Left);
            var right = road.GetMergedSideWay(RnDir.Right);
            var sides = new List<IEnumerable<Vector3>> { left, right }.Where(s => s != null);
            var walks = road.SideWalks.Where(s => s != null).ToArray();
            var selfSideEdges = road.SideWalks
                .SelectMany(s => s.EdgeWays);
            var neighborSideEdges = road
                .GetNeighborRoads()
                .SelectMany(r => r.SideWalks)
                .SelectMany(s => s.EdgeWays);
            var sideEdges = neighborSideEdges.Concat(selfSideEdges).ToArray();
            foreach (var side in sides)
            {
                if (walks.Any(w => w.OutsideWay != null && AreTouching(side, w.OutsideWay))) continue;
                if (walks.Any(w => w.InsideWay != null && AreTouching(side, w.InsideWay))) continue;
                var touchingEdges = sideEdges.Where(e => AreTouching(side, e)).ToArray();
                if(touchingEdges.Length == 0) continue;
                var calc = new RnmContourCalculator(RnmMaterialType.RoadCarLane);
                calc.AddLine(side, Vector2.zero, Vector2.zero); // FIXME UV1は未実装
                foreach (var touchingEdge in touchingEdges)
                {
                    calc.AddLine(touchingEdge, Vector2.zero, Vector2.zero); // FIXME UV1は未実装
                }
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