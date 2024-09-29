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
                
                // nonBorderEdgeのうち、insideWayと重ならないもの
                var outsideNonBorders = new List<RnWay>();
                foreach(var nonBorder in inter.Edges.Where(e => !e.IsBorder).Select(e => e.Border))
                {
                    bool isBorderOutside = true;
                    foreach(var insideWay in inter.SideWalks.Select(sw => sw.InsideWay))
                    {
                        if (ContourGeneratorCommon.NearestDist(nonBorder.Vertices, insideWay.Vertices) < 1)
                        {
                            isBorderOutside = false;
                            Debug.Log("border is not outside");
                            break;
                        }   
                    }
                    if(isBorderOutside) outsideNonBorders.Add(nonBorder);
                }
                
                // 隣接する道路について
                var neighborEdges = new List<RnWay>();
                foreach (var neighbor in inter.Neighbors.Where(n => n.Road != null))
                {
                    // 隣の道路のもっとも近い StartEdgeWay, EndEdgeWay を選択
                    var nStarts = neighbor.Road.SideWalks.Select(sw => sw.StartEdgeWay).Where(w => w != null).ToArray();
                    var nEnds = neighbor.Road.SideWalks.Select(sw => sw.EndEdgeWay).Where(w => w != null).ToArray();
                    if (nStarts.Length != 0)
                    {
                        int nearestID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(nStarts, neighbor.Border);
                        var nearest = nStarts[nearestID];
                        if (carBoarderEdges.Min(cb =>
                                ContourGeneratorCommon.NearestDist(nearest.Vertices.ToArray(), cb.Vertices.ToArray()) <
                                1f))
                        {
                            neighborEdges.Add(nearest);
                        }
                        
                    }

                    if (nEnds.Length != 0)
                    {
                        int nearestID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(nEnds, neighbor.Border);
                        var nearest = nEnds[nearestID];
                        if (carBoarderEdges.Min(cb =>
                                ContourGeneratorCommon.NearestDist(nearest.Vertices.ToArray(), cb.Vertices.ToArray()) <
                                1f))
                            neighborEdges.Add(nEnds[nearestID]);
                    }
                    
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