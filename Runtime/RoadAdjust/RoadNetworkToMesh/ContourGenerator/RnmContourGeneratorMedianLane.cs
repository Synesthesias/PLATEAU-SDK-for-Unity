using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    internal class RnmContourGeneratorMedianLane : IRnmContourGenerator
    {
        private const float PileUpHeight = 0.20f;
        public RnmContourMeshList Generate(RnModel model)
        {
            var contourMeshList = new RnmContourMeshList();
            foreach (var road in model.Roads)
            {
                var contours = GenerateMedianLane(road);
                var targetObjs = road.TargetTrans.Select(cog => cog.gameObject);
                var contourMeshes = new RnmContourMeshList(
                    contours.Select(c => new RnmContourMesh(targetObjs, c))
                );
                contourMeshList.AddRange(contourMeshes);
            }

            return contourMeshList;
        }

        public IEnumerable<RnmContour> GenerateMedianLane(RnRoad road)
        {
            var median = road.MedianLane;
            if (median == null) yield break;
            var contourCalc = new RnmContourCalculator(RnmMaterialType.MedianLane);
            var left = median.LeftWay;
            var right = median.RightWay;
            var prev = median.PrevBorder;
            var next = median.NextBorder;
            var laneCount = road.AllLanesWithMedian.Count();
            int medianLaneIndex = road.AllLanesWithMedian.FindFirstIndex(lane => lane == median);
            var uv1Calc = new RnmLaneUV1Calc(median, laneCount, medianLaneIndex, next, prev);
            if(left != null) contourCalc.AddLine(left, uv1Calc.LeftWayStart(), uv1Calc.LeftWayEnd());
            if(right != null) contourCalc.AddLine(right, uv1Calc.RightWayStart(), uv1Calc.RightWayEnd());
            if(prev != null) contourCalc.AddLine(prev, uv1Calc.PrevBorderStart(), uv1Calc.PrevBorderEnd());
            if(next != null) contourCalc.AddLine(next, uv1Calc.NextBorderStart(), uv1Calc.NextBorderEnd());
            var contour = contourCalc.Calculate();

            var modifier = new RnmTessModifierPileUp(contour.Vertices.Select(v => v.Position).ToArray(), PileUpHeight);
            contour.AddModifier(modifier);
            yield return contour;
        }
    }
}