using PLATEAU.RoadNetwork.Structure;
using System;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 道路の輪郭線を生成します。 </summary>
    public class RnmContourGeneratorRoad : IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            // 道路ごとに輪郭を追加します。
            foreach (var road in model.Roads)
            {
                // 歩道を取得
                var sideWalks = road.SideWalks.Where(s => s.InsideWay != null && s.OutsideWay != null).ToArray();
                var calc = new RnmContourCalculator();

                switch (sideWalks.Length)
                {
                    // 歩道が2つ以上あるなら、左右ともに歩道がもっとも外側です。
                    case >=2:
                        calc.AddRangeLine(sideWalks.Select(s => s.OutsideWay));
                        break;
                    // 歩道がないなら、左右ともに外側の車道がもっとも外側です。
                    case 0:
                        calc.AddRangeLine(OutsideCarWays(road).Select(w => w));
                        break;
                    // 歩道が1つなら、歩道がある側は歩道、ない側は車道が外側です。
                    case 1:
                        var cars = OutsideCarWays(road);
                        if (cars.Length <= 1) break;
                        var sideWalkInside = sideWalks[0].InsideWay;
                        int correspondID = ContourGeneratorCommon.FindCorrespondWayIDMinDist(cars, sideWalkInside);
                        calc.AddLine(sideWalks[0].OutsideWay);
                        calc.AddLine(cars[cars.Length - 1 - correspondID]);
                        break;
                }
                
                // 道路終端のレーンを取得します
                var borders = road.GetBorderWays(RnLaneBorderType.Next);
                borders = borders.Concat(road.GetBorderWays(RnLaneBorderType.Prev));
                calc.AddRangeLine(borders);
                

                var contour = calc.Calculate();
                if(contour.Count > 2) contours.Add(contour);
            }

            return contours;            
        }
        
        /// <summary> 道路の車道部分のうち、もっとも外側のWayを取得します。なければ空配列を返します。 </summary>
        private RnWay[] OutsideCarWays(RnRoad road)
        {
            switch (road.MainLanes.Count)
            {
                // 車道がないなら結果は空
                case 0:
                    return new RnWay[] { };
                // 車道が1つなら、その車道の左右
                case 1:
                    var lane = road.MainLanes[0];
                    return new[] { lane.LeftWay, lane.RightWay }.Where(l => l != null).ToArray();
                // 車道が2つ以上なら、外側の車道のLeftWay
                case >= 2:
                    var lanes = road.MainLanes.ToArray();
                    var first = lanes[0].LeftWay;
                    var last = lanes[^1].LeftWay;
                    return new[] { first, last }.Where(l => l != null).ToArray();
                default:
                    throw new ArgumentException("Invalid mainLanes.Count");
            }
        }
        
    }
}