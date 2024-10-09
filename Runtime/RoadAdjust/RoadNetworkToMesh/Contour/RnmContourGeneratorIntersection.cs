using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 交差点の輪郭線を生成します。 </summary>
    public class RnmContourGeneratorIntersection : IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            foreach (var inter in model.Intersections)
            {
                
                // 車道の端を集めます。
                var carBoarderEdges = inter.Edges
                    .Where(e => e.IsBorder)
                    .Select(e => e.Border)
                    .ToArray();

                // 歩道を集めます。
                var walks = inter.SideWalks;
                var walksStartEnd = 
                    inter.SideWalks
                        .SelectMany(w => new[] { w.StartEdgeWay, w.EndEdgeWay })
                        .Where(w => w != null);
                
                
                // 隣接する道路について
                var neighborEdges = new List<RnWay>();
                var nStartsAll = new List<RnWay>();
                var nEndsAll = new List<RnWay>(); 
                foreach (var neighbor in inter.Neighbors.Where(n => n.Road != null))
                {
                    // 隣の道路のもっとも近い StartEdgeWay, EndEdgeWay を選択
                    var nSidewalks = neighbor.Road.SideWalks;
                    var nStarts = nSidewalks.Select(sw => sw.StartEdgeWay).Where(w => w != null).ToArray();
                    var nEnds = nSidewalks.Select(sw => sw.EndEdgeWay).Where(w => w != null).ToArray();
                    nStartsAll.AddRange(nStarts);
                    nEndsAll.AddRange(nEnds);
                    if (nStarts.Length != 0)
                    {
                        int nearestID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(nStarts, neighbor.Border);
                        var nearest = nStarts[nearestID];
                        float minDist = carBoarderEdges.Min(cb =>
                            ContourGeneratorCommon.NearestDist(nearest, cb));
                        if (minDist < 1f)
                        {
                            neighborEdges.Add(nearest);
                        }
                    }

                    if (nEnds.Length != 0)
                    {
                        int nearestID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(nEnds, neighbor.Border);
                        var nearest = nEnds[nearestID];
                        float minDist = carBoarderEdges.Min(cb =>
                            ContourGeneratorCommon.NearestDist(nearest, cb));
                        if (minDist < 1f)
                        {
                            neighborEdges.Add(nEnds[nearestID]);
                        }
                        
                    }
                    
                }
                
                // nonBorderEdgeのうち外側のもの。すなわち、nonBorderEdgeのうち、歩道のinsideWayと重ならず、StartEdgeWayともEndEdgeWayとも重ならないもの
                var outsideNonBorders = new List<RnWay>();
                foreach(var nonBorder in inter.Edges.Where(e => !e.IsBorder).Select(e => e.Border))
                {
                    bool isBorderOutside = true;
                    // 条件: 歩道のinsideWayと重ならない
                    foreach(var insideWay in inter.SideWalks.Select(sw => sw.InsideWay))
                    {
                        if (ContourGeneratorCommon.NearestDist(nonBorder, insideWay) < 1)
                        {
                            isBorderOutside = false;
                            break;
                        }   
                    }
                    // 条件: StartEdgeWayともEndEdgeWayとも重ならない
                    foreach (var edge in nStartsAll.Concat(nEndsAll))
                    {
                        if (ContourGeneratorCommon.NearestDist(nonBorder, edge.Vertices) < 1)
                        {
                            isBorderOutside = false;
                            break;
                        }
                    }
                    if(isBorderOutside) outsideNonBorders.Add(nonBorder);
                }
                
                // 輪郭線を作ります。
                var calc = new RnmContourCalculator();
                // calc.AddRangeLine(neighborSidewalksStartEnd);
                calc.AddRangeLine(outsideNonBorders.Select(b => b.Vertices));
                calc.AddRangeLine(walks.Select(w => w.OutsideWay.Vertices)); // 歩道を追加します。
                calc.AddRangeLine(carBoarderEdges.Select(w => w.Vertices)); // BoarderEdgeは必ず追加します。
                calc.AddRangeLine(walksStartEnd.Select(w => w.Vertices)); // 歩道の端を追加します。
                calc.AddRangeLine(neighborEdges.Select(e => e.Vertices));
                contours.Add(calc.Calculate());
            }
            return contours;
        }
    }
}