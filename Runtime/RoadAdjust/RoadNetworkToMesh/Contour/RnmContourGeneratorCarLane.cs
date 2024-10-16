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
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            foreach (var road in model.Roads)
            {
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;
                
                // 車道
                foreach (var lane in road.MainLanes)
                {
                    var calc = new RnmContourCalculator(targetObj);
                    var lines = new List<IEnumerable<Vector3>>
                    {
                        lane.GetBorder(RnLaneBorderType.Next),
                        lane.GetBorder(RnLaneBorderType.Prev),
                        lane.RightWay,
                        lane.LeftWay
                    };
                    calc.AddRangeLine(lines.Where(l => l != null));
                    contours.Add(calc.Calculate());
                }
            }

            return contours;
        }
    }
}