using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{

    /// <summary>
    /// 道路ネットワークから輪郭線を生成します。
    /// RnmはRoad Network to Meshの略です。
    /// </summary>
    internal class RnmContourGenerator : IRnmContourGenerator
    {
        private readonly IRnmContourGenerator[] generators;
        
        public RnmContourGenerator(IEnumerable<IRnmContourGenerator> generators)
        {
            this.generators = generators.ToArray();
        }
        
        public RnmContourMeshList Generate(RnModel model)
        {
            var ret = new RnmContourMeshList();
            foreach (var gen in generators)
            {
                ret.AddRange(gen.Generate(model));
            }

            return ret;
        }
    }
    
    /// <summary>
    /// 道路ネットワークから輪郭線メッシュを生成するインターフェイスです。
    /// RnmはRoad Network to Meshの略です。
    /// </summary>
    internal interface IRnmContourGenerator
    {
        public RnmContourMeshList Generate(RnModel model);
    }

    /// <summary> 道路ネットワークから望みの<see cref="RnWay"/>を収集するインターフェイスです。 </summary>
    internal interface IRnWayCollector
    {
        IEnumerable<RnWay> Collect();
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

        /// <summary> 点群Aと点群Bを比べて、位置が同じ点の数を数えます。 ただし各点郡内の重複を排除したもので数えます。</summary>
        public static int MatchCount(IEnumerable<Vector3> pointsAArg, IEnumerable<Vector3> pointsBArg, float threshold)
        {
            var pointsA = RemoveDuplicate(pointsAArg, threshold);
            var pointsB = RemoveDuplicate(pointsBArg, threshold).ToArray();
            // var pointsA = pointsAArg.ToArray();
            // var pointsB = pointsBArg.ToArray();
            
            int count = 0;
            foreach (var a in pointsA)
            {
                foreach (var b in pointsB)
                {
                    if (Vector3.Distance(a, b) < threshold) count++;
                }
            }
            return count;
        }
        
        private static IEnumerable<Vector3> RemoveDuplicate(IEnumerable<Vector3> lineArg, float threshold)
        {
            var line = lineArg.ToArray();
            var duplicateIds = new List<int>();
            for (int i = 0; i < line.Length; i++)
            {
                for (int j = i + 1; j < line.Length; j++)
                {
                    if(Vector3.Distance(line[i], line[j]) < threshold)
                    {
                        duplicateIds.Add(j);
                    }
                }
            }

            for (int i = 0; i < line.Length; i++)
            {
                if (!duplicateIds.Contains(i)) yield return line[i];
            }
        }
        
    }
}