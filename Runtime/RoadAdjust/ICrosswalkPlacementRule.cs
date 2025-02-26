using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.RoadAdjust
{
    /// <summary>
    /// 横断歩道を設置する条件を表すインターフェイスです。
    /// 設置すべきときにメソッド<see cref="ShouldPlace"/>がtrueとなるようにサブクラスを実装します。
    /// </summary>
    public interface ICrosswalkPlacementRule
    {
        public bool ShouldPlace(RnRoad road);
    }

    /// <summary> 横断歩道を設置する条件であり、太く長い道路であるときにtrueを返します。 </summary>
    public class CrosswalkPlacementRuleBigRoad : ICrosswalkPlacementRule
    {
        /// <summary> レーン数がこの値未満の道路には設置しません </summary>
        private const int LaneCountThreshold = 2;
        
        /// <summary> 道路の長さがこの値未満の道路には設置しません </summary>
        private const float LengthThreshold = 30f;
        
        public bool ShouldPlace(RnRoad road)
        {
            // レーン数があること
            if (road.MainLanes.Count < LaneCountThreshold) return false;
            // 十分長いこと
            var lane = road.MainLanes[0];
            float leftLength = lane.LeftWay?.CalcLength() ?? 0f;
            float rightLength = lane.RightWay?.CalcLength() ?? 0f;
            return Mathf.Max(leftLength, rightLength) >= LengthThreshold;
        }
    }

    /// <summary> 横断歩道を設置する条件であり、全交差点に設置します。 </summary>
    public class CrosswalkPlacementRuleAll : ICrosswalkPlacementRule
    {
        public bool ShouldPlace(RnRoad _)
        {
            return true;
        }
    }

    /// <summary> 横断歩道を設置しません </summary>
    public class CrosswalkPlacementRuleNone : ICrosswalkPlacementRule
    {
        public bool ShouldPlace(RnRoad _)
        {
            return false;
        }
    }

    /// <summary> 横断歩道を設置しません。すでに設置されている場合は削除します。 </summary>
    public class CrosswalkPlacementRuleDelete : ICrosswalkPlacementRule
    {
        /// <summary> 削除を伴います </summary>
        public bool ShouldPlace(RnRoad road)
        {
            var target = new RoadReproduceSource(road);
            var crosswalkA =
                PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, target, ReproducedRoadDirection.Next);
            var crosswalkB = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, target, ReproducedRoadDirection.Prev);
            if(crosswalkA != null) Object.DestroyImmediate(crosswalkA);
            if(crosswalkB != null) Object.DestroyImmediate(crosswalkB);
            return false;
        }
    }
    
    /// <summary> 横断歩道を置く頻度設定 </summary>
    internal enum CrosswalkFrequency
    {
        /// <summary> 太くて長い道路に置く </summary>
        BigRoad,
        /// <summary> 全ての交差点に置く </summary>
        All,
        /// <summary> 置かない。すでに置かれているものは削除しない </summary>
        None,
        /// <summary> 置かない。すでに置かれているものは削除する。 </summary>
        Delete
    }

    internal static class CrosswalkFrequencyExtensions
    {
        public static ICrosswalkPlacementRule ToPlacementRule(this CrosswalkFrequency freq)
        {
            switch (freq)
            {
                case CrosswalkFrequency.BigRoad:
                    return new CrosswalkPlacementRuleBigRoad();
                case CrosswalkFrequency.All:
                    return new CrosswalkPlacementRuleAll();
                case CrosswalkFrequency.None:
                    return new CrosswalkPlacementRuleNone();
                case CrosswalkFrequency.Delete:
                    return new CrosswalkPlacementRuleDelete();
                default:
                    throw new ArgumentOutOfRangeException();   
            }
        }
    }
}