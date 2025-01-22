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
    internal class RnmContourGeneratorCarLane : IRnmContourGenerator
    {
        public RnmContourMeshList Generate(IRrTarget target)
        {
            var contourMeshList = new RnmContourMeshList();
            foreach (var road in target.Roads())
            {
                var contours = GenerateCarLane(road);
                var targetObjs = new[]{new RoadReproduceSource(road)};
                var contourMeshes = new RnmContourMeshList
                (
                    contours.Select(c => new RnmContourMesh(targetObjs, c))
                );
                contourMeshList.AddRange(contourMeshes);
            }

            return contourMeshList;
        }

        public IEnumerable<RnmContour> GenerateCarLane(RnRoad road)
        {
            // 車道
            var allLanesWithMedian = road.AllLanesWithMedian.ToArray();
            var allLanesCount = allLanesWithMedian.Length;
            for (int i = 0; i < allLanesCount; i++)
            {
                var lane = allLanesWithMedian[i];
                if (lane == road.MedianLane) continue; // MedianLaneは別で処理します
                
                // 輪郭線となる線を集めていきます
                var contourCalc = new RnmContourCalculator(RnmMaterialType.RoadCarLane);
                
                var nextBorder = lane.GetBorder(RnLaneBorderType.Next);
                var prevBorder = lane.GetBorder(RnLaneBorderType.Prev);
                var uv1Calc = new RnmLaneUV1Calc(lane, allLanesCount, i, nextBorder, prevBorder);
                
                // Next側の境界を追加します。
                if (nextBorder != null)
                {
                    contourCalc.AddLine(nextBorder, uv1Calc.NextBorderStart(), uv1Calc.NextBorderEnd());
                }
                
                // Prev側の境界を追加します。
                if (prevBorder != null)
                {
                    contourCalc.AddLine(prevBorder, uv1Calc.PrevBorderStart(), uv1Calc.PrevBorderEnd());
                }
                
                // 右側の境界を追加します。
                var rightWay = lane.RightWay;
                if (rightWay != null)
                {
                    contourCalc.AddLine(rightWay, uv1Calc.RightWayStart(), uv1Calc.RightWayEnd());
                }
                
                // 左側の境界を追加します。
                var leftWay = lane.LeftWay;
                if (leftWay != null)
                {
                    contourCalc.AddLine(leftWay, uv1Calc.LeftWayStart(), uv1Calc.LeftWayEnd());
                }

                yield return contourCalc.Calculate();
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