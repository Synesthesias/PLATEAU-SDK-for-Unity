using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 交差点の輪郭線を生成します。レーンは分割します。
    /// </summary>
    internal class RnmContourGeneratorIntersectionSeparate: IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            foreach (var inter in model.Intersections)
            {
                var targetObj = inter.TargetTran == null ? null : inter.TargetTran.gameObject;
                // 車道
                var carEdges = inter.Edges.Select(e => e.Border);
                var calcCar = new RnmContourCalculator(targetObj);
                calcCar.AddRangeLine(carEdges);
                contours.Add(calcCar.Calculate());
                
                // 歩道
                foreach (var sideWalk in inter.SideWalks)
                {
                    var calc = new RnmContourCalculator(targetObj);
                    var lines = new List<IEnumerable<Vector3>> { sideWalk.InsideWay, sideWalk.OutsideWay};
                    lines.AddRange(sideWalk.EdgeWays);
                    calc.AddRangeLine(lines.Where(l => l != null));
                    contours.Add(calc.Calculate());
                }
            }
            return contours;
        }

    }
}