namespace PLATEAU.RoadNetwork.Structure
{
    public class RnSideWalk : ARnParts<RnSideWalk>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        private RnRoadBase parentRoad;

        // 道路と反対側のWay
        private RnWay outsideWay { get; }

        // 道路と同じ側のWay
        private RnWay insideWay { get; }

        // outsideWayとinsideWayの始点を繋ぐWay
        private RnWay startEdgeWay { get; }

        // outsideWayとinsideWayの終点を繋ぐWay
        private RnWay endEdgeWay { get; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public RnRoadBase ParentRoad => parentRoad;


        public RnWay OutsideWay => outsideWay;

        public RnWay InsideWay => insideWay;

        public RnWay StartEdgeWay => startEdgeWay;

        public RnWay EndEdgeWay => endEdgeWay;

        public RnSideWalk() { }

        private RnSideWalk(RnRoadBase parent, RnWay outsideWay, RnWay insideWay, RnWay startEdgeWay, RnWay endEdgeWay)
        {
            this.parentRoad = parent;
            this.outsideWay = outsideWay;
            this.insideWay = insideWay;
            this.startEdgeWay = startEdgeWay;
            this.endEdgeWay = endEdgeWay;
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
        /// <param name="outsideWay"></param>
        /// <param name="insideWay"></param>
        /// <param name="startEdgeWay"></param>
        /// <param name="endEdgeWay"></param>
        /// <returns></returns>
        public static RnSideWalk Create(RnRoadBase parent, RnWay outsideWay, RnWay insideWay, RnWay startEdgeWay, RnWay endEdgeWay)
        {
            var sideWalk = new RnSideWalk(parent, outsideWay, insideWay, startEdgeWay, endEdgeWay);
            parent.AddSideWalk(sideWalk);
            return sideWalk;
        }
    }
}