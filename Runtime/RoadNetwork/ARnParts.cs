using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public abstract class ARnPartsBase
    {
        [SerializeField]
        private ulong debugId;

        public ulong DebugMyId
        {
            get
            {
                return debugId;
            }
            set
            {
                debugId = value;
            }
        }

        protected ARnPartsBase(ulong id)
        {
            debugId = id;
        }
    }

    [Serializable]
    public abstract class ARnParts<TSelf> : ARnPartsBase where TSelf : ARnParts<TSelf>
    {
        private static ulong counter = 0;

        protected ARnParts()
        : base(counter++)
        {
        }
    }

    public static class ARnPartsEx
    {
        private static readonly Dictionary<Type, string> s_debugIdPrefix = new()
        {
            [typeof(RnRoadBase)] = "Rb",
            [typeof(RnIntersection)] = "In",
            [typeof(RnRoad)] = "Rd",
            [typeof(RnWay)] = "W",
            [typeof(RnLane)] = "Ln",
            [typeof(RnLineString)] = "Ls",
            [typeof(RnPoint)] = "Pt",
            [typeof(RnNeighbor)] = "Nb",
            [typeof(RnTrack)] = "Tr",
            [typeof(RnSideWalk)] = "Sw",
        };

        /// <summary>
        /// self.DebugMyIdをlong型で取得. nullの場合は-1を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long GetDebugMyIdOrDefault(this ARnPartsBase self)
        {
            if (self == null)
                return -1;
            return (long)self.DebugMyId;
        }

        /// <summary>
        /// RnWayは内部のLineStringで比較する
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long GetDebugMyIdOrDefault(this RnWay self)
        {
            return self?.LineString?.GetDebugMyIdOrDefault() ?? -1;
        }

        /// <summary>
        /// DebugIdにパーツタイプを表すプレフィックスを付与して返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string GetDebugLabelOrDefault(this ARnPartsBase self, string prefix = null)
        {
            if (self == null)
                return "null";
            if (prefix.IsNullOrEmpty())
                s_debugIdPrefix.TryGetValue(self.GetType(), out prefix);
            return $"{prefix}[{self.DebugMyId}]";
        }

        /// <summary>
        /// Wayは内部のLineStringで比較することが多いのでIdもそっちを使う
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetDebugIdLabelOrDefault(this RnWay self)
        {
            if (self == null)
                return "null";
            return self.LineString.GetDebugLabelOrDefault();
        }
    }
}