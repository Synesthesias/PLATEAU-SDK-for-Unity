using PLATEAU.DynamicTile;
using UnityEngine;
using System.Collections.Generic;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PLATEAUDynamicTileのリストを保持するScriptableObject
    /// </summary>
    public class PLATEAUDynamicTileCollection : ScriptableObject
    {
        [SerializeField]
        private List<PLATEAUDynamicTile> tileList = new List<PLATEAUDynamicTile>();
        public List<PLATEAUDynamicTile> TileList => tileList;

        public void AddTile(PLATEAUDynamicTile tileData)
        {
            if (tileData != null)
            {
                tileList.Add(tileData);
            }
        }

        public PLATEAUDynamicTile GetTile(string address)
        {
            return tileList.Find(tile => tile.Address == address);
        }

        public void Clear()
        {
            tileList.Clear();
        }
    }
} 