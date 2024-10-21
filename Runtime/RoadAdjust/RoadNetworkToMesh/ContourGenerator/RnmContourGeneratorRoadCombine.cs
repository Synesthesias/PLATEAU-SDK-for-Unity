using PLATEAU.RoadNetwork.Structure;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 道路の輪郭線を生成します。道路レーンはまとめます。 </summary>
    internal class RnmContourMeshGeneratorRoadCombine : IRnmContourMeshGenerator
    {

        public RnmContourMeshList Generate(RnModel model)
        {
            var cMeshes = new RnmContourMeshList();
            // 道路ごとに輪郭を追加します。
            foreach (var road in model.Roads)
            {
                var targetObj = road.TargetTran == null ? null : road.TargetTran.gameObject;

                var carLanes = new RnmContourMeshGeneratorCarLane().GenerateCarLane(road);
                var sideWalks = new RnmContourMeshGeneratorSidewalk().GenerateSidewalks(road);
                var cMesh = new RnmContourMesh(targetObj, carLanes.Concat(sideWalks));
                cMeshes.Add(cMesh);
            }

            return cMeshes;            
        }
        
    }
}