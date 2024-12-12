using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから生成された道路メッシュであることをシーン上に記録するためのMonoBehaviourです。
    /// </summary>
    public class PLATEAUReproducedRoad : MonoBehaviour
    {
        public ReproducedRoadType roadType;
        public Transform sourceRoad;

        public void Init(ReproducedRoadType roadTypeArg, Transform sourceRoadArg)
        {
            roadType = roadTypeArg;
            sourceRoad = sourceRoadArg;
        }

        /// <summary> 値が合致するものをシーンから探します。なければnullを返します。 </summary>
        public static GameObject Find(ReproducedRoadType roadTypeArg, Transform sourceRoadArg)
        {
            var comps = FindObjectsOfType<PLATEAUReproducedRoad>(false);
            foreach (var c in comps)
            {
                bool match = c.roadType == roadTypeArg && c.sourceRoad == sourceRoadArg;
                if (match)
                {
                    return c.gameObject;
                }
            }

            return null;
        }
    }

    public enum ReproducedRoadType
    {
        RoadMesh,
        RoadMarking
    }
}