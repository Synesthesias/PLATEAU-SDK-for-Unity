using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 交差点の輪郭線を生成します。レーンは分割します。
    /// </summary>
    internal class RnmContourMeshGeneratorIntersectionSeparate: IRnmContourMeshGenerator
    {
        public RnmContourMeshList Generate(RnModel model)
        {
            var cMeshes = new RnmContourMeshList();
            foreach (var inter in model.Intersections)
            {
                var targetObj = inter.TargetTran == null ? null : inter.TargetTran.gameObject;
                cMeshes.Add(CarMesh(inter, targetObj));

                // 歩道
                foreach (var sideWalk in inter.SideWalks)
                {
                    cMeshes.Add(SidewalkMesh(sideWalk, targetObj));
                }
            }
            return cMeshes;
        }

        private RnmContourMesh CarMesh(RnIntersection inter, GameObject targetObj)
        {
            // 車道
            var edges = inter.Edges.Select(e => e.Border);
            var calc = new RnmContourCalculator();
            calc.AddRangeLine(edges);
            return new RnmContourMesh(targetObj, calc.Calculate());
        }

        private RnmContourMesh SidewalkMesh(RnSideWalk sideWalk, GameObject targetObj)
        {
            var calc = new RnmContourCalculator();
            var lines = new List<IEnumerable<Vector3>> { sideWalk.InsideWay, sideWalk.OutsideWay};
            lines.AddRange(sideWalk.EdgeWays);
            calc.AddRangeLine(lines.Where(l => l != null));
            return (new RnmContourMesh(targetObj, calc.Calculate()));
        }
    }
}