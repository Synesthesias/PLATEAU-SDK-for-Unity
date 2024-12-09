using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから道路モデルを生成する処理において、どの部分を対象にするのかを記述します。
    /// 対象が道路ネットワーク全体か、指定の道路かを切り替えます。
    /// RnmはRoad Network to Meshの略です。
    /// </summary>
    public interface IRnmTarget
    {
        public IEnumerable<RnRoadBase> RoadBases();
        public IEnumerable<RnRoad> Roads();
        public IEnumerable<RnIntersection> Intersections();
        public IRnmTarget Copy();
        public RnModel Network();
    }

    /// <summary>
    /// RnmTargetで、1つの道路モデル全体を対象に取ります。
    /// </summary>
    public class RnmTargetModel : IRnmTarget

    {
        private RnModel model;

        public RnmTargetModel(RnModel model)
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

        public IRnmTarget Copy()
        {
            var serializer = new RoadNetworkSerializer();
            var copiedNetwork = serializer.Deserialize(serializer.Serialize(model));
            return new RnmTargetModel(copiedNetwork);
        }
        
        public RnModel Network() => model;
    }

    /// <summary>
    /// RnmTargetで、特定の道路または交差点を対象に取ります。
    /// </summary>
    public class RnmTargetRoadBases : IRnmTarget
    {
        private RnModel parentModel; // この道路ネットワークのうち、 roadBases に含まれるもののみを対象に取ります。
        private RnRoadBase[] roadBases;
        
        public RnmTargetRoadBases(RnModel parentModel, IEnumerable<RnRoadBase> roadBases)
        {
            this.parentModel = parentModel;
            this.roadBases = roadBases.ToArray();
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

        /// <summary>
        /// ターゲットだけコピーします。
        /// <see cref="parentModel"/>はコピーしません。
        /// </summary>
        public IRnmTarget Copy()
        {
            var model = new RnModel();
            foreach (var rb in roadBases)
            {
                model.AddRoadBase(rb);
            }

            var serializer = new RoadNetworkSerializer();
            var copiedNetwork = serializer.Deserialize(serializer.Serialize(model));
            var copiedRoadBases = copiedNetwork.Roads.Cast<RnRoadBase>().Concat(copiedNetwork.Intersections);
            return new RnmTargetRoadBases(parentModel, copiedRoadBases);

        }

        public RnModel Network() => parentModel;
    }
}