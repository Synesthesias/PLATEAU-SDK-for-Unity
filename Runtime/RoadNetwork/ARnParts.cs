using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public abstract class ARnParts<TSelf>
    {
        private static ulong counter = 0;
        [SerializeField] private ulong debugId;

        public ulong DebugMyId => debugId;

        protected ARnParts()
        {
            debugId = counter++;
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
            [typeof(RnBorder)] = "Bd",
        };

        /// <summary>
        /// self.DebugMyIdをlong型で取得. nullの場合は-1を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long GetDebugMyIdOrDefault<T>(this T self) where T : ARnParts<T>
        {
            if (self == null)
                return -1;
            return (long)self.DebugMyId;
        }


        /// <summary>
        /// self.DebugMyIdをlong型で取得. nullの場合は-1を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long GetDebugMyIdOrDefault<T>(this ARnParts<T> self)
        {
            if (self == null)
                return -1;
            return (long)self.DebugMyId;
        }

        /// <summary>
        /// self.DebugMyIdをlong型で取得. nullの場合は-1を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long GetDebugMyIdOrDefault(this RnRoad self)
        {
            return self.GetDebugMyIdOrDefault<RnRoadBase>();
        }

        /// <summary>
        /// self.DebugMyIdをlong型で取得. nullの場合は-1を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long GetDebugMyIdOrDefault(this RnIntersection self)
        {
            return self.GetDebugMyIdOrDefault<RnRoadBase>();
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
        public static string GetDebugLabelOrDefault<T>(this T self, string prefix = null) where T : ARnParts<T>
        {
            if (self == null)
                return "null";
            if (prefix.IsNullOrEmpty() && s_debugIdPrefix.TryGetValue(self.GetType(), out prefix))
                return $"{prefix}[{self.DebugMyId}]";
            return self.DebugMyId.ToString();
        }

        /// <summary>
        /// Wayは内部のLineStringで比較することが多いのでIdもそっちを使う
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetDebugIdLabelOrDefault(this RnWay self)
        {
            return self.LineString.GetDebugLabelOrDefault("W");
        }
    }
}