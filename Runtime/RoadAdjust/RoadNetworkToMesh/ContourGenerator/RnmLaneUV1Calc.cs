using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路の端のUV1が(0,0)、反対側の端が(1,1)になるようにUV1を計算します。
    /// RnmはRoad Network to Meshの略です。
    /// </summary>
    public class RnmLaneUV1Calc
    {
        private readonly float laneUvLeft;
        private readonly float laneUvRight;
        private readonly float nextUvY; // 道路のnext側のUV1
        private readonly float prevUvY; // 道路のprev側のUV1
        private readonly bool reversedLane;
        private readonly RnWay nextBorder;
        private readonly RnWay prevBorder;
        
        public RnmLaneUV1Calc(RnLane lane, int laneCount, int laneIndex, RnWay nextBorder, RnWay prevBorder)
        {
            reversedLane = lane.IsReverse;
            laneUvLeft = ((float)laneIndex) / laneCount;
            laneUvRight = (((float)laneIndex) + 1) / laneCount;
            nextUvY = reversedLane ? 0 : 1;
            prevUvY = reversedLane ? 1 : 0;
            this.nextBorder = nextBorder;
            this.prevBorder = prevBorder;
        }

        /// <summary> 道路のNext側のボーダー線の開始点のUVです。 </summary>
        public Vector2 NextBorderStart()
        {
            var uvX = nextBorder.IsReversed ? laneUvRight : laneUvLeft;
            return new Vector2(uvX, nextUvY);
        }

        /// <summary> 道路のNext側のボーダー線の終了地点のUVです。 </summary>
        public Vector2 NextBorderEnd()
        {
            var uvX = nextBorder.IsReversed ? laneUvLeft : laneUvRight;
            return new Vector2(uvX, nextUvY);
        }

        /// <summary> 道路のPrev側のボーダー線の開始地点のUVです。 </summary>
        public Vector2 PrevBorderStart()
        {
            float uvX = prevBorder.IsReversed ? laneUvRight : laneUvLeft;
            return new Vector2(uvX, prevUvY);
        }

        /// <summary> 道路のPrev側のボーダー線の終了地点のUVです。 </summary>
        public Vector2 PrevBorderEnd()
        {
            float uvX = prevBorder.IsReversed ? laneUvLeft : laneUvRight;
            return new Vector2(uvX, prevUvY);
        }

        /// <summary> 道路の右側Wayの開始地点のUVです。 </summary>
        public Vector2 RightWayStart()
        {
            var uvX = reversedLane ? laneUvLeft : laneUvRight;
            return new Vector2(uvX, prevUvY);
        }

        /// <summary> 道路の右側Wayの終了地点のUVです。 </summary>
        public Vector2 RightWayEnd()
        {
            var uvX = reversedLane ? laneUvLeft : laneUvRight;
            return new Vector2(uvX, nextUvY);
        }

        /// <summary> 道路の左側Wayの開始地点のUVです。 </summary>
        public Vector2 LeftWayStart()
        {
            var uvX = reversedLane ? laneUvRight : laneUvLeft;
            return new Vector2(uvX, prevUvY);
        }
        
        /// <summary> 道路の左側Wayの終了地点のUVです。 </summary>
        public Vector2 LeftWayEnd()
        {
            var uvX = reversedLane ? laneUvRight : laneUvLeft;
            return new Vector2(uvX, nextUvY);
        }
    }
}