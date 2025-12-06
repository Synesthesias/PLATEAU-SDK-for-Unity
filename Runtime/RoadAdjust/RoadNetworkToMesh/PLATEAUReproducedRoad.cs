using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから生成された道路メッシュであることをシーン上に記録するためのMonoBehaviourです。
    /// </summary>
    public class PLATEAUReproducedRoad : MonoBehaviour
    {
        public ReproducedRoadType roadType;
        [FormerlySerializedAs("sourceRoad")] public RoadReproduceSource reproduceSource;
        public ReproducedRoadDirection roadDirection;

        public void Init(ReproducedRoadType roadTypeArg, RoadReproduceSource sourceRoadArg, ReproducedRoadDirection roadDirectionArg)
        {
            roadType = roadTypeArg;
            reproduceSource = sourceRoadArg;
            roadDirection = roadDirectionArg;
        }

        /// <summary> 値が合致するものをシーンから探します。なければnullを返します。 </summary>
        public static GameObject Find(ReproducedRoadType roadTypeArg, RoadReproduceSource sourceRoadArg, ReproducedRoadDirection roadDirectionArg)
        {
            var comps = FindObjectsOfType<PLATEAUReproducedRoad>(false);
            foreach (var c in comps)
            {
                bool match = c.roadType == roadTypeArg && c.reproduceSource.IsMatch(sourceRoadArg) && c.roadDirection == roadDirectionArg;
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
    
    /// <summary>
    /// 生成された道路について、生成元となった道路オブジェクトを記録するためのクラスです。
    /// </summary>
    [Serializable]
    public class RoadReproduceSource
    {
        /* 優先順位は以下で比較する
        * targetCityObjectKey
        * transform
        * roadNetworkID
        */
        
        // 元のCityObjectのGmlId
        [SerializeField]
        private RnCityObjectGroupKey targetCityObjectKey;

        [SerializeField] private Transform transform;
        [SerializeField] private long roadNetworkID;// 元となるtransformが削除された場合、代わりにこちらを一致判定に使います。不明な場合は-1です。
        
        public Transform Transform => transform;
        
        public RnCityObjectGroupKey TargetCityObjectKey => targetCityObjectKey;

        public RoadReproduceSource(RnRoadBase road)
        {
            // RnCityObjectGroupKeyの設定(タイルメッシュ対応)
            targetCityObjectKey = road?.TargetGroupKeys.FirstOrDefault() ?? new RnCityObjectGroupKey();
            
            // transformの設定
            var targetTran = road?.TargetTrans?.FirstOrDefault();
            transform = targetTran ? targetTran.transform : null;
           
            // roadNetworkIDの設定
            roadNetworkID = road == null ? -1 : (long)road.DebugMyId;
        }

        public string GetName()
        {
            if(TargetCityObjectKey.IsValid)
                return TargetCityObjectKey.GmlId;
            
            if(transform)
                return transform.name;
            return "UnknownRoad";
        }

        public bool IsSourceExists()
        {
            return transform != null;
        }

        public bool IsMatch(RoadReproduceSource other)
        {
            if (other == null) 
                return false;
            
            // TargetCityObjectKey(最小地物分解されたGmlID)で比較(タイル読み込みでも動く)
            if(TargetCityObjectKey.IsValid)
                return TargetCityObjectKey == other.TargetCityObjectKey;
            
            // TargetCityObjectKeyが存在しない昔のデータの場合はtransformで比較(タイルだとうまく動かない)
            if (transform || other.transform)
                return transform == other.transform;
            
            // 元の道路が削除された場合は、代わりに道路ネットワーク上のIDで比較
            return roadNetworkID == other.roadNetworkID && roadNetworkID >= 0;
        }
    }
}