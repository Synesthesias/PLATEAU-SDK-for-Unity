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

        /// <summary> 各歩道の輪郭線を生成します。 </summary>
        public IEnumerable<RnmContour> GenerateSidewalks(RnRoad road)
        {
            var contours = new List<RnmContour>();
            
            // 歩道
            foreach (var sideWalk in road.SideWalks)
            {
                var calc = new RnmContourCalculator(RnmMaterialType.SideWalk);

                var inside = sideWalk.InsideWay;
                if (inside != null)
                {
                    calc.AddLine(inside, new Vector2(1, 0), new Vector2(1, 1));
                }

                var outside = sideWalk.OutsideWay;
                if (outside != null)
                {
                    bool outsideReverse =
                        inside != null &&
                        Vector3.Distance(inside[0], outside[0]) > Vector3.Distance(inside[0], outside[^1]);
                    
                    var (uvY1, uvY2) = outsideReverse ? (1, 0) : (0, 1);
                    calc.AddLine(outside, new Vector2(0, uvY1), new Vector2(0, uvY2));
                }

                contours.Add(calc.Calculate());
            }
            
            
            return contours;
        }
        
    }
}