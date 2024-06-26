using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public abstract class ARoadNetworkParts<TSelf>
    {
        private static ulong counter = 0;
        [SerializeField]
        private ulong debugId;

        public ulong DebugMyId => debugId;

        protected ARoadNetworkParts()
        {
            debugId = counter++;
        }
    }
}