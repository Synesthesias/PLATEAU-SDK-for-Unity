using PLATEAU.RoadNetwork.Structure;
using System;
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
        public ReproducedRoadDirection roadDirection;

        public void Init(ReproducedRoadType roadTypeArg, Transform sourceRoadArg, ReproducedRoadDirection roadDirectionArg)
        {
            roadType = roadTypeArg;
            sourceRoad = sourceRoadArg;
            roadDirection = roadDirectionArg;
        }

        /// <summary> 値が合致するものをシーンから探します。なければnullを返します。 </summary>
        public static GameObject Find(ReproducedRoadType roadTypeArg, Transform sourceRoadArg, ReproducedRoadDirection roadDirectionArg)
        {
            var comps = FindObjectsOfType<PLATEAUReproducedRoad>(false);
            foreach (var c in comps)
            {
                bool match = c.roadType == roadTypeArg && c.sourceRoad == sourceRoadArg && c.roadDirection == roadDirectionArg;
                if (match)
                {
                    return c.gameObject;
                }
            }

            return null;
        }
    }

    /// <summary> 道路の両端に配置される道路標示（横断歩道）において、道路のどちら側に配置されたかを示します。どちらでもない道路標示はNoneとなります。 </summary>
    public enum ReproducedRoadDirection
    {
        None, Prev, Next
    }

    /// <summary> 道路ネットワークから何を生成したかです </summary>
    public enum ReproducedRoadType
    { 
        RoadMesh,
        LaneLineAndArrow,
        Crosswalk
    }
    
    public static class ReproducedRoadTypeExtension
    {
        public static string ToGameObjName(this ReproducedRoadType type)
        {
            switch (type)
            {
                case ReproducedRoadType.RoadMesh:
                    return "Road";
                case ReproducedRoadType.LaneLineAndArrow:
                    return "LaneArrow";
                case ReproducedRoadType.Crosswalk:
                    return "Crosswalk";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}