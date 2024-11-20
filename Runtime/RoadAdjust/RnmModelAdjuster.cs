using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust
{
    /// <summary>
    /// 道路ネットワークをメッシュ化するために<see cref="RnModel"/>に必要な変更を加えます。
    /// </summary>
    public class RnmModelAdjuster
    {
        private const float SideWalkShrinkDist = 0.3f;
        public RnModel Adjust(RnModel srcModel)
        {
            // 変更対象はディープコピーです。
            var serializer = new RoadNetworkSerializer();
            var model = serializer.Deserialize(serializer.Serialize(srcModel));
            
            foreach (var road in model.Roads)
            {
                // 歩道の車道側を内側に移動します。
                // 歩道を狭くする意図:
                // 歩道と車道の間には白線が引かれ、歩道端には段差が出来ることとなります。
                // しかしこのままでは段差と白線が重なってしまい見た目が悪いです。
                // そのため3Dモデル上は歩道をずらします。
                foreach (var sideWalk in road.SideWalks)
                {
                    MoveToward(sideWalk.InsideWay, sideWalk.OutsideWay, SideWalkShrinkDist);
                    // 歩道を狭くした分、車道を広くする必要がありますが、
                    // 両者は共通のRnWayを持つため、片方を変更するともう片方も変更されます。
                }
            }

            return model;
        }

        /// <summary>
        /// <paramref name="srcWay"/>を、<paramref name="targetWay"/>の方向に<paramref name="dist"/>だけ移動します。
        /// </summary>
        private void MoveToward(RnWay srcWay, RnWay targetWay, float dist)
        {
            if (targetWay == null || targetWay.Count == 0) return;
            if (srcWay == null || srcWay.Count == 0) return;
            
            var targetPoints = targetWay.Points.ToArray();
            int srcCount = srcWay.Count;
            for (int i = 0; i < srcCount; i++)
            {
                float minSqrDist = float.MaxValue;
                int nearestID = 0;
                for (int j = 0; j < targetPoints.Length; j++)
                {
                    float d = Vector3.SqrMagnitude(srcWay[i] - targetPoints[j]);
                    if (d < minSqrDist)
                    {
                        minSqrDist = d;
                        nearestID = j;
                    }
                }

                var nearestV = targetPoints[nearestID].Vertex;
                var diff = nearestV - srcWay[i];
                srcWay[i] += diff.normalized * Mathf.Min(dist, diff.magnitude);
            }
        } 
        
    }
}