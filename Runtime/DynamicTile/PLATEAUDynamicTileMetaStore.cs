using UnityEngine;
using System;
using System.Collections.Generic;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PLATEAUDynamicTileの情報を保持するScriptableObject
    /// </summary>
    [Serializable]
    public class PLATEAUDynamicTileMetaInfo
    {
        [SerializeField]
        private string addressName;
        public string AddressName => addressName;
        
        [SerializeField]
        private Bounds extent;
        public Bounds Extent => extent;

        [SerializeField]
        private int lod;
        public int LOD => lod;

        public PLATEAUDynamicTileMetaInfo(string addressName, Bounds extent, int lod)
        {
            this.addressName = addressName;
            this.extent = extent;
            this.lod = lod;
        }
    }

    public class PLATEAUDynamicTileMetaStore : ScriptableObject
    {
        [SerializeField]
        private List<PLATEAUDynamicTileMetaInfo> tileMetaInfos = new List<PLATEAUDynamicTileMetaInfo>();
        public List<PLATEAUDynamicTileMetaInfo> TileMetaInfos => tileMetaInfos;

        /// <summary>
        /// meta情報を追加します。
        /// </summary>
        /// <param name="addressName"></param>
        /// <param name="extent"></param>
        /// <param name="lod"></param>
        public void AddMetaInfo(string addressName, Bounds extent, int lod)
        {
            if (string.IsNullOrEmpty(addressName))
            {
                Debug.LogWarning("Address name cannot be null or empty.");
                return;
            }

            if (tileMetaInfos.Exists(info => info.AddressName == addressName))
            {
                Debug.LogWarning($"Address '{addressName}' already exists in tile meta infos.");
                return;
            }

            tileMetaInfos.Add(new PLATEAUDynamicTileMetaInfo(addressName, extent, lod));
        }
        
        /// <summary>
        /// meta情報を取得します。
        /// </summary>
        /// <param name="addressName"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public PLATEAUDynamicTileMetaInfo GetMetaInfo(string addressName)
        {
            foreach (var metaInfo in tileMetaInfos)
            {
                if (metaInfo.AddressName == addressName)
                {
                    return metaInfo;
                }
            }
            throw new KeyNotFoundException($"Address '{addressName}' not found in tile meta infos.");
        }

        /// <summary>
        /// meta情報をすべてクリアします。
        /// </summary>
        public void Clear()
        {
            tileMetaInfos.Clear();
        }
    }
} 