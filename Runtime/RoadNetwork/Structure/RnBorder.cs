namespace PLATEAU.RoadNetwork.Structure
{
    public class RnBorder
    {
        // 境界辺
        public RnWay EdgeWay { get; set; }

        public RnBorder(RnWay edgeWay)
        {
            EdgeWay = edgeWay;
        }
    }
}