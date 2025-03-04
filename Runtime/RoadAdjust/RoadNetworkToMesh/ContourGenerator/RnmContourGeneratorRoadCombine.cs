using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 道路の輪郭線を生成します。道路レーンはまとめます。 </summary>
    internal class RnmContourGeneratorRoadCombine : IRnmContourGenerator
    {

        public RnmContourMeshList Generate(IRrTarget target)
        {
            var cMeshes = new RnmContourMeshList();
            // 道路ごとに輪郭を追加します。
            foreach (var road in target.Roads())
            {
                var targetObjs = new []{new RoadReproduceSource(road)};

                var carLanes = new RnmContourGeneratorCarLane().GenerateCarLane(road);
                var sideWalks = new RnmContourGeneratorSidewalk().GenerateSidewalks(road);
                var medianLanes = new RnmContourGeneratorMedianLane().GenerateMedianLane(road);
                var cMesh = new RnmContourMesh(targetObjs, carLanes.Concat(sideWalks).Concat(medianLanes));
                cMeshes.Add(cMesh);
            }

            return cMeshes;            
        }
        
    }
}