using PLATEAU.RoadNetwork;
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
                var calc = new RnmContourCalculator(RnmMaterialType.SideWalk);
                var lines = new List<IEnumerable<Vector3>>
                {
                    sideWalk.InsideWay,
                    sideWalk.OutsideWay
                };
                lines.AddRange(sideWalk.EdgeWays);
                calc.AddRangeLine(lines.Where(l => l != null));
                contours.Add(calc.Calculate());
            }
            
            // 道路ネットワーク上、歩道が見当たらない場合は、SideWalkEdgesと外側の車道から推測される道路の外側部を歩道とします。
            // if (/*road.SideWalks.Count == 0*/true)
            // {
            //     var selfSideEdges = road.SideWalks
            //         .SelectMany(s => s.EdgeWays);
            //     var neighborSideEdges = road
            //         .GetNeighborRoads()
            //         .SelectMany(r => r.SideWalks)
            //         .SelectMany(s => s.EdgeWays);
            //
            //     var sideEdges = neighborSideEdges.Concat(selfSideEdges).ToArray();
            //     sideEdges = sideEdges.Where(si => road.SideWalks.All(sw => !AreTouching(sw.OutsideWay, si))).ToArray();
            //     
            //     // 車道端
            //     // 車道端に隣接する隣接歩道端を追加
            //     contours.AddRange(NonSideWalkOutside(road, RnDir.Left, sideEdges));
            //     contours.AddRange(NonSideWalkOutside(road, RnDir.Right, sideEdges));
            // }
            
            return contours;
        }

        // private IEnumerable<RnmContour> NonSideWalkOutside(RnRoad road, RnDir dir, RnWay[] sideEdges)
        // {
        //     var sideWay = road.GetMergedSideWay(dir);
        //     if (sideWay == null) yield break;
        //     var sideWalks = road.SideWalks.ToArray();
        //     if (sideWalks.Length == 0) yield break;
        //
        //     bool isTouchingBorder =
        //         AreTouching(road.GetMergedBorder(RnLaneBorderType.Next), sideWay) ||
        //         AreTouching(road.GetMergedBorder(RnLaneBorderType.Prev), sideWay);
        //     if (!isTouchingBorder) yield break;
        //     
        //     
        //     var sideEdge = sideEdges.Where(s => AreTouching(s, sideWay));
        //     var calc = new RnmContourCalculator(RnmMaterialType.SideWalk);
        //     calc.AddRangeLine(sideEdge);
        //     calc.AddLine(sideWay);
        //     yield return calc.Calculate();
        // }

        
    }
}