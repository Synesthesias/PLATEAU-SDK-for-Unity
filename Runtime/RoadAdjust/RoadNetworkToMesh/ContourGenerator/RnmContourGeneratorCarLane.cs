using PLATEAU.CityAdjust.NonLibData;
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
                var calc = new RnmContourCalculator();
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
        }
    }
}