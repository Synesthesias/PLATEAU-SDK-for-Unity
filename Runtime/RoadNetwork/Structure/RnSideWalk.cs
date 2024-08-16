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

        private RnLineString line { get; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public RnRoadBase ParentRoad => parentRoad;

        public RnLineString Line => line;

        public RnSideWalk() { }

        private RnSideWalk(RnRoadBase parent, RnLineString line)
        {
            this.parentRoad = parent;
            this.line = line;
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
        /// <param name="line"></param>
        /// <returns></returns>
        public static RnSideWalk Create(RnRoadBase parent, RnLineString line)
        {
            var sideWalk = new RnSideWalk(parent, line);
            parent.AddSideWalk(sideWalk);
            return sideWalk;
        }
    }
}