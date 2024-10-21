using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 交差点の輪郭線を生成します。レーンは結合します。 </summary>
    internal class RnmContourMeshGeneratorIntersectionCombine : IRnmContourMeshGenerator
    {
        public RnmContourMeshList Generate(RnModel model)
        {
            var cMeshes = new RnmContourMeshList();
            foreach (var inter in model.Intersections)
            {
                var targetObj = inter.TargetTran == null ? null : inter.TargetTran.gameObject;
                var contours = new RnmContourMeshGeneratorIntersectionSeparate().GenerateContours(inter);
                var cMesh = new RnmContourMesh(targetObj, contours);
                cMeshes.Add(cMesh);

                // // 交差点の外側に位置するWayをここに列挙します。
                // var outsideWaysCollectors = new IRnWayCollector[]
                // {
                //     new Sidewalk(inter), // 歩道と交差点の歩道の端。歩道から、交差点のカーブ部分が得られます。交差点の歩道の端から、一部隣接する道路の端が得られます。
                //     new CarBorder(inter), // 車道のボーダー。車道と接する部分が得られます。
                //     new NeighborSidewalkEdges(inter), // 隣接する道路の歩道の端。交差点自身の歩道の端では得られない歩道端を隣接道路から取得します。
                //     new OutsideBorderEdges(inter), // 交差点ボーダーのうち外側に位置するもの。歩道がない箇所では代わりにこちらが使われるようにします。
                // };
                //
                // // 交差点ごとの輪郭線を作ります。
                // var targetObj = inter.TargetTran == null ? null : inter.TargetTran.gameObject;
                // var calc = new RnmContourCalculator(RnmMaterialType.CarLane);
                // foreach (var collector in outsideWaysCollectors)
                // {
                //     calc.AddRangeLine(collector.Collect());
                // }
                //
                // var contour = calc.Calculate();
                // var cMesh = new RnmContourMesh(targetObj, contour);
                // cMeshes.Add(cMesh);
            }

            return cMeshes;
        }


        

        /// <summary> 交差点の歩道と、歩道の端を返します </summary>
        private class Sidewalk : IRnWayCollector
        {
            private readonly RnIntersection inter;

            public Sidewalk(RnIntersection inter)
            {
                this.inter = inter;
            }

            public IEnumerable<RnWay> Collect()
            {
                // 歩道の外側
                var walks = inter.SideWalks.Select(sw => sw.OutsideWay);

                // 歩道の端にある線
                var walksEdge =
                    inter.SideWalks
                        .SelectMany(w => new[] { w.StartEdgeWay, w.EndEdgeWay })
                        .Where(w => w != null);

                return walks.Concat(walksEdge);
            }
        }
        

        
    }
}