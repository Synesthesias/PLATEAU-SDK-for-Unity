using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 道路の輪郭線を生成します。道路レーンはまとめます。 </summary>
    internal class RnmContourGeneratorRoadCombine : IRnmContourGenerator
    {

        /// <summary> 車道の点と歩道の点が対応しているとみなす距離のしきい値 </summary>
        private const float CarWalkMatchDistThreshold = 5.8f; // 歩道と車道の細かい差を拾わない程度には大きく、歩道から大きく形が外れた車道の形状を大雑把に拾う程度には小さくした、経験則から来る数値
        
        
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            // 道路ごとに輪郭を追加します。
            foreach (var road in model.Roads)
            {
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;
                var calc = new RnmContourCalculator(targetObj);
                
                var sideWalks = road.SideWalks.ToArray(); // 歩道
                
                
                
                // 道路終端のレーンを取得します
                var borders = road.GetBorderWays(RnLaneBorderType.Next);
                borders = borders.Concat(road.GetBorderWays(RnLaneBorderType.Prev)).ToArray();
                calc.AddRangeLine(borders);

                // 歩道の端
                var walkEdges = sideWalks.SelectMany(sw => sw.EdgeWays).Where(w => w != null).ToArray();
                calc.AddRangeLine(walkEdges);
                
                // 隣接道路の歩道の端のうち、この道路に接するもの
                var neighborWalkEdges = new List<RnWay>();
                foreach (var neighbor in road.GetNeighborRoads())
                {
                    var nEdges = neighbor.SideWalks.SelectMany(sw => sw.EdgeWays).Where(w => w != null);
                    foreach(var nEdge in nEdges)
                    {
                        bool isNeighborEdge = false;
                        foreach (var border in borders)
                        {
                            if(ContourGeneratorCommon.MatchCount(border, nEdge, CarWalkMatchDistThreshold) >= 1)
                            {
                                isNeighborEdge = true;
                                break;
                            }
                        }

                        if (isNeighborEdge)
                        {
                            neighborWalkEdges.Add(nEdge);
                        }
                    }
                }
                calc.AddRangeLine(neighborWalkEdges);
                    
                
                var carOutsidesPreRemove = OutsideCarWays(road).ToList(); // 車道の外側
                
                // 車道部分の外側のうち、歩道と接さず、歩道端とも接さない部分
                var allSidewalks = sideWalks.SelectMany(sw => sw.InsideWay);
                allSidewalks = allSidewalks.Concat(walkEdges.SelectMany(we => we));
                allSidewalks = allSidewalks.Concat(neighborWalkEdges.SelectMany(we => we)).ToArray();
                var carOutsidePostRemove = carOutsidesPreRemove.SelectMany(co => co.SubtractSeparate(allSidewalks, CarWalkMatchDistThreshold));
                
                calc.AddRangeLine(sideWalks.Select(sw => sw.OutsideWay));
                calc.AddRangeLine(carOutsidePostRemove);
                

                var contour = calc.Calculate();
                if(contour.Count > 2) contours.Add(contour);
            }

            return contours;            
        }
        
        /// <summary> 道路の車道部分のうち、もっとも外側のWayを取得します。なければ空配列を返します。 </summary>
        private IEnumerable<RnmLine> OutsideCarWays(RnRoad road)
        {
            switch (road.MainLanes.Count)
            {
                // 車道がないなら結果は空
                case 0:
                    yield break;
                // 車道が1つなら、その車道の左右
                case 1:
                    var lane = road.MainLanes[0];
                    foreach (var way in new[] { lane.LeftWay, lane.RightWay }.Where(w => w != null))
                    {
                        yield return new RnmLine(way);
                    }
                    yield break;
                // 車道が2つ以上なら、外側の車道のLeftWay
                case >= 2:
                    var lanes = road.MainLanes.ToArray();
                    var first = lanes[0].LeftWay;
                    var last = lanes[^1].LeftWay;
                    foreach (var way in new[] { first, last }.Where(w => w != null))
                    {
                        yield return new RnmLine(way);
                    }
                    yield break;
                
                default:
                    throw new ArgumentException("Invalid mainLanes.Count");
            }
        }
        
    }
}