using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから輪郭線を生成するインターフェイスです。
    /// RnmはRoad Network to Meshの略です。
    /// </summary>
    public interface IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model);
    }

    /// <summary>
    /// 道路ネットワークから輪郭線を生成します。
    /// RnmはRoad Network to Meshの略です。
    /// </summary>
    public class RnmContourGenerator : IRnmContourGenerator
    {
        public RnmContourList Generate(RnModel model)
        {
            // ここに生成したい輪郭線を記載します。
            var generators = new IRnmContourGenerator[]
            {
                new RnmContourGeneratorRoad(), // 道路
                new RnmContourGeneratorIntersection() // 交差点
            };

            var ret = new RnmContourList();
            foreach (var gen in generators)
            {
                ret.AddRange(gen.Generate(model));
            }

            return ret;
        }
    }

    /// <summary>
    /// <see cref="IRnmContourGenerator"/>のサブクラスで使うための共通機能を提供します。
    /// </summary>
    internal static class ContourGeneratorCommon
    {
        /// <summary>
        /// 歩道の内側のWay1つと、車道の外側のWay複数<paramref name="cars"/>を比較します。
        /// <paramref name="cars"/>のうち、歩道と対応するWayを見つけ、そのインデックスを返します。
        /// 頂点距離の最小で調べます。
        /// </summary>
        public static int FindCorrespondWayIDMinDist(IList<RnWay> cars, RnWay sideWalkInside)
        {
            if (cars.Count == 0) throw new ArgumentException("argument length is zero.");
            int correspondID = 0;
            float minDist = float.MaxValue;
            for(int i=0; i<cars.Count; i++)
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
        public static float NearestDist(IEnumerable<Vector3> pointsA, IEnumerable<Vector3> pointsBArg)
        {
            var pointsB = pointsBArg.ToArray();
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