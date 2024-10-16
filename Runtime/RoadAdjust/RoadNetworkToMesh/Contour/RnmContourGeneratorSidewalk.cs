using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 歩道の輪郭線を生成します。レーンは分割します。
    /// </summary>
    internal class RnmContourGeneratorSidewalk : IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            foreach (var road in model.Roads)
            {
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;

                // 歩道
                foreach (var sideWalk in road.SideWalks)
                {
                    var calc = new RnmContourCalculator(targetObj);
                    var lines = new List<IEnumerable<Vector3>>
                    {
                        sideWalk.InsideWay,
                        sideWalk.OutsideWay
                    };
                    lines.AddRange(sideWalk.EdgeWays);
                    calc.AddRangeLine(lines.Where(l => l != null));
                    contours.Add(calc.Calculate());
                }
            }

            return contours;
        }
    }
}