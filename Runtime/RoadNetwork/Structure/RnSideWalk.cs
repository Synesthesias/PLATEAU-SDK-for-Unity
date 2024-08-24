using PLATEAU.CityInfo;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork.Structure
{
    public class RnSideWalk : ARnParts<RnSideWalk>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        private RnRoadBase parentRoad;

        private RnWay way { get; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public RnRoadBase ParentRoad => parentRoad;

        public RnWay Way => way;

        public RnSideWalk() { }

        private RnSideWalk(RnRoadBase parent, RnWay way)
        {
            this.parentRoad = parent;
            this.way = way;
        }

        public void SetParent(RnRoadBase parent)
        {
            if (parent != null)
                parent.RemoveSideWalk(this);
            this.parentRoad = parent;
        }

        /// <summary>
        /// 歩道作成
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="way"></param>
        /// <returns></returns>
        public static RnSideWalk Create(RnRoadBase parent, RnWay way)
        {
            var sideWalk = new RnSideWalk(parent, way);
            parent.AddSideWalk(sideWalk);
            return sideWalk;
        }
    }
}