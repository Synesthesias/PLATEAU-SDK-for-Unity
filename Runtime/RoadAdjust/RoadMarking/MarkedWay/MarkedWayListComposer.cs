using PLATEAU.RoadAdjust.RoadNetworkToMesh;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークをもとに、車線を引く対象となる<see cref="MarkedWay"/>のリスト<see cref="MarkedWayList"/>を生成します。
    /// </summary>
    public class MarkedWayListComposer : IMarkedWayListComposer
    {
        /// <summary> 道路ネットワークから、車線を引く対象となる<see cref="MarkedWayList"/>を収集します。 </summary>
        public MarkedWayList ComposeFrom(IRrTarget target)
        {
            // ここに、どの線を追加したいか記述します。
            var composers = new IMarkedWayListComposer[]
            {
                new MCLaneLine(), // 車線の間の線のうち、センターラインでないもの。
                new MCShoulderLine(), // 路側帯線、すなわち歩道と車道の間の線。
                new MCCenterLine(), // センターライン
                new MCIntersection() // 交差点の線
            };
            
            var ret = new MarkedWayList();
            foreach (var composer in composers)
            {
                ret.AddRange(composer.ComposeFrom(target));
            }
            return ret;
        }
    }

    internal interface IMarkedWayListComposer
    {
        public MarkedWayList ComposeFrom(IRrTarget target);
    }
    
}