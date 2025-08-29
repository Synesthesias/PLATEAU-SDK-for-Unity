using UnityEngine;
using System;
using System.Collections.Generic;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Addressに紐づく情報を保持するデータクラス。
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

        [SerializeField]
        private int zoomLevel;
        public int ZoomLevel => zoomLevel;

        public PLATEAUDynamicTileMetaInfo(string addressName, Bounds extent, int lod, int zoomLevel)
        {
            this.addressName = addressName;
            this.extent = extent;
            this.lod = lod;
            this.zoomLevel = zoomLevel;
        }
    }

    /// <summary>
    /// Addressに紐づく情報のリストを保持するScriptableObject。
    /// </summary>
    public class PLATEAUDynamicTileMetaStore : ScriptableObject
    {
        [SerializeField]
        private List<PLATEAUDynamicTileMetaInfo> tileMetaInfos = new List<PLATEAUDynamicTileMetaInfo>();
        public List<PLATEAUDynamicTileMetaInfo> TileMetaInfos => tileMetaInfos;

        // タイル群で共有される参照点（CityImportConfig.ReferencePoint 相当）
        [SerializeField]
        private Vector3 referencePoint;
        public Vector3 ReferencePoint
        {
            get => referencePoint;
            internal set => referencePoint = value;
        }

        /// <summary>
        /// meta情報を追加します。
        /// </summary>
        /// <param name="addressName"></param>
        /// <param name="extent"></param>
        /// <param name="lod"></param>
        public void AddMetaInfo(string addressName, Bounds extent, int lod, int zoomLevel)
        {
            if (string.IsNullOrEmpty(addressName))
            {
                Debug.LogWarning("アドレス名がnullまたは空です。無効なアドレス名です。");
                return;
            }

            if (tileMetaInfos.Exists(info => info.AddressName == addressName))
            {
                Debug.LogWarning($"{addressName}がすでに存在します。重複を避けてください。");
                return;
            }

            tileMetaInfos.Add(new PLATEAUDynamicTileMetaInfo(addressName, extent, lod, zoomLevel));
        }
        
        /// <summary>
        /// meta情報を取得します。
        /// </summary>
        /// <param name="addressName"></param>
        /// <returns></returns>
        public PLATEAUDynamicTileMetaInfo GetMetaInfo(string addressName)
        {
            foreach (var metaInfo in tileMetaInfos)
            {
                if (metaInfo.AddressName == addressName)
                {
                    return metaInfo;
                }
            }
            Debug.LogWarning($"{addressName}が見つかりません。");
            return null;
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