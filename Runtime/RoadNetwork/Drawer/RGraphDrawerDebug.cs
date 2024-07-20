using PLATEAU.RoadNetwork.Graph;

namespace PLATEAU.RoadNetwork.Drawer
{
    public class RGraphDrawerDebug
    {
        void Draw(RPolygon polygon)
        {
            // Draw polygon
        }

        public void Draw(RGraph graph)
        {
            foreach (var vertex in graph.Vertices)
            {
                foreach (var edge in vertex.Edges)
                {
                    // Draw edge
                }
            }
        }
    }
}