using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    public interface IContourGenerator
    {
        public RnmContourList Generate(RnModel model);
    }

    /// <summary>
    /// 道路ネットワークから輪郭線を生成します。
    /// </summary>
    public class ContourGenerator : IContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            // ここに生成したい輪郭線を記載します。
            var generators = new IContourGenerator[]
            {
                new ContourGeneratorRoad(), // 道路
                new ContourGeneratorIntersection() // 交差点
            };

            var ret = new RnmContourList();
            foreach (var gen in generators)
            {
                ret.AddRange(gen.Generate(model));
            }

            return ret;
        }
    }
    
    /// <summary> 交差点の輪郭線を生成します。 </summary>
    public class ContourGeneratorIntersection : IContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            var contours = new RnmContourList();
            foreach (var inter in model.Intersections)
            {
                var calc = new RnmContourCalculator();
                foreach (var edge in inter.Edges)
                {
                    calc.AddLine(edge.Border);
                }
                contours.Add(calc.Calculate());
            }
            return contours;
        }
    }
    
    /// <summary> 道路の輪郭線を生成します。 </summary>
    public class ContourGeneratorRoad : IContourGenerator
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
                    // 歩道が2つあるなら、左右ともに歩道がもっとも外側です。
                    case 2:
                        calc.AddRangeLine(sideWalks.Select(s => s.OutsideWay.Vertices));
                        break;
                    // 歩道がないなら、左右ともに外側の車道がもっとも外側です。
                    case 0:
                        calc.AddRangeLine(OutsideCarWays(road).Select(w => w.Vertices));
                        break;
                    // 歩道が1つなら、歩道がある側は歩道、ない側は車道が外側です。
                    case 1:
                        var cars = OutsideCarWays(road);
                        if (cars.Length <= 1) break;
                        var sideWalkInside = sideWalks[0].InsideWay;
                        int correspondID = FindCorrespondID(cars, sideWalkInside);
                        calc.AddLine(sideWalks[0].OutsideWay.Vertices);
                        calc.AddLine(cars[cars.Length - 1 - correspondID]);
                        break;
                }

                var contour = calc.Calculate();
                if(contour.Count > 2) contours.Add(contour);
            }

            return contours;            
        }
        
        /// <summary> 車道のうち、もっとも外側のWayを取得します。なければ空配列を返します。 </summary>
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
        
        /// <summary>
        /// 歩道の内側のWay1つと、車道の外側のWay複数<paramref name="cars"/>を比較します。
        /// <paramref name="cars"/>のうち、歩道と対応するWayを見つけ、そのインデックスを返します。
        /// </summary>
        private int FindCorrespondID(RnWay[] cars, RnWay sideWalkInside)
        {
            int correspondID = 0;
            float minDist = float.MaxValue;
            for(int i=0; i<cars.Length; i++)
            {
                var carVerts = cars[i].Vertices.ToArray();
                var sideWalkVerts = sideWalkInside.Vertices.ToArray();
                float dist = NearestDist(carVerts, sideWalkVerts);
                if (dist < minDist)
                {
                    minDist = dist;
                    correspondID = i;
                }
            }
            return correspondID;
        }
        
        /// <summary> 点群Aと点群Bからそれぞれ1つの点を選ぶとし、2点の距離が最小となるように点a,bを選んだときの距離を返します。</summary>
        private float NearestDist(IList<Vector3> pointsA, IList<Vector3> pointsB)
        {
            float minDist = float.MaxValue;
            foreach (var a in pointsA)
            {
                foreach (var b in pointsB)
                {
                    minDist = Math.Min(minDist, Vector3.Distance(a, b));
                }
            }

            return minDist;
        }
    }
}