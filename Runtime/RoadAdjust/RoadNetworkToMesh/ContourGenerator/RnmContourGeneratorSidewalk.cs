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
        public const float PileUpHeightSideWalk = 0.15f;
        public const float PileUpHeightCurb = 0.16f; // PileUpHeightSideWalkより大きくしないと見た目上の不具合があります
        public const float CurbWidth = 0.2f;
        
        public RnmContourMeshList Generate(IRnmTarget target)
        {
            var contours = new RnmContourMeshList();
            foreach (var road in target.Roads())
            {
                var targetObjs = road.TargetTrans.Select(t => t.gameObject);
                var sideWalkContours = GenerateSidewalks(road);
                var sideWalkContourMeshes = new RnmContourMeshList(sideWalkContours.Select(s => new RnmContourMesh(targetObjs, s)));
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

                var inside = sideWalk.InsideWay;
                var outside = sideWalk.OutsideWay;
                if (inside == null || outside == null) continue;
                var curbBoundary = new RnWay(inside);
                new RnmModelAdjuster().MoveToward(curbBoundary, outside, CurbWidth, 0, 0);
                
                

                bool outsideReverse =
                    Vector3.Distance(inside[0], outside[0]) > Vector3.Distance(inside[0], outside[^1]);

                var (uvY1, uvY2) = outsideReverse ? (1, 0) : (0, 1);
                
                // 歩道部を生成
                var calc = new RnmContourCalculator(RnmMaterialType.SideWalk);
                calc.AddLine(outside, new Vector2(1, uvY1), new Vector2(1, uvY2));
                calc.AddLine(curbBoundary, new Vector2(0, uvY1), new Vector2(0, uvY2));
                var contour = calc.Calculate();

                // 歩道の段差を作成
                var modifier =
                    new RnmTessModifierPileUp(contour.Vertices.Select(v => v.Position).ToArray(), PileUpHeightSideWalk);
                contour.AddModifier(modifier);
                
                contours.Add(contour);
                
                
                // 縁石部を生成
                var calc2 = new RnmContourCalculator(RnmMaterialType.MedianLane);
                calc2.AddLine(curbBoundary, new Vector2(0, uvY1), new Vector2(0, uvY2));
                calc2.AddLine(inside , new Vector2(0, 0), new Vector2(0, 1));
                var contour2 = calc2.Calculate();
                // 段差
                var modifier2 =
                    new RnmTessModifierPileUp(contour2.Vertices.Select(v => v.Position).ToArray(), PileUpHeightCurb);
                contour2.AddModifier(modifier2);
                contours.Add(contour2);
            }
            
            
            return contours;
        }
        
    }
}