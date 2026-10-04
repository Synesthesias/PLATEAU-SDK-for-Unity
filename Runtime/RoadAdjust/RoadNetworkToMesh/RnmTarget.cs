using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから道路モデルを生成する処理において、どの部分を対象にするのかを記述します。
    /// 対象が道路ネットワーク全体か、指定の道路かを切り替えます。
    /// Rrは<see cref="RoadReproducer"/>の略です。
    /// </summary>
    public interface IRrTarget
    {
        public IEnumerable<RnRoadBase> RoadBases();
        public IEnumerable<RnRoad> Roads();
        public IEnumerable<RnIntersection> Intersections();
        public IRrTarget Copy();
        public RnModel Network();
    }

    /// <summary>
    /// <see cref="IRrTarget"/>で、1つの道路モデル全体を対象に取ります。
    /// </summary>
    public class RrTargetModel : IRrTarget

    {
        private RnModel model;

        public RrTargetModel(RnModel model)
        {
            this.model = model;
        }

        public IEnumerable<RnRoadBase> RoadBases()
        {
            return Roads().Cast<RnRoadBase>().Concat(Intersections());
        }

        public IEnumerable<RnRoad> Roads()
        {
            foreach (var r in model.Roads)
            {
                yield return r;
            }
        }

        public IEnumerable<RnIntersection> Intersections()
        {
            foreach (var i in model.Intersections)
            {
                yield return i;
            }
        }

        /// <summary>
        /// 道路ネットワークをディープコピーします。
        /// ただし<see cref="RoadReproducer"/>に使う部分のみコピーします。
        /// </summary>
        public IRrTarget Copy()
        {
            // var serializer = new RoadNetworkSerializer();
            // var copiedNetwork = serializer.Deserialize(serializer.Serialize(model, false)); // 完全コピー

            // ↓ RrRoadNetworkCopier の正しさをチェックしたい場合は、上のコメントアウトを外して完全コピーと結果を比較してください。
            var copiedNetwork = new RrRoadNetworkCopier().Copy(model, out _);
            
            return new RrTargetModel(copiedNetwork);
        }
        
        public RnModel Network() => model;
    }

    /// <summary>
    /// <see cref="IRrTarget"/>で、特定の道路または交差点を対象に取ります。
    /// </summary>
    public class RrTargetRoadBases : IRrTarget
    {
        private RnModel network; // この道路ネットワークのうち、 roadBases に含まれるもののみを対象に取ります。
        private RnRoadBase[] roadBases;
        private readonly bool copyNeighborsOnly;
        
        /// <param name="copyNeighborsOnly">
        /// true なら <see cref="Copy"/> で、対象とその隣接する道路・交差点だけをコピーします。
        /// 対象が少数のとき、ネットワーク全体をコピーするより大幅に速くなります。
        /// メッシュ化の前処理は対象と隣接部分しか参照しないため、生成結果は全体コピーと同じになる想定です。
        /// </param>
        public RrTargetRoadBases(RnModel network, IEnumerable<RnRoadBase> roadBases, bool copyNeighborsOnly = false)
        {
            this.network = network;
            this.roadBases = roadBases.ToArray();
            this.copyNeighborsOnly = copyNeighborsOnly;
        }

        public IEnumerable<RnRoadBase> RoadBases()
        {
            foreach (var rb in roadBases) yield return rb;
        }

        public IEnumerable<RnRoad> Roads()
        {
            foreach (var rb in roadBases)
            {
                if (rb is RnRoad r) yield return r;
            }
        }

        public IEnumerable<RnIntersection> Intersections()
        {
            foreach (var rb in roadBases)
            {
                if (rb is RnIntersection i) yield return i;
            }
        }

        public IRrTarget Copy()
        {
            // var network = new RnModel();
            // foreach (var rb in roadBases)
            // {
            //     network.AddRoadBase(rb);
            // }
            // var serializer = new RoadNetworkSerializer();
            // var copiedNetwork = serializer.Deserialize(serializer.Serialize(network, true));
            // var copiedRoadBases = copiedNetwork.Roads.Cast<RnRoadBase>().Concat(copiedNetwork.Intersections);
            
            // ↓ RrRoadNetworkCopier の正しさをチェックしたいときは、上のコメントアウトを外して完全コピーの結果と比較してください。
            Dictionary<RnRoadBase, RnRoadBase> roadSrcToDst;
            RnModel copiedNetwork;
            if (copyNeighborsOnly)
            {
                var copyTargets = roadBases
                    .Concat(roadBases.SelectMany(rb => rb.GetNeighborRoads()))
                    .Where(rb => rb != null)
                    .Distinct();
                copiedNetwork = new RrRoadNetworkCopier().CopyPartial(copyTargets, out roadSrcToDst);
            }
            else
            {
                copiedNetwork = new RrRoadNetworkCopier().Copy(network, out roadSrcToDst);
            }
            var copiedRoadBases = roadBases.Select(src => roadSrcToDst[src]);
            
            return new RrTargetRoadBases(copiedNetwork, copiedRoadBases);
        }

        public RnModel Network() => network;
    }
}