using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 歩道の輪郭線を生成します。レーンは分割します。
    /// </summary>
    internal class RnmContourMeshGeneratorSidewalk : IRnmContourMeshGenerator
    {
        public RnmContourMeshList Generate(RnModel model)
        {
            var contours = new RnmContourMeshList();
            foreach (var road in model.Roads)
            {
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;
                var sideWalkContours = GenerateSidewalks(road);
                var sideWalkContourMeshes = new RnmContourMeshList(sideWalkContours.Select(s => new RnmContourMesh(targetObj, s)));
                contours.AddRange(sideWalkContourMeshes);
            }

            return contours;
        }

        public IEnumerable<RnmContour> GenerateSidewalks(RnRoad road)
        {
            var contours = new List<RnmContour>();
            

            // 歩道
            foreach (var sideWalk in road.SideWalks)
            {
                var calc = new RnmContourCalculator();
                var lines = new List<IEnumerable<Vector3>>
                {
                    sideWalk.InsideWay,
                    sideWalk.OutsideWay
                };
                lines.AddRange(sideWalk.EdgeWays);
                calc.AddRangeLine(lines.Where(l => l != null));
                contours.Add(calc.Calculate());
            }
            return contours;
        }
    }
}