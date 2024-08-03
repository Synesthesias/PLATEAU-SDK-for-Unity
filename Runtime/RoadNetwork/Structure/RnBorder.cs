namespace PLATEAU.RoadNetwork.Structure
{
    public class RnBorder
    {
        // 境界辺
        public RnWay EdgeWay { get; set; }

        // 隣接道路(交差点)
        public RnLane Neighbor { get; set; }

        public RnBorder(RnWay edgeWay, RnLane neighbor)
        {
            EdgeWay = edgeWay;
            Neighbor = neighbor;
        }
    }
}