using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 交差点の輪郭線を生成します。レーンは結合します。 </summary>
    internal class RnmContourMeshGeneratorIntersectionCombine : IRnmContourMeshGenerator
    {
        public RnmContourMeshList Generate(RnModel model)
        {
            var cMeshes = new RnmContourMeshList();
            foreach (var inter in model.Intersections)
            {
                var targetObj = inter.TargetTran == null ? null : inter.TargetTran.gameObject;
                var contours = new RnmContourMeshGeneratorIntersectionSeparate().GenerateContours(inter);
                var cMesh = new RnmContourMesh(targetObj, contours);
                cMeshes.Add(cMesh);
            }

            return cMeshes;
        }


        

        /// <summary> 交差点の歩道と、歩道の端を返します </summary>
        private class Sidewalk : IRnWayCollector
        {
            private readonly RnIntersection inter;

            public Sidewalk(RnIntersection inter)
            {
                this.inter = inter;
            }

            public IEnumerable<RnWay> Collect()
            {
                // 歩道の外側
                var walks = inter.SideWalks.Select(sw => sw.OutsideWay);

                // 歩道の端にある線
                var walksEdge =
                    inter.SideWalks
                        .SelectMany(w => new[] { w.StartEdgeWay, w.EndEdgeWay })
                        .Where(w => w != null);

                return walks.Concat(walksEdge);
            }
        }
        

        
    }
}