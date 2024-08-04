using System;
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
    }
}